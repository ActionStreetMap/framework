using System;
using System.Collections.Generic;
using System.Linq;
using ActionStreetMap.Core;
using ActionStreetMap.Core.Utils;
using ActionStreetMap.Infrastructure.Config;
using ActionStreetMap.Infrastructure.Dependencies;
using ActionStreetMap.Infrastructure.Diagnostic;
using ActionStreetMap.Infrastructure.IO;
using ActionStreetMap.Infrastructure.IO.Compression;
using ActionStreetMap.Infrastructure.Reactive;

namespace ActionStreetMap.Maps.Data.Elevation
{
    /// <summary>
    ///     Implementation of <see cref="IElevationProvider"/> which uses SRTM data files.
    /// </summary>
    public class SrtmElevationProvider : IElevationProvider, IConfigurable
    {
        private const string LogTag = "mapdata.srtm";

        private readonly object _lockObj = new object();
        private readonly IFileSystemService _fileSystemService;
        private readonly ITrace _trace;

        private string _srtmServer;
        private string _srtmSchemaPath;
        private SrtmDownloader _downloader;

        //arc seconds per pixel (3 equals cca 90m)
        private int _secondsPerPx;
        private int _totalPx;
        private int _offset;
        private string _dataDirectory;
        private GeoCoordinate _relativeNullPoint;

        //default never valid
        private int _srtmLat = 255;
        private int _srtmLon = 255;

        private byte[] _hgtData;

        /// <summary>
        ///     Creates SRTM specific implementation of <see cref="IElevationProvider"/>
        /// </summary>
        /// <param name="fileSystemService">File system service.</param>
        /// <param name="trace">Trace.</param>
        [Dependency]
        public SrtmElevationProvider(IFileSystemService fileSystemService, ITrace trace)
        {
            _fileSystemService = fileSystemService;
            _trace = trace;
        }

        /// <inheritdoc />
        public bool HasElevation(BoundingBox bbox)
        {
            bool hasAllElevationData = true;
            CheckBoundingBox(bbox, (coordinate) => hasAllElevationData = false);
            return hasAllElevationData;
        }

        /// <inheritdoc />
        public IObservable<Unit> Download(BoundingBox bbox)
        {
            var tasks = new List<IObservable<Unit>>();
            CheckBoundingBox(bbox, (coordinate) => tasks.Add(Download(coordinate)));
            if (tasks.Any())
                return tasks.WhenAll().AsCompletion();
            return Observable.Empty<Unit>();
        }

        private void CheckBoundingBox(BoundingBox bbox, Action<GeoCoordinate> action)
        {
            var minLat = (int)bbox.MinPoint.Latitude;
            var minLon = (int)bbox.MinPoint.Longitude;

            var maxLat = (int)bbox.MaxPoint.Latitude;
            var maxLon = (int)bbox.MaxPoint.Longitude;

            var latDiff = maxLat - minLat;
            var lonDiff = maxLon - minLon;
            var minPoint = bbox.MinPoint;
            for (int j = 0; j <= latDiff; j++)
                for (int i = 0; i <= lonDiff; i++)
                {
                    var coordinate = new GeoCoordinate(minPoint.Latitude + j, minPoint.Longitude + i);
                    if (!HasElevation(coordinate))
                        action(coordinate);
                }
        }

        private bool HasElevation(GeoCoordinate coordinate)
        {
            var latitude = (int)coordinate.Latitude;
            var longitude = (int)coordinate.Longitude;
            return (_srtmLat == (latitude) && _srtmLon == (longitude)) ||
                _fileSystemService.Exists(GetFilePath(latitude, longitude));
        }

        private IObservable<Unit> Download(GeoCoordinate coordinate)
        {
            _trace.Info(LogTag, "downloading data for {0}", coordinate);
            return _downloader.Download(coordinate)
                    .SelectMany(bytes =>
                    {
                        _trace.Info(LogTag, "downloaded {0} bytes", bytes.Length);
                        _hgtData = CompressionUtils.Unzip(bytes).Single().Value;
                        InitData((int)coordinate.Latitude, (int)coordinate.Longitude);
                        // store data to disk
                        var path = GetFilePath(_srtmLat, _srtmLon);
                        _trace.Info(LogTag, "storing as {0}", path);
                        using (var stream = _fileSystemService.WriteStream(path))
                            stream.Write(_hgtData, 0, _hgtData.Length);

                        return Observable.Return<Unit>(Unit.Default);
                    });
        }

