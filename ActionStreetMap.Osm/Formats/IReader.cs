namespace ActionStreetMap.Osm.Formats
{
    /// <summary>
    ///     Interfaces of map data reader.
    /// </summary>
    internal interface IReader
    {
        /// <summary>
        ///     Reads whole map data file.
        /// </summary>
        void Read();
    }
}
