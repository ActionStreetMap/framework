using System;
using System.Collections.Generic;
using ActionStreetMap.Core;
using ActionStreetMap.Infrastructure.Config;
using ActionStreetMap.Infrastructure.IO;
using ActionStreetMap.Infrastructure.Reactive;

namespace ActionStreetMap.Explorer.Downloaders
{
    /// <summary> Downloads SRTM data from NASA server. </summary>
    public class SrtmDownloader: IConfigurable
    {
        private readonly IFileSystemService _fileSystemService;
        private string _srtmMapPath;
        private static readonly Dictionary<int, string> ContinentMap = new Dictionary<int, string>()
        {
            {0, "Eurasia"},
            {1, "South_America"},
            {2, "Africa"},
            {3, "North_America"},
            {4, "Australia"},
            {5, "Islands"},
        };

        /// <summary> Creates instance of <see cref="SrtmDownloader"/>. </summary>
        /// <param name="fileSystemService">File system service.</param>
        public SrtmDownloader(IFileSystemService fileSystemService)
        {
            _fileSystemService = fileSystemService;
        }

        /// <summary> Download SRTM data for given coordinate. </summary>
        /// <param name="coordinate">Coordinate.</param>
        /// <returns>Stream.</returns>
        public IObservable<byte[]> Download(GeoCoordinate coordinate)
        {
            var prefix = GetFileNamePrefix(coordinate);
            foreach (var line in _fileSystemService.ReadText(_srtmMapPath).Split('\n'))
            {
                if (line.StartsWith(prefix))
                {
                    var parameters = line.Split(' ');
                    // NOTE some of files miss exptension point between name and .hgt.zip
                    var url = String.Format("{0}/{1}/{2}", _srtmMapPath, ContinentMap[int.Parse(parameters[2])],
                        parameters[1].EndsWith("zip") ? "" : parameters[1] + "hgt.zip");
                    return ObservableWWW.GetAndGetBytes(url);
                }
            }
            return Observable.Throw<byte[]>(new ArgumentException(
                String.Format("Cannot find {0} on {1}", prefix, _srtmMapPath)));
        }

        /// <summary> Downloads SRTM for given boudning box. </summary>
        /// <param name="boundingBox"></param>
        /// <returns>Streams</returns>
        public IObservable<byte[]> Download(BoundingBox boundingBox)
        {
            return Observable.Throw<byte[]>(new NotImplementedException());
        }

        private string GetFileNamePrefix(GeoCoordinate coordinate)
        {
            return String.Format("{0}{1:00}{2}{3:000}",
                coordinate.Latitude > 0 ? 'N' : 'S', Math.Abs(coordinate.Latitude),
                coordinate.Longitude > 0 ? 'E' : 'W', Math.Abs(coordinate.Longitude));
        }

        public void Configure(IConfigSection configSection)
        {
            _srtmMapPath = configSection.GetString("map", null);
            _srtmMapPath = configSection.GetString("server", @"http://dds.cr.usgs.gov/srtm/version2_1/SRTM3");
        }
    }
}
