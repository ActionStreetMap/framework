namespace ActionStreetMap.Maps.Data
{
    internal static class MapConsts
    {
        public const uint ScaleFactor = 10000000;

        #region Zooming

        /// <summary> Max supported zoom level. </summary>
        public const int MaxZoomLevel = 19;

        /// <summary> OVerview zoom level. </summary>
        public const int OverviewZoomLevel = 15;

        /// <summary> Min supported zoom level. </summary>
        public const int MinZoomLevel = 0;

        #endregion

        #region Path consts

        public const string HeaderFileName = @"header.txt";
        /// <summary>
        ///     Path to header file which stores information about bounding box, city name, etc.
        /// </summary>
        public const string HeaderPathFormat = @"{0}/" + HeaderFileName;
        /// <summary>
        ///     Path to tag usage file which
        /// </summary>
        public const string KeyValueUsagePathFormat = @"{0}/tags.usg.bytes";
        public const string KeyValueStorePathFormat = @"{0}/tags.dat.bytes";
        public const string KeyValueIndexPathFormat = @"{0}/tags.idx.bytes";
        public const string ElementStorePathFormat = @"{0}/elements.dat.bytes";
        public const string SpatialIndexPathFormat = @"{0}/spatial.idx.bytes";

        #endregion
    }
}
