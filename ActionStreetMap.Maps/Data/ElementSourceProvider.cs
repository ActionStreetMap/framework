using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using ActionStreetMap.Core;
using ActionStreetMap.Infrastructure.Config;
using ActionStreetMap.Infrastructure.Dependencies;
using ActionStreetMap.Infrastructure.Diagnostic;
using ActionStreetMap.Infrastructure.Formats.Json;
using ActionStreetMap.Infrastructure.IO;
using ActionStreetMap.Infrastructure.Primitives;
using ActionStreetMap.Infrastructure.Reactive;
using ActionStreetMap.Maps.Data.Import;
using ActionStreetMap.Maps.Data.Spatial;
using ActionStreetMap.Infrastructure.Utilities;

namespace ActionStreetMap.Maps.Data
{
    /// <summary> Provides the way to get the corresponding element source by geocoordinate. </summary>
    public interface IElementSourceProvider: IDisposable
    {
        // <summary> Adds element source. </summary>
        void Add(IElementSource elementSource);

        /// <summary> Returns element sources by query represented by bounding box. </summary>
        /// <returns>Element source.</returns>
        IObservable<IElementSource> Get(BoundingBox query);
    }

    /// <summary> Default implementation of <see cref="IElementSourceProvider"/>. </summary>
    internal sealed class ElementSourceProvider : IElementSourceProvider, IConfigurable
    {
        private const string LogTag = "mapdata.source";
        private const string CacheFileNameExtension = ".map";
        private readonly Regex _geoCoordinateRegex = new Regex(@"([-+]?\d{1,2}([.]\d+)?),\s*([-+]?\d{1,3}([.]\d+)?)");
        private readonly string[] _splitString= { " " };

        private string _mapDataServerUri;
        private string _mapDataServerQuery;
        private string _mapDataFormat;
        private string _indexSettingsPath;
        private string _cachePath;

        private readonly IPathResolver _pathResolver;
        private readonly IFileSystemService _fileSystemService;
        private readonly IObjectPool _objectPool;
        private IndexSettings _settings;
        private RTree<string> _tree;
        private MutableTuple<string, IElementSource> _elementSourceCache;

        /// <summary> Trace. </summary>
        [Dependency]
        public ITrace Trace { get; set; }

        /// <summary> Creates instance of <see cref="ElementSourceProvider"/>. </summary>
        /// <param name="pathResolver">Path resolver.</param>
        /// <param name="fileSystemService">File system service.</param>
        /// <param name="objectPool">Object pool.</param>
        [Dependency]
        public ElementSourceProvider(IPathResolver pathResolver, IFileSystemService fileSystemService, 
            IObjectPool objectPool)
        {
            _pathResolver = pathResolver;
            _fileSystemService = fileSystemService;
            _objectPool = objectPool;
        }

