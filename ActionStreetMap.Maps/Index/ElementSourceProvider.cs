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
using ActionStreetMap.Infrastructure.Reactive;
using ActionStreetMap.Maps.Index.Spatial;
using ActionStreetMap.Maps.Index.Import;

namespace ActionStreetMap.Maps.Index
{
    /// <summary>
    ///     Provides the way to get the corresponding element source by geocoordinate.
    /// </summary>
    public interface IElementSourceProvider: IDisposable
    {
        /// <summary>
        ///     Returns element sources by query represented by bounding box.
        /// </summary>
        /// <returns>Element source.</returns>
        IObservable<IElementSource> Get(BoundingBox query);

        /// <summary>
        ///     Returns active element sources.
        /// </summary>
        /// <returns>Element source.</returns>
        IObservable<IElementSource> Get();
    }

    /// <summary>
    ///     Default implementation of <see cref="IElementSourceProvider"/>
    /// </summary>
    public sealed class ElementSourceProvider : IElementSourceProvider, IConfigurable
    {
        private const string MapPathKey = "";

        private readonly Regex _geoCoordinateRegex = new Regex(@"([-+]?\d{1,2}([.]\d+)?),\s*([-+]?\d{1,3}([.]\d+)?)");
        private readonly string[] _splitString= { " " };

        private readonly IPathResolver _pathResolver;
        private readonly IFileSystemService _fileSystemService;
        private ISpatialIndex<string> _searchTree;
        private RTree<string> _insertTree;
        private MutableTuple<string, IElementSource> _elementSourceCache;

        /// <summary>
        ///     Trace.
        /// </summary>
        [Dependency]
        public ITrace Trace { get; set; }

        /// <summary>
        ///     Creates instance of <see cref="ElementSourceProvider"/>.
        /// </summary>
        /// <param name="pathResolver">Path resolver.</param>
        /// <param name="fileSystemService">File system service.</param>
        [Dependency]
        public ElementSourceProvider(IPathResolver pathResolver, IFileSystemService fileSystemService)
        {
            _pathResolver = pathResolver;
            _fileSystemService = fileSystemService;
        }

        /// <inheritdoc />
        public IObservable<IElementSource> Get()
        {
            return _elementSourceCache != null ? 
                Observable.Return<IElementSource>(_elementSourceCache.Item2) : 
                Observable.Empty<IElementSource>();
        }

        /// <inheritdoc />
        public IObservable<IElementSource> Get(BoundingBox query)
        {
            // NOTE block thread here
            var elementSourcePath = _searchTree.Search(query).Wait();

            if (elementSourcePath == null)
            {
                Trace.Warn("Maps", String.Format(Strings.NoPresistentElementSourceFound, query));
                // TODO genetate url
                var uri = query.ToString();
                return ObservableWWW.GetAndGetBytes(uri).Take(1)
                    .SelectMany(b =>
                    {
                        var indexBuilder = new InMemoryIndexBuilder(".xml", new MemoryStream(b), Trace);
                        indexBuilder.Build();
                        var elementStore = new ElementSource(indexBuilder.KvUsage, indexBuilder.KvIndex, 
                            indexBuilder.KvStore, indexBuilder.Store, indexBuilder.Tree);
                        SetCurrentElementSource(query.ToString(), elementStore);
                        return Observable.Return<IElementSource>(elementStore);
                    });
            }

            if (_elementSourceCache == null || elementSourcePath != _elementSourceCache.Item1)
                SetCurrentElementSource(elementSourcePath, new ElementSource(elementSourcePath, _fileSystemService));

            return Observable.Return(_elementSourceCache.Item2);
        }

        private void SetCurrentElementSource(string elementSourcePath, IElementSource elementSource)
        {
            if (_elementSourceCache != null)
                _elementSourceCache.Item2.Dispose();
            _elementSourceCache = new MutableTuple<string, IElementSource>(elementSourcePath, elementSource);
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

        /// <inheritdoc />
        public void Configure(IConfigSection configSection)
        {
            _insertTree = new RTree<string>();
            var rootFolder = configSection.GetString(MapPathKey, null);
            if (!String.IsNullOrEmpty(rootFolder))
                SearchAndReadMapIndexHeaders(_pathResolver.Resolve(rootFolder));
            
            // convert to search tree and release insert tree
            _searchTree = SpatialIndex<string>.ToReadOnly(_insertTree);
            _insertTree = null;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if(_elementSourceCache != null)
                _elementSourceCache.Item2.Dispose();
        }
    }
}
