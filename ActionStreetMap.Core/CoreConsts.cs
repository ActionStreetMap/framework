namespace ActionStreetMap.Core
{
    /// <summary> Defines core consts. </summary>
    internal static class CoreConsts
    {
        /// <summary> Max zoom level (~ 1:1,000). See <see cref="http://wiki.openstreetmap.org/wiki/Zoom_levels"/>. </summary>
        public const int MaxZoomLevel = 19;

        /// <summary> Min zoom level (~ 1:500 Mio). See <see cref="http://wiki.openstreetmap.org/wiki/Zoom_levels"/>. </summary>
        public const int MinZoomLevel = 0;
    }
}
