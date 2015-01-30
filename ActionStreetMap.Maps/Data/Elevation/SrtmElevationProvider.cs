using System;
using System.Linq;
using ActionStreetMap.Core;
using ActionStreetMap.Core.Elevation;
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
        private readonly object _lockObj = new object();
        private readonly IFileSystemService _fileSystemService;
        private const string PathKey = "";

        private string _srtmServer;
        private string _srtmMapPath;
        private SrtmDownloader _downloader;

        //arc seconds per pixel (3 equals cca 90m)
        private int _secondsPerPx;
        private int _totalPx;
        private int _offset;
        private string _dataDirectory;

        //default never valid
        private int _srtmLat = 255;
        private int _srtmLon = 255;

        private byte[] _hgtData;

        /// <summary>
        ///     Trace.
        /// </summary>
        [Dependency]
        public ITrace Trace { get; set; }

        /// <summary>
        ///     Creates SRTM specific implementation of <see cref="IElevationProvider"/>
        /// </summary>
        /// <param name="fileSystemService">File system service.</param>
        [Dependency]
        public SrtmElevationProvider(IFileSystemService fileSystemService)
        {
            _fileSystemService = fileSystemService;
        }

        /// <inheritdoc />
        public bool HasElevation(double latitude, double longitude)
        {
            return _fileSystemService.Exists(GetFilePath((int) latitude, (int) longitude));
        }

        /// <inheritdoc />
        public IObservable<Unit> Download(double latitude, double longitude)
        {
            return _downloader.Download(new GeoCoordinate(latitude, longitude))
                    .SelectMany(bytes => 
                    {
                        _hgtData = CompressionUtils.Unzip(bytes).Single().Value;
                        _srtmLat = (int)latitude;
                        _srtmLon = (int)longitude;
                        return Observable.Return<Unit>(Unit.Default);
                    });
        }

        /// <inheritdoc />
        public float GetElevation(double latitude, double longitude)
        {
            int latDec = (int) latitude;
            int lonDec = (int) longitude;

            float secondsLat = (float) (latitude - latDec)*3600;
            float secondsLon = (float) (longitude - lonDec)*3600;

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

        private void LoadTile(int latDec, int lonDec)
        {
            if (_srtmLat != latDec || _srtmLon != lonDec)
            {
                lock (_lockObj)
                {
                    if (_srtmLat == latDec && _srtmLon == lonDec)
                        return;

                    var filePath = GetFilePath(latDec, lonDec);

                    Trace.Output(String.Format(Strings.LoadElevationFrom, filePath));

                    if (!_fileSystemService.Exists(filePath))
                        throw new Exception(String.Format(Strings.CannotFindSrtmData, filePath));

                    _hgtData = _fileSystemService.ReadBytes(filePath);

                    switch (_hgtData.Length)
                    {
                        case 1201*1201*2: // SRTM-3
                            _totalPx = 1201;
                            _secondsPerPx = 3;
                            break;
                        case 3601*3601*2: // SRTM-1
                            _totalPx = 3601;
                            _secondsPerPx = 1;
                            break;
                        default:
                            throw new ArgumentException("Invalid file size.", filePath);
                    }
                    // NOTE this is just perfromance optimization
                    _offset = (_totalPx*_totalPx - _totalPx)*2;
                    _srtmLat = latDec;
                    _srtmLon = lonDec;
                }
            }
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
            var path = configSection.GetString(PathKey, null);
            _dataDirectory = path;

            _srtmMapPath = configSection.GetString("map", null);
            _srtmServer = configSection.GetString("server", @"http://dds.cr.usgs.gov/srtm/version2_1/SRTM3");
            _downloader = new SrtmDownloader(_srtmServer, _srtmMapPath, _fileSystemService, Trace);
        }
    }
}
