namespace ActionStreetMap.Osm.Index
{
    internal static class Consts
    {
        public const uint ScaleFactor = 10000000;

        #region Path consts

        public const string HeaderFileName = @"header.txt";
        /// <summary>
        ///     Path to header file which stores information about bounding box, city name, etc.
        /// </summary>
        public const string HeaderPathFormat = @"{0}/" + HeaderFileName;
        /// <summary>
        ///     Path to tag usage file which
        /// </summary>
        public const string KeyValueUsagePathFormat = @"{0}/tags.usg.txt";
        public const string KeyValueStorePathFormat = @"{0}/tags.dat.bytes";
        public const string KeyValueIndexPathFormat = @"{0}/tags.idx.bytes";
        public const string ElementStorePathFormat = @"{0}/elements.dat.bytes";
        public const string SpatialIndexPathFormat = @"{0}/spatial.idx.bytes";

        #endregion
    }
}
