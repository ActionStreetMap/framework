using ActionStreetMap.Core;
using ActionStreetMap.Infrastructure.Reactive;

namespace ActionStreetMap.Maps.Data.Spatial
{
    /// <summary> Represents spatial index. </summary>
    /// <typeparam name="T">Node type.</typeparam>
    public interface ISpatialIndex<T>
    {
        /// <summary> Performs search for given bounding box. </summary>
        /// <param name="query">Bounding box.</param>
        /// <returns>Observable results.</returns>
        IObservable<T> Search(BoundingBox query);

        /// <summary> Performs search for given bounding box and zoom level. </summary>
        /// <param name="query">Bounding box.</param>
        /// <param name="zoomLevel">Zoom level.</param>
        /// <returns>Observable results.</returns>
        IObservable<T> Search(BoundingBox query, int zoomLevel);

        /// <summary> Performs insertion into tree. </summary>
        /// <param name="data">Data to insert. </param>
        /// <param name="boundingBox">Bounding box of data.</param>
        void Insert(T data, BoundingBox boundingBox);

        /// <summary> Removes data from the tree. </summary>
        /// <param name="data">Data to remove. </param>
        /// <param name="boundingBox">Bounding box of data.</param>
        void Remove(T data, BoundingBox boundingBox);
    }
}
