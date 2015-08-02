using System;
using System.Collections.Generic;
using System.Linq;
using ActionStreetMap.Core.Utils;

namespace ActionStreetMap.Core.Geometry.Utils
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

            var centerLat = (minLat + (maxLat - minLat) / 2);
            var centerLon = (minLon + (maxLon - minLon) / 2);
            center = GeoProjection.ToMapCoordinate(relativeNullPoint, new GeoCoordinate(centerLat, centerLon));
            radius = (float)((maxLat - minLat) * ConvertionCoefficient) / 2;
        }

        /// <summary> Gets circle from given list of points. </summary>
        public static void GetCircle(List<Vector2d> points, out double radius, out Vector2d center)
        {
            var minX = points.Min(a => a.X);
            var maxX = points.Max(a => a.X);

            var minY = points.Min(a => a.Y);
            var maxY = points.Max(a => a.Y);

            var centerX = (minX + (maxX - minX) / 2);
            var centerY = (minY + (maxY - minY) / 2);

            center = new Vector2d(centerX, centerY);
            radius = (maxX - minX) / 2;
        }
    }
}
