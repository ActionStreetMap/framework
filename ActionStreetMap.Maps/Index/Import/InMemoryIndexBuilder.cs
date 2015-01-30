using System.IO;
using ActionStreetMap.Infrastructure.Diagnostic;
using ActionStreetMap.Maps.Index.Spatial;
using ActionStreetMap.Maps.Index.Storage;
using ActionStreetMap.Maps.Formats;

namespace ActionStreetMap.Maps.Index.Import
{
    internal class InMemoryIndexBuilder: IndexBuilder
    {
        private readonly string _extension;
        private readonly Stream _sourceStream;

        internal KeyValueIndex KvIndex { get; private set; }
        internal KeyValueStore KvStore { get; private set; }
        internal KeyValueUsage KvUsage { get; private set; }

        public InMemoryIndexBuilder(string extension, Stream sourceStream, IndexSettings settings, ITrace trace)
            : base(settings, trace)
        {
            _extension = extension;
            _sourceStream = sourceStream;
        }

        public override void Build()
        {
            var reader = GetReader(_extension);

            var kvUsageMemoryStream = new MemoryStream();
            KvUsage = new KeyValueUsage(kvUsageMemoryStream);

            var keyValueStoreFile = new MemoryStream();
            KvIndex = new KeyValueIndex(Settings.Search.KvIndexCapacity, Settings.Search.PrefixLength);
            KvStore = new KeyValueStore(KvIndex, KvUsage, keyValueStoreFile);

            var storeFile = new MemoryStream();
            Store = new ElementStore(KvStore, storeFile);
            Tree = new RTree<uint>(65);

            reader.Read(new ReaderContext
            {
                SourceStream = _sourceStream,
                Builder = this,
                ReuseEntities = false,
                SkipTags = false,
            });
            Clear();
            Complete();
        }

        protected override void Dispose(bool disposing)
        {
            KvIndex = null;
            KvStore.Dispose();
            KvUsage.Dispose();
            base.Dispose(disposing);
        }
    }
}
