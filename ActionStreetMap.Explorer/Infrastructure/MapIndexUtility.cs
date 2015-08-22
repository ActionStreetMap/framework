using System;
using System.IO;
using ActionStreetMap.Core;
using ActionStreetMap.Infrastructure.Config;
using ActionStreetMap.Infrastructure.Dependencies;
using ActionStreetMap.Infrastructure.Diagnostic;
using ActionStreetMap.Infrastructure.Formats.Json;
using ActionStreetMap.Infrastructure.IO;
using ActionStreetMap.Infrastructure.Reactive;
using ActionStreetMap.Infrastructure.Utilities;
using ActionStreetMap.Maps.Data.Import;

namespace ActionStreetMap.Explorer.Infrastructure
{
    /// <summary> Responsible for map index maintainance. </summary>
    public class MapIndexUtility: IConfigurable
    {
        private readonly IFileSystemService _fileSystemService;
        private readonly IObjectPool _objectPool;

        private string _indexSettingsPath;
        private string _indexRootFolder;

        [Dependency]
        public ITrace Trace { get; set; }

        /// <summary> Creates instance of <see cref="MapIndexUtility"/>. </summary>
        /// <param name="fileSystemService">File system service.</param>
        /// <param name="objectPool"></param>
        [Dependency]
        public MapIndexUtility(IFileSystemService fileSystemService, IObjectPool objectPool)
        {
            _fileSystemService = fileSystemService;
            _objectPool = objectPool;
        }

        #region Public methods

        /// <summary> Builds persistent index from given parameters. </summary>
        /// <param name="filePath">Path to map data file.</param>
        /// <param name="outputDirectory">Output directory for index files.</param>
        public void BuildIndex(string filePath, string outputDirectory)
        {
            if (!outputDirectory.StartsWith(_indexRootFolder))
                Trace.Warn("index.mngr", Strings.MapIndexBuildOutputDirectoryMismatch);

            // create directory for output files
            _fileSystemService.CreateDirectory(outputDirectory);

            // read settings
            var jsonContent = _fileSystemService.ReadText(_indexSettingsPath);
            var node = JSON.Parse(jsonContent);

            var settings = new IndexSettings();
            settings.ReadFromJson(node);

            // create builder from given parameters
            var builder = new PersistentIndexBuilder(filePath, outputDirectory,
                _fileSystemService, settings, _objectPool, Trace);

            // build
            builder.Build();
        }

        /// <summary> Returns list of registered map indices. </summary>
        public IObservable<IndexEntry> GetIndexEntries()
        {
            return Observable.Create<IndexEntry>(o =>
            {
                foreach (var directory in _fileSystemService.GetDirectories(_indexRootFolder, "*"))
                {
                    var headerFile = Path.Combine(directory, "header.txt");
                    var content = _fileSystemService.ReadText(headerFile);
                    // TODO add additional information to entry
                    o.OnNext(new IndexEntry()
                    {
                        DisplayName = content,
                        Path = directory,
                    });
                }
                o.OnCompleted();
                return Disposable.Empty;
            });
        }

        #endregion

        #region Nested classes

        /// <summary> Index entry. </summary>
        public class IndexEntry
        {
            public string DisplayName { get; internal set; }
            public string Path { get; internal set; }
            public BoundingBox BoundingBox { get; internal set; }
        }

        #endregion

        #region IConfigurable implementation

        /// <inheritdoc />
        public void Configure(IConfigSection configSection)
        {
            _indexSettingsPath = configSection.GetString("index.settings", null);
            _indexRootFolder = configSection.GetString("local", null);
        }

        #endregion
    }
}
