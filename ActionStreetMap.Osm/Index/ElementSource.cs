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

    internal class ElementSource: IElementSource
    {
        private readonly IFileSystemService _fileService;

        private readonly SpatialIndex<uint> _spatialIndexTree;
        private readonly ElementStore _elementStore;

        /// <summary>
        ///     Creates instance of <see cref="ElementSource"/>.
        /// </summary>
        /// <param name="directory">Already resolved directory which contains all indecies.</param>
        /// <param name="fileService">File system service.</param>
        public ElementSource(string directory, IFileSystemService fileService)
        {
            _fileService = fileService;

            var kvIndex = KeyValueIndex.Load(fileService.ReadStream(string.Format(Consts.KeyValueIndexPathFormat, directory)));
            var kvStore = new KeyValueStore(kvIndex, fileService.ReadStream(string.Format(Consts.KeyValueStorePathFormat, directory)));
            _elementStore = new ElementStore(kvStore, fileService.ReadStream(string.Format(Consts.ElementStorePathFormat, directory)));
            _spatialIndexTree = SpatialIndex<uint>.Load(fileService.ReadStream(string.Format(Consts.SpatialIndexPathFormat, directory)));
        }

        public IEnumerable<Element> Get(BoundingBox bbox)
        {
            var results = _spatialIndexTree.Search(new Envelop(bbox.MinPoint, bbox.MaxPoint));
            foreach (var result in results)
                yield return _elementStore.Get(result);
        }

        public void Dispose()
        {
            _elementStore.Dispose();
        }
    }
}
