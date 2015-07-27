using System;
using System.Collections.Generic;
using System.Linq;
using ActionStreetMap.Core;
using ActionStreetMap.Core.Geometry;
using ActionStreetMap.Core.Utils;

namespace ActionStreetMap.Explorer.Geometry.Utils
{
    /// <summary> Provides circle helper methods. </summary>
    internal static class CircleUtils
    {
        private const double ConvertionCoefficient = (6378137 * Math.PI) / 180;

        /// <summary> Gets circle from given list of geo coordinates. </summary>
        public static void GetCircle(GeoCoordinate relativeNullPoint, List<GeoCoordinate> points,
            out double radius, out Vector2d center)
        {
            var minLat = points.Min(a => a.Latitude);
            var maxLat = points.Max(a => a.Latitude);

            var minLon = points.Min(a => a.Longitude);
            var maxLon = points.Max(a => a.Longitude);

            var centerLat = (float)(minLat + (maxLat - minLat) / 2);
            var centerLon = (float)(minLon + (maxLon - minLon) / 2);
            center = GeoProjection.ToMapCoordinate(relativeNullPoint, new GeoCoordinate(centerLat, centerLon));
            radius = (float)((maxLat - minLat) * ConvertionCoefficient) / 2;
        }
    }
}
