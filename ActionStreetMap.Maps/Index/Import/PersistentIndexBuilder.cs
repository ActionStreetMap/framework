using System;
using System.IO;
using ActionStreetMap.Core;
using ActionStreetMap.Infrastructure.Diagnostic;
using ActionStreetMap.Infrastructure.IO;
using ActionStreetMap.Maps.Index.Spatial;
using ActionStreetMap.Maps.Index.Storage;

namespace ActionStreetMap.Maps.Index.Import
{
    internal class PersistentIndexBuilder : IndexBuilder
    {
        private readonly string _filePath;
        private readonly string _outputDirectory;

        public PersistentIndexBuilder(string filePath, string outputDirectory, IFileSystemService fileSystemService,
            ITrace trace)
            : base(trace)
        {
            _filePath = filePath;
            _outputDirectory = outputDirectory;
        }

        public override void Build()
        {
            var sourceStream = new FileStream(_filePath, FileMode.Open);
            var reader = GetReader(Path.GetExtension(_filePath), sourceStream);

            var kvUsageMemoryStream = new MemoryStream();
            var kvUsage = new KeyValueUsage(kvUsageMemoryStream);

            var keyValueStoreFile = new FileStream(String.Format(Consts.KeyValueStorePathFormat, _outputDirectory), FileMode.Create);
            var index = new KeyValueIndex(Settings.Search.KvIndexCapacity, Settings.Search.PrefixLength);
            var keyValueStore = new KeyValueStore(index, kvUsage, keyValueStoreFile);

            var storeFile = new FileStream(String.Format(Consts.ElementStorePathFormat, _outputDirectory), FileMode.Create);
            Store = new ElementStore(keyValueStore, storeFile);
            Tree = new RTree<uint>(65);

            reader.Read();
            Clear();
            Complete();

            using (var kvFileStream = new FileStream(String.Format(Consts.KeyValueUsagePathFormat, _outputDirectory), FileMode.Create))
            {
                var buffer = kvUsageMemoryStream.GetBuffer();
                kvFileStream.Write(buffer, 0, (int) kvUsageMemoryStream.Length);
            }

            KeyValueIndex.Save(index, new FileStream(String.Format(Consts.KeyValueIndexPathFormat, _outputDirectory), FileMode.Create));
            SpatialIndex<uint>.Save(Tree, new FileStream(String.Format(Consts.SpatialIndexPathFormat, _outputDirectory), FileMode.Create));
            Store.Dispose();
        }

        public override void ProcessBoundingBox(BoundingBox bbox)
        {
            using (var writer = new StreamWriter(new FileStream(String.Format(Consts.HeaderPathFormat, _outputDirectory), FileMode.Create)))
            {
                writer.Write("{0} {1}", bbox.MinPoint, bbox.MaxPoint);
            }
        }
    }
}