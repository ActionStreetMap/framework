using System;
using System.IO;
using ActionStreetMap.Core;
using ActionStreetMap.Infrastructure.IO;
using ActionStreetMap.Infrastructure.Reactive;
using ActionStreetMap.Maps.Entities;
using ActionStreetMap.Maps.Index;
using ActionStreetMap.Maps.Index.Spatial;
using ActionStreetMap.Maps.Index.Storage;

namespace ActionStreetMap.Maps.Sources
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
        ///     Creates instance of <see cref="ActionStreetMap.Maps.Sources.ElementSource" /> from persistent storage.
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
        ///     Creates instance of <see cref="ActionStreetMap.Maps.Sources.ElementSource" /> from streams and 
        ///     created spatial index.
        /// </summary>
        internal ElementSource(Stream keyValueUsageStream, Stream keyValueIndexStream, Stream keyValueStoreStream,
            Stream elementStoreStream, ISpatialIndex<uint> spatialIndex)
        {
            KvUsage = new KeyValueUsage(keyValueUsageStream);
            KvIndex = KeyValueIndex.Load(keyValueIndexStream);
            KvStore = new KeyValueStore(KvIndex, KvUsage, keyValueStoreStream);
            ElementStore = new ElementStore(KvStore, elementStoreStream);
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