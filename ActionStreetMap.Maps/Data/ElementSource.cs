using System;
using ActionStreetMap.Core;
using ActionStreetMap.Infrastructure.IO;
using ActionStreetMap.Infrastructure.Reactive;
using ActionStreetMap.Maps.Data.Spatial;
using ActionStreetMap.Maps.Data.Storage;
using ActionStreetMap.Maps.Entities;
using ActionStreetMap.Infrastructure.Utilities;
using ActionStreetMap.Maps.Data.Import;

namespace ActionStreetMap.Maps.Data
{
    /// <summary> Represents an abstract source of Element objects. </summary>
    public interface IElementSource : IDisposable
    {
        /// <summary> Returns bounding box covered by element source. </summary>
        BoundingBox BoundingBox { get; }

        /// <summary> Gets true if element source is read only. </summary>
        bool IsReadOnly { get; }

        /// <summary> Returns elements which are located in the corresponding bbox for given zoom level. </summary>
        IObservable<Element> Get(BoundingBox bbox, int zoomLevel);
    }

    /// <summary> ASM's spatial index based element store implementation. </summary>
    internal sealed class ElementSource : IElementSource
    {
        internal readonly ISpatialIndex<uint> SpatialIndexTree;
        internal readonly KeyValueIndex KvIndex;
        internal readonly KeyValueStore KvStore;
        internal readonly KeyValueUsage KvUsage;
        internal readonly ElementStore ElementStore;

        /// <summary> Creates instance of <see cref="ElementSource" /> from persistent storage. </summary>
        /// <param name="directory">Already resolved directory which contains all indecies.</param>
        /// <param name="fileService">File system service.</param>
        /// <param name="objectPool">ObjectPool.</param>
        internal ElementSource(string directory, IFileSystemService fileService, IObjectPool objectPool)
        {
            // load map data from streams
            BoundingBox = PersistentIndexBuilder.ReadBoundingBox(fileService.ReadStream(string.Format(MapConsts.HeaderPathFormat, directory)));
            KvUsage = new KeyValueUsage(fileService.ReadStream(string.Format(MapConsts.KeyValueUsagePathFormat, directory)));
            KvIndex = KeyValueIndex.Load(fileService.ReadStream(string.Format(MapConsts.KeyValueIndexPathFormat, directory)));
            KvStore = new KeyValueStore(KvIndex, KvUsage, fileService.ReadStream(string.Format(MapConsts.KeyValueStorePathFormat, directory)));
            ElementStore = new ElementStore(KvStore, fileService.ReadStream(string.Format(MapConsts.ElementStorePathFormat, directory)), objectPool);
            SpatialIndexTree = SpatialIndex.Load(fileService.ReadStream(string.Format(MapConsts.SpatialIndexPathFormat, directory)));
            IsReadOnly = true;
        }

        /// <summary>
        ///     Creates instance of <see cref="ElementSource" /> from streams and 
        ///     created spatial index.
        /// </summary>
        internal ElementSource(BoundingBox boundingBox, KeyValueUsage keyValueUsage, KeyValueIndex keyValueIndex, KeyValueStore keyValueStore,
            ElementStore elementStore, ISpatialIndex<uint> spatialIndex)
        {
            BoundingBox = boundingBox;
            KvUsage = keyValueUsage;
            KvIndex = keyValueIndex;
            KvStore = keyValueStore;
            ElementStore = elementStore;
            SpatialIndexTree = spatialIndex;
            IsReadOnly = true;
        }

        /// <summary> Creates instance of <see cref="ElementSource"/>. from in memory index builder. </summary>
        /// <param name="indexBuilder"></param>
        internal ElementSource(InMemoryIndexBuilder indexBuilder) : 
            this(indexBuilder.BoundingBox, indexBuilder.KvUsage, indexBuilder.KvIndex, 
            indexBuilder.KvStore, indexBuilder.Store, indexBuilder.Tree)
        {
        }

        /// <inheritdoc />
        public BoundingBox BoundingBox { get; internal set; }

        /// <inheritdoc />
        public bool IsReadOnly { get; set; }

        /// <inheritdoc />
        public IObservable<Element> Get(BoundingBox bbox, int zoomLevel)
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