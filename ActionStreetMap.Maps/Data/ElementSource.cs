using System;
using ActionStreetMap.Core;
using ActionStreetMap.Infrastructure.IO;
using ActionStreetMap.Infrastructure.Reactive;
using ActionStreetMap.Maps.Data.Spatial;
using ActionStreetMap.Maps.Data.Storage;
using ActionStreetMap.Maps.Entities;

namespace ActionStreetMap.Maps.Data
{
    /// <summary> Represents an abstract source of Element objects. </summary>
    public interface IElementSource : IDisposable
    {
        /// <summary> Returns elements which are located in the corresponding bbox. </summary>
        IObservable<Element> Get(BoundingBox bbox);
    }

    /// <summary> ASM's spatial index based element store implementation. </summary>
    internal sealed class ElementSource : IElementSource
    {
        // these values are used by search
        internal readonly ISpatialIndex<uint> SpatialIndexTree;
        internal readonly KeyValueIndex KvIndex;
        internal readonly KeyValueStore KvStore;
        internal readonly KeyValueUsage KvUsage;
        internal readonly ElementStore ElementStore;

        /// <summary>
        ///     Creates instance of <see cref="ElementSource" /> from persistent storage.
        /// </summary>
        /// <param name="directory">Already resolved directory which contains all indecies.</param>
        /// <param name="fileService">File system service.</param>
        internal ElementSource(string directory, IFileSystemService fileService)
        {
            // load map data from streams
            KvUsage = new KeyValueUsage(fileService.ReadStream(string.Format(Consts.KeyValueUsagePathFormat, directory)));
            KvIndex = KeyValueIndex.Load(fileService.ReadStream(string.Format(Consts.KeyValueIndexPathFormat, directory)));
            KvStore = new KeyValueStore(KvIndex, KvUsage, fileService.ReadStream(string.Format(Consts.KeyValueStorePathFormat, directory)));
            ElementStore = new ElementStore(KvStore, fileService.ReadStream(string.Format(Consts.ElementStorePathFormat, directory)));
            SpatialIndexTree = SpatialIndex<uint>.Load(fileService.ReadStream(string.Format(Consts.SpatialIndexPathFormat, directory)));
        }

        /// <summary>
        ///     Creates instance of <see cref="ElementSource" /> from streams and 
        ///     created spatial index.
        /// </summary>
        internal ElementSource(KeyValueUsage keyValueUsage, KeyValueIndex keyValueIndex, KeyValueStore keyValueStore,
            ElementStore elementStore, ISpatialIndex<uint> spatialIndex)
        {
            KvUsage = keyValueUsage;
            KvIndex = keyValueIndex;
            KvStore = keyValueStore;
            ElementStore = elementStore;
            SpatialIndexTree = spatialIndex;
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