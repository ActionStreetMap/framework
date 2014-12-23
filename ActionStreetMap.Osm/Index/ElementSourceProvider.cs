using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using ActionStreetMap.Core;
using ActionStreetMap.Infrastructure.Config;
using ActionStreetMap.Infrastructure.Dependencies;
using ActionStreetMap.Infrastructure.Diagnostic;
using ActionStreetMap.Infrastructure.IO;
using ActionStreetMap.Infrastructure.Primitives;
using ActionStreetMap.Osm.Index.Spatial;

namespace ActionStreetMap.Osm.Index
{
    /// <summary>
    ///     Provides the way to get the corresponding element source by geocoordinate.
    /// </summary>
    public interface IElementSourceProvider
    {
        /// <summary>
        ///     Returns element source by query represented by bounding box.
        /// </summary>
        IElementSource Get(BoundingBox query);
    }

    /// <summary>
    ///     Default implementation of <see cref="IElementSourceProvider"/>
    /// </summary>
    public class ElementSourceProvider : IElementSourceProvider, IConfigurable
    {
        private const string MapPathKey = "mapdata";

        private readonly Regex _geoCoordinateRegex = new Regex(@"([-+]?\d{1,2}([.]\d+)?),\s*([-+]?\d{1,3}([.]\d+)?)");
        private readonly string[] _splitString= { " " };

        private readonly IPathResolver _pathResolver;
        private readonly IFileSystemService _fileSystemService;
        private SpatialIndex<string> _searchTree;
        private RTree<string> _insertTree;
        private Tuple<string, IElementSource> _elementSourceCache;

        public ITrace Trace { get; set; }

        [Dependency]
        public ElementSourceProvider(IPathResolver pathResolver, IFileSystemService fileSystemService)
        {
            _pathResolver = pathResolver;
            _fileSystemService = fileSystemService;
        }

        public IElementSource Get(BoundingBox query)
        {
            var sourcePaths = _searchTree
                .Search(new Envelop(query.MinPoint, query.MaxPoint)).ToArray();

            if (!sourcePaths.Any())
            {
                Trace.Warn("Maps", String.Format("No element found for given query:{0}", query));
                return null;
            }

            // TODO so far, we support only one element source per query
            // but it can be extended to handle intersection cases
            if (sourcePaths.Count() > 1)
                Trace.Warn("Maps", "Current IElementSourceProvider doesn't support multiply element sources bbox match.");

            var elementSourcePath = sourcePaths.First();
            if (_elementSourceCache == null || elementSourcePath != _elementSourceCache.Item1)
            {
                if(_elementSourceCache != null)
                    _elementSourceCache.Item2.Dispose();
                var elementSource = new ElementSource(elementSourcePath, _fileSystemService);
                _elementSourceCache = new Tuple<string, IElementSource>(elementSourcePath, elementSource);
            }

            return _elementSourceCache.Item2;
        }

        private void SearchAndReadMapIndexHeaders(string folder)
        {
            _fileSystemService.GetFiles(folder, Consts.HeaderFileName).ToList()
                .ForEach(ReadMapIndexHeader);

            _fileSystemService.GetDirectories(folder, "*").ToList()
                .ForEach(SearchAndReadMapIndexHeaders);
        }

        private void ReadMapIndexHeader(string headerPath)
        {
            using (var reader = new StreamReader(_fileSystemService.ReadStream(headerPath)))
            {
                var str = reader.ReadLine();
                var coordinateStrings = str.Split(_splitString, StringSplitOptions.None);
                var minPoint = GetCoordinateFromString(coordinateStrings[0]);
                var maxPoint = GetCoordinateFromString(coordinateStrings[1]);

                var envelop = new Envelop(minPoint, maxPoint);
                _insertTree.Insert(Path.GetDirectoryName(headerPath), envelop);
            }
        }

        private GeoCoordinate GetCoordinateFromString(string coordinateStr)
        {
            var coordinates = _geoCoordinateRegex.Match(coordinateStr).Value.Split(',');

            var latitude = double.Parse(coordinates[0]);
            var longitude = double.Parse(coordinates[1]);

            return new GeoCoordinate(latitude, longitude);
        }

        public void Configure(IConfigSection configSection)
        {
            _insertTree = new RTree<string>();
            var rootFolder = configSection.GetString(MapPathKey);
            SearchAndReadMapIndexHeaders(_pathResolver.Resolve(rootFolder));
            // convert to search tree and release insert tree
            _searchTree = SpatialIndex<string>.ToReadOnly(_insertTree);
            _insertTree = null;
        }
    }
}