        /// <inheritdoc />
        public float GetElevation(GeoCoordinate coordinate)
        {
            int latDec = (int)coordinate.Latitude;
            int lonDec = (int)coordinate.Longitude;

            float secondsLat = (float)(coordinate.Latitude - latDec) * 3600;
            float secondsLon = (float)(coordinate.Longitude - lonDec) * 3600;

            LoadTile(latDec, lonDec);

            // load tile
            //X coresponds to x/y values,
            //everything easter/norhter (< S) is rounded to X.
            //
            //  y   ^
            //  3   |       |   S
            //      +-------+-------
            //  0   |   X   |
            //      +-------+-------->
            // (sec)    0        3   x  (lon)

            //both values are [0; totalPx - 1] (totalPx reserved for interpolating)
            int y = (int) (secondsLat/_secondsPerPx);
            int x = (int) (secondsLon/_secondsPerPx);

            //get norther and easter points
            var height2 = ReadPx(y, x);
            var height0 = ReadPx(y + 1, x);
            var height3 = ReadPx(y, x + 1);
            var height1 = ReadPx(y + 1, x + 1);

            //ratio where X lays
            float dy = (secondsLat%_secondsPerPx)/_secondsPerPx;
            float dx = (secondsLon%_secondsPerPx)/_secondsPerPx;

            // Bilinear interpolation
            // h0------------h1
            // |
            // |--dx-- .
            // |       |
            // |      dy
            // |       |
            // h2------------h3   

            return height0*dy*(1 - dx) +
                   height1*dy*(dx) +
                   height2*(1 - dy)*(1 - dx) +
                   height3*(1 - dy)*dx;
        }

        /// <inheritdoc />
        public float GetElevation(MapPoint point)
        {
            var geoCoordinate = GeoProjection.ToGeoCoordinate(_relativeNullPoint, point);
            return GetElevation(geoCoordinate);
        }

        /// <inheritdoc />
        public void SetNullPoint(GeoCoordinate coordinate)
        {
            _relativeNullPoint = coordinate;
        }

        private void LoadTile(int latDec, int lonDec)
        {
            if (_srtmLat != latDec || _srtmLon != lonDec)
            {
                lock (_lockObj)
                {
                    if (_srtmLat == latDec && _srtmLon == lonDec)
                        return;

                    var filePath = GetFilePath(latDec, lonDec);

                    _trace.Info("mapdata.srtm", Strings.LoadElevationFrom, filePath);

                    if (!_fileSystemService.Exists(filePath))
                        throw new Exception(String.Format(Strings.CannotFindSrtmData, filePath));

                    _hgtData = _fileSystemService.ReadBytes(filePath);
                    InitData(latDec, lonDec);
                }
            }
        }

        private void InitData(int latDec, int lonDec)
        {
            switch (_hgtData.Length)
            {
                case 1201 * 1201 * 2: // SRTM-3
                    _totalPx = 1201;
                    _secondsPerPx = 3;
                    break;
                case 3601 * 3601 * 2: // SRTM-1
                    _totalPx = 3601;
                    _secondsPerPx = 1;
                    break;
                default:
                    throw new ArgumentException("Invalid file size.");
            }
            // NOTE this is just perfromance optimization
            _offset = (_totalPx * _totalPx - _totalPx) * 2;
            _srtmLat = latDec;
            _srtmLon = lonDec;
        }

        // NOTE inline instruction would be nice here, but current CLR version doesn't support it
        private int ReadPx(int y, int x)
        {
            var pos = _offset + 2*(x - _totalPx*y);
            return _hgtData[pos] << 8 | _hgtData[pos + 1];
        }

        private string GetFilePath(int latDec, int lonDec)
        {
            return String.Format("{0}/{1}{2:00}{3}{4:000}.hgt", _dataDirectory,
                     latDec > 0 ? 'N' : 'S', Math.Abs(latDec),
                     lonDec > 0 ? 'E' : 'W', Math.Abs(lonDec));
        }

        /// <inheritdoc />
        public void Configure(IConfigSection configSection)
        {
            _dataDirectory = configSection.GetString("local", null);

            _srtmSchemaPath = configSection.GetString("remote.schema", null);
            _srtmServer = configSection.GetString("remote.server", @"http://dds.cr.usgs.gov/srtm/version2_1/SRTM3");
            _downloader = new SrtmDownloader(_srtmServer, _srtmSchemaPath, _fileSystemService, _trace);
        }
    }
}
