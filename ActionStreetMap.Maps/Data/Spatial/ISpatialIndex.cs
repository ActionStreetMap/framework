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
    }
}
