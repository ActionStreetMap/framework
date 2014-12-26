using System;
using ActionStreetMap.Infrastructure.Config;
using ActionStreetMap.Infrastructure.Dependencies;
using ActionStreetMap.Infrastructure.Diagnostic;
using ActionStreetMap.Infrastructure.IO;

namespace ActionStreetMap.Core.Elevation
{
    /// <summary>
    ///     Implementation of <see cref="IElevationProvider"/> which uses SRTM data files.
    /// </summary>
    public class SrtmElevationProvider: IElevationProvider, IConfigurable
    {
        private readonly IFileSystemService _fileSystemService;
        private const string PathKey = "";

        //arc seconds per pixel (3 equals cca 90m)
        private int _secondsPerPx;
        private int _totalPx;
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
        public float GetElevation(GeoCoordinate coordinate)
        {
            return GetElevation(coordinate.Latitude, coordinate.Longitude);
        }

        /// <inheritdoc />
        public float GetElevation(double latitude, double longitude)
        {
            int latDec = (int)latitude;
            int lonDec = (int)longitude;

            double secondsLat = (latitude - latDec) * 60 * 60;
            double secondsLon = (longitude - lonDec) * 60 * 60;

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
            int y = (int)(secondsLat / _secondsPerPx);
            int x = (int)(secondsLon / _secondsPerPx);

            //get norther and easter points
            var height2 = ReadPx(y, x);
            var height0 = ReadPx(y + 1, x);
            var height3 = ReadPx(y, x + 1);
            var height1 = ReadPx(y + 1, x + 1);

            //ratio where X lays
            double dy = (secondsLat % _secondsPerPx) / _secondsPerPx;
            double dx = (secondsLon % _secondsPerPx) / _secondsPerPx;

            // Bilinear interpolation
            // h0------------h1
            // |
            // |--dx-- .
            // |       |
            // |      dy
            // |       |
            // h2------------h3   
            return (float)(height0 * dy * (1 - dx) +
                            height1 * dy * (dx) +
                            height2 * (1 - dy) * (1 - dx) +
                            height3 * (1 - dy) * dx);
        }

        private void LoadTile(int latDec, int lonDec)
        {
            if (_srtmLat != latDec || _srtmLon != lonDec)
            {
                _srtmLat = latDec;
                _srtmLon = lonDec;

                var filePath = String.Format("{0}/{1}{2:00}{3}{4:000}.hgt", _dataDirectory,
                    latDec > 0 ? 'N' : 'S', Math.Abs(latDec),
                    lonDec > 0 ? 'E' : 'W', Math.Abs(lonDec));

                Trace.Output(String.Format(Strings.LoadElevationFrom, filePath));

                if (!_fileSystemService.Exists(filePath))
                    throw new Exception(String.Format(Strings.CannotFindSrtmData, filePath));

                _hgtData = _fileSystemService.ReadBytes(filePath);

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
                        throw new ArgumentException("Invalid file size.", filePath);
                }
            }
        }

        private int ReadPx(int y, int x)
        {
            int row = (_totalPx - 1) - y;
            int col = x;
            int pos = (row * _totalPx + col) * 2;
            return (_hgtData[pos]) << 8 | _hgtData[pos + 1];
        }

        /// <inheritdoc />
        public void Configure(IConfigSection configSection)
        {
            var path = configSection.GetString(PathKey);
            _dataDirectory = path;
        }
    }
}
