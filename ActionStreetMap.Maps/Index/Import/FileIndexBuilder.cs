using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using ActionStreetMap.Infrastructure.Diagnostic;
using ActionStreetMap.Infrastructure.IO;
using ActionStreetMap.Maps.Index.Spatial;
using ActionStreetMap.Maps.Index.Storage;
using ActionStreetMap.Core;

namespace ActionStreetMap.Maps.Index.Import
{
    internal class FileIndexBuilder: IndexBuilder
    {
        private readonly string _filePath;
        private readonly string _outputDirectory;

        public FileIndexBuilder(string filePath, string outputDirectory, IFileSystemService fileSystemService, ITrace trace)
            : base(trace)
        {
            _filePath = filePath;
            _outputDirectory = outputDirectory;
        }

        public override void Build()
        {
            var reader = GetReader(_filePath);

            var kvUsageMemoryStream = new MemoryStream();
            var kvUsage = new KeyValueUsage(kvUsageMemoryStream);

            var keyValueStoreFile = new FileStream(String.Format(Consts.KeyValueStorePathFormat, _outputDirectory), FileMode.Create);
            var index = new KeyValueIndex(_settings.Search.KvIndexCapacity, _settings.Search.PrefixLength);
            var keyValueStore = new KeyValueStore(index, kvUsage, keyValueStoreFile);

            var storeFile = new FileStream(String.Format(Consts.ElementStorePathFormat, _outputDirectory), FileMode.Create);
            _store = new ElementStore(keyValueStore, storeFile);
            _tree = new RTree<uint>(65);

            reader.Read();
            Clear();
            Complete();

            using (var kvFileStream = new FileStream(String.Format(Consts.KeyValueUsagePathFormat, _outputDirectory), FileMode.Create))
            {
                var buffer = kvUsageMemoryStream.GetBuffer();
                kvFileStream.Write(buffer, 0, (int)kvUsageMemoryStream.Length);
            }

            KeyValueIndex.Save(index, new FileStream(String.Format(Consts.KeyValueIndexPathFormat, _outputDirectory), FileMode.Create));
            SpatialIndex<uint>.Save(_tree, new FileStream(String.Format(Consts.SpatialIndexPathFormat, _outputDirectory), FileMode.Create));
            _store.Dispose();
        }

        public override void ProcessBoundingBox(BoundingBox bbox)
        {
            // TODO save header file
            using (var writer = new StreamWriter(new FileStream(String.Format(Consts.HeaderPathFormat, _outputDirectory),
                    FileMode.Create)))
            {
                writer.Write("{0} {1}", bbox.MinPoint, bbox.MaxPoint);
            }
        }
    }
}
