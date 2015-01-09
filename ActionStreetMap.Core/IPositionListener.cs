namespace ActionStreetMap.Core
{
    /// <summary>
    ///     Represents listener for map and geo coordinate changes.
    /// </summary>
    public interface IPositionListener
    {
        /// <summary>
        ///     Gets last updated geo position.
        /// </summary>
        GeoCoordinate CurrentPosition { get; }

        /// <summary>
        ///     Gets last updated map point.
        /// </summary>
        MapPoint CurrentPoint{ get; }

        /// <summary>
        ///     This geo coordinate is mapped to (0, 0, 0) map coordinate.
        /// </summary>
        GeoCoordinate RelativeNullPoint { get; set; }

        /// <summary>
        ///     Called when map position is changed. It should occur when character moves.
        /// </summary>
        /// <param name="position">New position.</param>
        void OnMapPositionChanged(MapPoint position);

        /// <summary>
        ///      Called when new relative null point is set.
        /// </summary>
        /// <param name="position">New rellative null point.</param>
        void OnGeoPositionChanged(GeoCoordinate position);
    }
}