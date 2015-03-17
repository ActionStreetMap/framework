using ActionStreetMap.Infrastructure.Reactive;

namespace ActionStreetMap.Core
{
    /// <summary> Defines behavior of elevation provider. </summary>
    public interface IElevationProvider
    {
        /// <summary> Checks whether elevation data for given bounding box is present in map data. </summary>
        /// <returns>True, if data is already there.</returns>
        bool HasElevation(BoundingBox bbox);

        /// <summary> Download elevation data from server. </summary>
        IObservable<Unit> Download(BoundingBox bbox);

        /// <summary> Gets elevation for given geocoordinate. </summary>
        float GetElevation(GeoCoordinate coordinate);

        /// <summary> Gets elevation for given map point. </summary>
        float GetElevation(MapPoint point);

        /// <summary> Sets coordinate correspongin for (0,0). </summary>
        void SetNullPoint(GeoCoordinate coordinate);
    }
}
