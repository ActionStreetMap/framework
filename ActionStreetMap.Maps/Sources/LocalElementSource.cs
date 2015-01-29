using ActionStreetMap.Core;
using ActionStreetMap.Infrastructure.IO;
using ActionStreetMap.Infrastructure.Reactive;
using ActionStreetMap.Maps.Entities;
using ActionStreetMap.Maps.Index;
using ActionStreetMap.Maps.Index.Spatial;
using ActionStreetMap.Maps.Index.Storage;

namespace ActionStreetMap.Maps.Sources
{
    /// <summary>
    ///     This implementation uses custom map data format which should created using ASM framework's importer.
    /// </summary>
    public sealed class LocalElementSource : IElementSource
    {
        // these values are used by search
        internal readonly SpatialIndex<uint> SpatialIndexTree;
        internal readonly KeyValueIndex KvIndex;
        internal readonly KeyValueStore KvStore;
        internal readonly KeyValueUsage KvUsage;
        internal readonly ElementStore ElementStore;

        /// <summary>
        ///     Creates instance of <see cref="LocalElementSource" />.
        /// </summary>
        /// <param name="directory">Already resolved directory which contains all indecies.</param>
        /// <param name="fileService">File system service.</param>
        public LocalElementSource(string directory, IFileSystemService fileService)
        {
            // load map data from streams
            KvUsage = new KeyValueUsage(fileService.ReadStream(string.Format(Consts.KeyValueUsagePathFormat, directory)));
            KvIndex = KeyValueIndex.Load(fileService.ReadStream(string.Format(Consts.KeyValueIndexPathFormat, directory)));
            KvStore = new KeyValueStore(KvIndex, KvUsage, fileService.ReadStream(string.Format(Consts.KeyValueStorePathFormat, directory)));
            ElementStore = new ElementStore(KvStore,fileService.ReadStream(string.Format(Consts.ElementStorePathFormat, directory)));
            SpatialIndexTree = SpatialIndex<uint>.Load(fileService.ReadStream(string.Format(Consts.SpatialIndexPathFormat, directory)));
        }

        /// <inheritdoc />
        public IObservable<Element> Get(BoundingBox bbox)
        {
            return SpatialIndexTree.Search(bbox)
                .ObserveOn(Scheduler.CurrentThread)
                .Select((offset) => ElementStore.Get(offset));
        }

        /// <inheritdoc />
        public void Dispose()
        {
            KvUsage.Dispose();
            KvStore.Dispose();
            ElementStore.Dispose();
        }
    }
}