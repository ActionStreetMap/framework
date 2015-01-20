using System;
using System.Collections.Generic;
using ActionStreetMap.Core;
using ActionStreetMap.Infrastructure.IO;
using ActionStreetMap.Osm.Entities;
using ActionStreetMap.Osm.Index.Spatial;
using ActionStreetMap.Osm.Index.Storage;
using ActionStreetMap.Infrastructure.Reactive;

namespace ActionStreetMap.Osm.Index
{
    /// <summary>
    ///     Represents an abstract source of Element objects.
    /// </summary>
    public interface IElementSource : IDisposable
    {
        /// <summary>
        ///     Returns elements which are located in the corresponding bbox.
        /// </summary>
        IObservable<Element> Get(BoundingBox bbox);
    }

    /// <summary>
    ///     Default implementation of <see cref="IElementSource" />. It uses custom map data format which should created using
    ///     ASM framework's importer.
    /// </summary>
    public sealed class ElementSource : IElementSource
    {
        // these values are used by search
        internal readonly SpatialIndex<uint> SpatialIndexTree;
        internal readonly KeyValueIndex KvIndex;
        internal readonly KeyValueStore KvStore;
        internal readonly KeyValueUsage KvUsage;
        internal readonly ElementStore ElementStore;

        /// <summary>
        ///     Creates instance of <see cref="ElementSource" />.
        /// </summary>
        /// <param name="directory">Already resolved directory which contains all indecies.</param>
        /// <param name="fileService">File system service.</param>
        public ElementSource(string directory, IFileSystemService fileService)
        {
            // load map data from streams
            KvUsage = 
                new KeyValueUsage(fileService.ReadStream(string.Format(Consts.KeyValueUsagePathFormat, directory)));
            KvIndex =
                KeyValueIndex.Load(fileService.ReadStream(string.Format(Consts.KeyValueIndexPathFormat, directory)));
            KvStore = new KeyValueStore(KvIndex, KvUsage,
                fileService.ReadStream(string.Format(Consts.KeyValueStorePathFormat, directory)));
            ElementStore = new ElementStore(KvStore,
                fileService.ReadStream(string.Format(Consts.ElementStorePathFormat, directory)));
            SpatialIndexTree =
                SpatialIndex<uint>.Load(fileService.ReadStream(string.Format(Consts.SpatialIndexPathFormat, directory)));
        }

        /// <inheritdoc />
        public IObservable<Element> Get(BoundingBox bbox)
        {
            return SpatialIndexTree.Search(new Envelop(bbox.MinPoint, bbox.MaxPoint))
                .ObserveOn(Scheduler.CurrentThread)
                .Select(ElementStore.Get);
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