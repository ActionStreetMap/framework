using System;
using System.Collections.Generic;
using ActionStreetMap.Core;
using ActionStreetMap.Infrastructure.IO;
using ActionStreetMap.Osm.Entities;
using ActionStreetMap.Osm.Index.Spatial;
using ActionStreetMap.Osm.Index.Storage;

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
        IEnumerable<Element> Get(BoundingBox bbox);
    }

    /// <summary>
    ///     Default implementation of <see cref="IElementSource" />. It uses custom map data format which should created using
    ///     ASM framework's importer.
    /// </summary>
    public class ElementSource : IElementSource
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
                KeyValueUsage.Load(fileService.ReadStream(string.Format(Consts.KeyValueIndexPathFormat, directory)));
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
        public IEnumerable<Element> Get(BoundingBox bbox)
        {
            var results = SpatialIndexTree.Search(new Envelop(bbox.MinPoint, bbox.MaxPoint));
            foreach (var result in results)
                yield return ElementStore.Get(result);
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