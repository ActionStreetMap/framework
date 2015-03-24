using System;
using System.Collections.Generic;
using System.Linq;
using ActionStreetMap.Core;
using ActionStreetMap.Core.Utils;
using ActionStreetMap.Infrastructure.Primitives;

namespace ActionStreetMap.Explorer.Geometry.Utils
{
    /// <summary> Provides circle helper methods. </summary>
    public static class CircleUtils
    {
        private const double ConvertionCoefficient = (6378137 * Math.PI) / 180;

        /// <summary> Gets circle. </summary>
        /// <param name="relativeNullPoint">Relative null point.</param>
        /// <param name="points">Geo coordinates.</param>
        /// <returns>Tuple which represents circle: Item1 is diameter, Item2 is shpere center.</returns>
        public static MutableTuple<float, MapPoint> GetCircle(GeoCoordinate relativeNullPoint, List<GeoCoordinate> points)
        {
            var minLat = points.Min(a => a.Latitude);
            var maxLat = points.Max(a => a.Latitude);

            var minLon = points.Min(a => a.Longitude);
            var maxLon = points.Max(a => a.Longitude);

            var centerLat = (float)(minLat + (maxLat - minLat) / 2);
            var centerLon = (float)(minLon + (maxLon - minLon) / 2);
            var sphereCenter = GeoProjection.ToMapCoordinate(relativeNullPoint,
                new GeoCoordinate(centerLat, centerLon));

            var diameter = (float)((maxLat - minLat) * ConvertionCoefficient);

            return new MutableTuple<float, MapPoint>(diameter, sphereCenter);
        }

        /// <summary> Gets circle. </summary>
        /// <param name="points">Map points.</param>
        /// <returns>Tuple which represents circle: Item1 is diameter, Item2 is shpere center.</returns>
        public static MutableTuple<float, MapPoint> GetCircle(List<MapPoint> points)
        {
            var minX = points.Min(a => a.X);
            var maxX = points.Max(a => a.X);

            var minY = points.Min(a => a.Y);
            var maxY = points.Max(a => a.Y);

            var centerX = (minX + (maxX - minX) / 2);
            var centerY = (minY + (maxY - minY) / 2);

            var sphereCenter = new MapPoint(centerX, centerY);

            var diameter = maxX - minX;

            return new MutableTuple<float, MapPoint>(diameter, sphereCenter);
        }
    }
}
