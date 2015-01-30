using ActionStreetMap.Core;
using ActionStreetMap.Infrastructure.Reactive;

namespace ActionStreetMap.Maps.Data.Spatial
{
    /// <summary> Represents spatial index. </summary>
    /// <typeparam name="T"></typeparam>
    public interface ISpatialIndex<T>
    {
        /// <summary> Performs search for given bounding box. </summary>
        /// <param name="query">Bounding box.</param>
        /// <returns>Observable results.</returns>
        IObservable<T> Search(BoundingBox query);
    }
}
