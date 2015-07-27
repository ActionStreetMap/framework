using System.Collections.Generic;
using ActionStreetMap.Core.Utils;

namespace ActionStreetMap.Core.Geometry.Utils
{
    /// <summary> Provids some helper methods for points. </summary>
    internal static class PointUtils
    {
        #region Vector operations

        /// <summary> Compute the dot product AB . AC. </summary>
        public static double DotProduct(Vector2d pointA, Vector2d pointB, Vector2d pointC)
        {
            var AB = new Vector2d(pointB.X - pointA.X, pointB.Y - pointA.Y);
            var BC = new Vector2d(pointC.X - pointB.X, pointC.Y - pointB.Y);
            return AB.X * BC.X + AB.Y * BC.Y;
        }

        /// <summary> Compute the cross product AB x AC. </summary>
        public static double CrossProduct(Vector2d pointA, Vector2d pointB, Vector2d pointC)
        {
            var AB = new Vector2d(pointB.X - pointA.X, pointB.Y - pointA.Y);
            var AC = new Vector2d(pointC.X - pointA.X, pointC.Y - pointA.Y);
            return AB.X * AC.Y - AB.Y * AC.X;
        }

        #endregion

        #region Points for polygons

        /// <summary> Converts geo coordinates to map coordinates without elevation data. </summary>
        /// <param name="center">Map center.</param>
        /// <param name="geoCoordinates">Geo coordinates.</param>
        /// <param name="points">Output points.</param>
        public static void GetClockwisePolygonPoints(GeoCoordinate center, List<GeoCoordinate> geoCoordinates,
            List<Vector2d> points)
        {
            GetPointsNoElevation(center, geoCoordinates, points, true);
        }

        /// <summary> Converts geo coordinates to map coordinates without sorting. </summary>
        /// <param name="center">Map center.</param>
        /// <param name="geoCoordinates">Geo coordinates.</param>
        /// <param name="points">Output points.</param>
        public static void GetPolygonPoints(GeoCoordinate center, List<GeoCoordinate> geoCoordinates,
            List<Vector2d> points)
        {
            GetPointsNoElevation(center, geoCoordinates, points, false);
        }

        private static void GetPointsNoElevation(GeoCoordinate center, List<GeoCoordinate> geoCoordinates, 
            List<Vector2d> verticies, bool sort)
        {
            var length = geoCoordinates.Count;

            if (geoCoordinates[0] == geoCoordinates[length - 1])
                length--;

            for (int i = 0; i < length; i++)
            {
                // skip the same points in sequence
                if (i == 0 || geoCoordinates[i] != geoCoordinates[i - 1])
                {
                    var point = GeoProjection.ToMapCoordinate(center, geoCoordinates[i]);
                    verticies.Add(point);
                }
            }

            if (sort)
                SortVertices(verticies);
        }

        #endregion

        /// <summary> Tests whether points represent convex polygon. </summary>
        /// <param name="points">Polygon points.</param>
        /// <returns>True if polygon is convex.</returns>
        public static bool IsConvex(List<Vector2d> points)
        {
            int count = points.Count;
            if (count < 4)
                return true;
            bool sign = false;
            for (int i = 0; i < count; i++)
            {
                double dx1 = points[(i + 2)%count].X - points[(i + 1)%count].X;
                double dy1 = points[(i + 2)%count].Y - points[(i + 1)%count].Y;
                double dx2 = points[i].X - points[(i + 1)%count].X;
                double dy2 = points[i].Y - points[(i + 1)%count].Y;
                double crossProduct = dx1*dy2 - dy1*dx2;
                if (i == 0)
                    sign = crossProduct > 0;
                else if (sign != (crossProduct > 0))
                    return false;

            }
            return true;
        }

        /// <summary> Sorts verticies in clockwise order. </summary>
        private static void SortVertices(List<Vector2d> verticies)
        {
            var direction = PointsDirection(verticies);

            switch (direction)
            {
                case PolygonDirection.CountClockwise:
                    verticies.Reverse();
                    break;
                case PolygonDirection.Clockwise:
                    break;
                default:
                    throw new AlgorithmException(Strings.BugInPolygonOrderAlgorithm);
            }
        }

        private static PolygonDirection PointsDirection(List<Vector2d> points)
        {
            if (points.Count < 3)
                return PolygonDirection.Unknown;

            // Calculate signed area
            // http://en.wikipedia.org/wiki/Shoelace_formula
            double sum = 0.0;
            for (int i = 0; i < points.Count; i++)
            {
                Vector2d v1 = points[i];
                Vector2d v2 = points[(i + 1) % points.Count];
                sum += (v2.X - v1.X) * (v2.Y + v1.Y);
            }
            return sum > 0.0 ? PolygonDirection.Clockwise : PolygonDirection.CountClockwise;
        }

        internal enum PolygonDirection
        {
            Unknown,
            Clockwise,
            CountClockwise
        }
    }
}
