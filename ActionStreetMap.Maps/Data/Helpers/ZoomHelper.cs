using System;
using System.Collections.Generic;
using ActionStreetMap.Core;
using ActionStreetMap.Core.Utils;

namespace ActionStreetMap.Maps.Data.Helpers
{
    /// <summary> Provides helper functionality to work with zoom levels. </summary>
    internal static class ZoomHelper
    {
        /// <summary> Meters per pixel map. See <see cref="http://wiki.openstreetmap.org/wiki/Zoom_levels"/>. </summary>
        private static readonly Dictionary<int, float> ZoomResolutionMap = new Dictionary<int, float>()
        {
             {0, 156412}, {1, 78206}, {2, 39103}, {3, 19551}, {4, 9776},
             {5, 4888}, {6, 2444}, {7, 1222}, {8, 610.984f}, {9, 305.492f},
             {10, 152.746f}, {11, 76.373f}, {12, 38.187f}, {13, 19.093f}, {14, 9.547f},
             {15, 4.773f}, {16, 2.387f}, {17, 1.193f}, {18, 0.596f}, {19, 0.298f},
        };

        /// <summary> Gets minimal envelop's margin for given zoom level.</summary>
        public static long GetMinMargin(int zoomLevel)
        {
            if (zoomLevel < CoreConsts.MinZoomLevel || zoomLevel > CoreConsts.MaxZoomLevel)
                throw new ArgumentException(Strings.InvalidZoomLevel);

            // show everything on this zoom level
            if (zoomLevel == CoreConsts.MaxZoomLevel) return 0;

            // some magic formula optimized for zoom level 15
            // TODO find better way to express zoom levels
            var minSizeInMeters = ZoomResolutionMap[zoomLevel] * 85.2f / 4;

            return GetMargin(minSizeInMeters);
        }

        /// <summary> Gets margin for bounding box of desired size. </summary>
        private static long GetMargin(float size)
        {
            var start = new MapPoint(0, 0);
            // (h * cos(45), h * sin(45))
            var end = new MapPoint((float)(size * 0.52532198881), (float)(size * 0.85090352453)); 
            var geoCenter = new GeoCoordinate(52, 13);

            var point1 = GeoProjection.ToGeoCoordinate(geoCenter, start);
            var point2 = GeoProjection.ToGeoCoordinate(geoCenter, end);

           return (long) (((point2.Longitude - point1.Longitude) + (point2.Latitude - point1.Latitude)) * MapConsts.ScaleFactor);
        }
    }
}
