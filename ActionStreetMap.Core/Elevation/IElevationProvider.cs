namespace ActionStreetMap.Core.Elevation
{
    /// <summary> Defines behavior of elevation provider. </summary>
    public interface IElevationProvider
    {
        /// <summary> Checks whether elevation data for given coordinate is present in map data. </summary>
        /// <returns>True, if data is here.</returns>
        bool HasElevation(double latitude, double longitude);

        /// <summary> Gets elevation for given latitude and longitude. </summary>
        /// <param name="latitude">Latitude.</param>
        /// <param name="longitude">Longitude.</param>
        /// <returns>Elevation.</returns>
        float GetElevation(double latitude, double longitude);
    }
}
