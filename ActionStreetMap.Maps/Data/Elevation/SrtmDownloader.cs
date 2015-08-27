using System;
using System.Collections.Generic;
using ActionStreetMap.Core;
using ActionStreetMap.Infrastructure.Diagnostic;
using ActionStreetMap.Infrastructure.IO;
using ActionStreetMap.Infrastructure.Reactive;

namespace ActionStreetMap.Maps.Data.Elevation
{
    /// <summary> Downloads SRTM data from NASA server. </summary>
    internal class SrtmDownloader
    {
        private const string LogTag = "mapdata.srtm";
        private readonly IFileSystemService _fileSystemService;
        private readonly ITrace _trace;
        private readonly string _server;
        private readonly string _schemaPath;
        private static readonly Dictionary<int, string> ContinentMap = new Dictionary<int, string>
        {
            {0, "Eurasia"},
            {1, "South_America"},
            {2, "Africa"},
            {3, "North_America"},
            {4, "Australia"},
            {5, "Islands"},
        };

        /// <summary> Creates instance of <see cref="SrtmDownloader"/>. </summary>
        /// <param name="schemaPath">Uri schema path.</param>
        /// <param name="fileSystemService">File system service.</param>
        /// <param name="server">Server.</param>
        /// <param name="trace">Trace.</param>
        public SrtmDownloader(string server, string schemaPath, IFileSystemService fileSystemService, ITrace trace)
        {
            _server = server;
            _schemaPath = schemaPath;
            _fileSystemService = fileSystemService;
            _trace = trace;
        }

        #region Public methods

        /// <summary> Download SRTM data for given coordinate. </summary>
        /// <param name="coordinate">Coordinate.</param>
        /// <returns>Stream.</returns>
        public IObservable<byte[]> Download(GeoCoordinate coordinate)
        {
            var prefix = GetFileNamePrefix(coordinate);
            foreach (var line in _fileSystemService.ReadText(_schemaPath).Split('\n'))
            {
                if (line.StartsWith(prefix))
                {
                    var parameters = line.Split(' ');
                    // NOTE some of files miss exptension point between name and .hgt.zip
                    var url = String.Format("{0}/{1}/{2}", _server, ContinentMap[int.Parse(parameters[1])],
                        parameters[1].EndsWith("zip") ? "" : parameters[0] + ".hgt.zip");
                    _trace.Warn(LogTag, String.Format("Downloading SRTM data from {0}", url));
                    return ObservableWWW.GetAndGetBytes(url).Take(1);
                }
            }
            return Observable.Throw<byte[]>(new ArgumentException(
                String.Format("Cannot find {0} on {1}", prefix, _schemaPath)));
        }

        /// <summary> Downloads SRTM for given bounding box. </summary>
        /// <param name="boundingBox"></param>
        /// <returns>Streams</returns>
        public IObservable<byte[]> Download(BoundingBox boundingBox)
        {
            return Observable.Throw<byte[]>(new NotImplementedException());
        }

        #endregion

        private string GetFileNamePrefix(GeoCoordinate coordinate)
        {
            return String.Format("{0}{1:00}{2}{3:000}",
                coordinate.Latitude > 0 ? 'N' : 'S', Math.Abs((int)coordinate.Latitude),
                coordinate.Longitude > 0 ? 'E' : 'W', Math.Abs((int)coordinate.Longitude));
        }
    }
}
