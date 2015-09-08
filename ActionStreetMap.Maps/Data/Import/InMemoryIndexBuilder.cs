using System.Diagnostics;
using System.IO;
using ActionStreetMap.Core;
using ActionStreetMap.Infrastructure.Diagnostic;
using ActionStreetMap.Maps.Data.Spatial;
using ActionStreetMap.Maps.Data.Storage;
using ActionStreetMap.Maps.Formats;
using ActionStreetMap.Infrastructure.Utilities;

namespace ActionStreetMap.Maps.Data.Import
{
    internal class InMemoryIndexBuilder: IndexBuilder
    {
        private readonly IReader _reader;
        private readonly Stream _sourceStream;

        internal KeyValueIndex KvIndex { get; private set; }
        internal KeyValueStore KvStore { get; private set; }
        internal KeyValueUsage KvUsage { get; private set; }

        public InMemoryIndexBuilder(BoundingBox boundingBox, IndexSettings settings, 
            IObjectPool objectPool, ITrace trace)
            : base(settings, objectPool, trace)
        {
            BoundingBox = boundingBox;
            _reader = new EmptyReader();
        }

        public InMemoryIndexBuilder(string extension, Stream sourceStream, IndexSettings settings, 
            IObjectPool objectPool, ITrace trace)
            : base(settings, objectPool, trace)
        {
            _reader = GetReader(extension);
            _sourceStream = sourceStream;
        }

        public override void Build()
        {
            var sw = new Stopwatch();
            sw.Start();

            var kvUsageMemoryStream = new MemoryStream();
            KvUsage = new KeyValueUsage(kvUsageMemoryStream);

            var keyValueStoreFile = new MemoryStream();
            KvIndex = new KeyValueIndex(Settings.Search.KvIndexCapacity, Settings.Search.PrefixLength);
            KvStore = new KeyValueStore(KvIndex, KvUsage, keyValueStoreFile);

            var storeFile = new MemoryStream();
            Store = new ElementStore(KvStore, storeFile, ObjectPool);
            Tree = new RTree<uint>(65);

            _reader.Read(new ReaderContext
            {
                SourceStream = _sourceStream,
                Builder = this,
                ReuseEntities = false,
                SkipTags = false,
            });
            Clear();
            Complete();

            sw.Stop();
            Trace.Debug(CategoryKey, Strings.IndexBuildInMs, sw.ElapsedMilliseconds.ToString());
        }

        protected override void Dispose(bool disposing)
        {
            KvIndex = null;
            KvStore.Dispose();
            KvUsage.Dispose();
            base.Dispose(disposing);
        }

        private class EmptyReader : IReader
        {
            public void Read(ReaderContext context)
            {
            }
        }
    }
}
