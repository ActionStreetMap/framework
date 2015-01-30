namespace ActionStreetMap.Maps.Formats
{
    /// <summary> Interfaces of map data reader. </summary>
    internal interface IReader
    {
        /// <summary>  Reads whole map data file. </summary>
        void Read(ReaderContext context);
    }
}
