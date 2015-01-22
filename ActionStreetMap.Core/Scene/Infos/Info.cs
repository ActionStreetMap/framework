namespace ActionStreetMap.Core.Scene.Infos
{
    /// <summary>
    ///     This class is associated with OSM node.
    /// </summary>
    public class Info
    {
        /// <summary>
        ///     Id of this info.
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        ///     Gets or sets style key.
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        ///     Gets or sets geocoordinate.
        /// </summary>
        public GeoCoordinate Coordinate { get; set; }
    }
}