        /// <inheritdoc />
        public void Add(IElementSource elementSource)
        {
            // TODO change accordingly cache implementation

            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public IObservable<IElementSource> Get(BoundingBox query)
        {
            // NOTE block thread here
            Trace.Info(LogTag, "getting element sources for {0}", query.ToString());
            var elementSourcePath = _tree.Search(query).Wait();

            // 1. online case: should use data from remove server as persistent cache is not present in tree..
            if (elementSourcePath == null)
            {
                var key = query.ToString().Replace(",", "_") + CacheFileNameExtension;
                var cacheFilePath = Path.Combine(_cachePath, key);

                // found in memory
                if (_elementSourceCache != null && cacheFilePath == _elementSourceCache.Item1)
                    return Observable.Return(_elementSourceCache.Item2);

                // NOTE at first glance, it seems to be nice idea to cache several built element sources
                // but this approach has major drawback: we don't know proper cache size:
                // for overview modes we will create a lot of tiles with different element sources, 
                // but only few will be reused. So, we will cache only last one

                // cache file is already there
                if (_fileSystemService.Exists(cacheFilePath))
                {
                    Trace.Info(LogTag, Strings.ElementSourceFileCacheHit, cacheFilePath);
                    var bytes = _fileSystemService.ReadBytes(cacheFilePath);
                    return GetElementSource(cacheFilePath, bytes);
                }

                // no cache and online source is not defined
                if (String.IsNullOrEmpty(_mapDataServerUri))
                {
                    Trace.Warn(LogTag, Strings.NoOfflineNoOnlineElementSource);
                    return Observable.Empty<IElementSource>();
                }

                return GetRemoteElementSource(cacheFilePath, query);
            }

            // load file cache or persistent index and put it into memory cache
            if (_elementSourceCache == null || elementSourcePath != _elementSourceCache.Item1)
            {
                Trace.Info(LogTag, "load index data from {0}", elementSourcePath);
                var elementSource = elementSourcePath.EndsWith(CacheFileNameExtension) ?
                    BuildElementSourceInMemory(_fileSystemService.ReadBytes(elementSourcePath)):
                    new ElementSource(elementSourcePath, _fileSystemService, _objectPool);

                AddElementSourceToMemoryCache(elementSourcePath, elementSource);
            }

            // element source should be already in memory cache
            return Observable.Return(_elementSourceCache.Item2);
        }

        #region Element source manipulation logic

        /// <summary> Gets data from remote server. </summary>
        private IObservable<IElementSource> GetRemoteElementSource(string path, BoundingBox query)
        {
            // make online query
            var queryString = String.Format(_mapDataServerQuery, query.MinPoint.Longitude, query.MinPoint.Latitude,
                query.MaxPoint.Longitude, query.MaxPoint.Latitude);
            var uri = String.Format("{0}{1}", _mapDataServerUri, Uri.EscapeDataString(queryString));
            Trace.Warn(LogTag, Strings.NoPresistentElementSourceFound, query.ToString(), uri);
            return ObservableWWW.GetAndGetBytes(uri)
                .Take(1)
                .SelectMany(bytes =>
                {
                    Trace.Info(LogTag, "add to cache {0} and build index from {1} bytes received",
                        path, bytes.Length.ToString());
                    // add to persistent cache
                    using (var stream = _fileSystemService.WriteStream(path))
                        stream.Write(bytes, 0, bytes.Length);
                    // build element source from bytes
                    return GetElementSource(path, bytes);
                });
        }

        /// <summary> Gets element source from byte array. </summary>
        private IObservable<IElementSource> GetElementSource(string elementSourcePath, byte[] bytes)
        {
            // build element source
            var elementSource = BuildElementSourceInMemory(bytes);
            // cache it
            AddElementSourceToMemoryCache(elementSourcePath, elementSource);
            return Observable.Return<IElementSource>(elementSource);
        }

        /// <summary> Adds element source to cache. </summary>
        private void AddElementSourceToMemoryCache(string elementSourcePath, IElementSource elementSource)
        {
            if (_elementSourceCache != null)
                _elementSourceCache.Item2.Dispose();
            _elementSourceCache = new MutableTuple<string, IElementSource>(elementSourcePath, elementSource);
        }

        /// <summary> Builds element source from raw data on fly. </summary>
        private IElementSource BuildElementSourceInMemory(byte[] bytes)
        {
            if (_settings == null) ReadIndexSettings();
            var indexBuilder = new InMemoryIndexBuilder(_mapDataFormat, new MemoryStream(bytes), 
                _settings, _objectPool, Trace);
            indexBuilder.Build();
            var elementSource = new ElementSource(indexBuilder.KvUsage, indexBuilder.KvIndex,
                indexBuilder.KvStore, indexBuilder.Store, indexBuilder.Tree);
            return elementSource;
        }

        #endregion

        #region Persistent index processing

        private void ReadIndexSettings() 
        {
            var jsonContent = _fileSystemService.ReadText(_indexSettingsPath);
            var node = JSON.Parse(jsonContent);
            _settings = new IndexSettings();
            _settings.ReadFromJson(node);
        }

        private void SearchAndReadMapIndexHeaders(string folder)
        {
            _fileSystemService.GetFiles(folder, MapConsts.HeaderFileName).ToList()
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
                _tree.Insert(Path.GetDirectoryName(headerPath), envelop);
            }
        }

        private GeoCoordinate GetCoordinateFromString(string coordinateStr)
        {
            var coordinates = _geoCoordinateRegex.Match(coordinateStr).Value.Split(',');

            var latitude = double.Parse(coordinates[0]);
            var longitude = double.Parse(coordinates[1]);

            return new GeoCoordinate(latitude, longitude);
        }

        #endregion

        /// <inheritdoc />
        public void Configure(IConfigSection configSection)
        {
            // @"http://api.openstreetmap.org/api/0.6/map?bbox="
            _mapDataServerUri = configSection.GetString(@"remote.server", null);
            //api/0.6/map?bbox=left,bottom,right,top
            _mapDataServerQuery = configSection.GetString(@"remote.query", null);
            _mapDataFormat = configSection.GetString(@"remote.format", "xml");
            _indexSettingsPath = configSection.GetString(@"index.settings", null);

            _tree = new RTree<string>();
            var rootFolder = configSection.GetString("local", null);
            if (!String.IsNullOrEmpty(rootFolder))
            {
                SearchAndReadMapIndexHeaders(_pathResolver.Resolve(rootFolder));
                // create cache directory
                _cachePath = Path.Combine(rootFolder, ".cache");
                _fileSystemService.CreateDirectory(_cachePath);
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if(_elementSourceCache != null)
                _elementSourceCache.Item2.Dispose();
        }
    }
}
