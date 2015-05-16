namespace ActionStreetMap.Maps
{
    internal static class Strings
    {
        public static string SearchNotSupported = "Search doesn't support given IElementSource implementation (or it's null)";
        public static string NoOfflineNoOnlineElementSource = "Cannot find element source: no offline data found and online server is undefined";
        public static string ElementSourceCacheHit = "Found element source in cache: {0}";

        public static string NotSupportedMapFormat = "Unknown or not supported map format";
        public static string NoPresistentElementSourceFound = "No offline map data found for {0}, will query default server: {1}";
        public static string CannotFindSrtmData = "SRTM data cell not found: {0}";

        public static string LoadElevationFrom = "Load elevation from {0}..";
        public static string IndexBuildInMs = "Index is build in {0}ms";

        public static string InvalidZoomLevel = "Invalid zoom level.";
    }
}
