using System;
using ActionStreetMap.Infrastructure.Reactive;
namespace ActionStreetMap.Core.Elevation
{
    /// <summary> Defines behavior of elevation provider. </summary>
    public interface IElevationProvider
    {
        /// <summary> Checks whether elevation data for given coordinate is present in map data. </summary>
        /// <returns>True, if data is here.</returns>
        [Obsolete]
        bool HasElevation(double latitude, double longitude);

        /// <summary> Download elevation data from server. </summary>
        /// <param name="latitude">Latitude.</param>
        /// <param name="longitude">Longitude.</param>
        /// <returns>IObservable.</returns>
        [Obsolete]
        IObservable<Unit> Download(double latitude, double longitude);

        /// <summary> Checks whether elevation data for given bounding box is present in map data. </summary>
        /// <returns>True, if data is here.</returns>
        bool HasElevation(BoundingBox bbox);

        /// <summary> Download elevation data from server. </summary>
        /// <param name="bbox">Bounding box.</param>
        /// <returns>IObservable.</returns>
        IObservable<Unit> Download(BoundingBox bbox);

        /// <summary> Gets elevation for given latitude and longitude. </summary>
        /// <param name="latitude">Latitude.</param>
        /// <param name="longitude">Longitude.</param>
        /// <returns>Elevation.</returns>
        float GetElevation(double latitude, double longitude);
    }
}
