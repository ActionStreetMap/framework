using System.Collections.Generic;
using ActionStreetMap.Infrastructure.Utilities;

namespace ActionStreetMap.Core.Geometry.Utils
{
    internal class PolygonUtils
    {
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
                double dx1 = points[(i + 2) % count].X - points[(i + 1) % count].X;
                double dy1 = points[(i + 2) % count].Y - points[(i + 1) % count].Y;
                double dx2 = points[i].X - points[(i + 1) % count].X;
                double dy2 = points[i].Y - points[(i + 1) % count].Y;
                double crossProduct = dx1 * dy2 - dy1 * dx2;
                if (i == 0)
                    sign = crossProduct > 0;
                else if (sign != (crossProduct > 0))
                    return false;

            }
            return true;
        }

        /// <summary> Check if polygon is clockwise. </summary>
        /// <param name="polygon"> List of polygon points. </param>
        /// <returns> If polygon is clockwise. </returns>
        public static bool IsClockwisePolygon(List<Vector2d> polygon)
        {
            return Area(polygon) < 0;
        }

        /// <summary> Calculate area of polygon outline. For clockwise are will be less then. </summary>
        /// <param name="polygon">List of polygon points.</param>
        /// <returns> Area. </returns>
        public static double Area(List<Vector2d> polygon)
        {
            var n = polygon.Count;
            double A = 0.0f;
            for (int p = n - 1, q = 0; q < n; p = q++)
                A += polygon[p].X*polygon[q].Y - polygon[q].X*polygon[p].Y;

            return A*0.5f;
        }

        /// <summary> Always returns points ordered as counter clockwise. </summary>
        /// <param name="polygon"> Polygon as list of points. </param>
        /// <returns> Counter clockwise polygon.</returns>
        public static List<Vector2d> MakeCounterClockwise(List<Vector2d> polygon)
        {
            if (IsClockwisePolygon(polygon))
                polygon.Reverse();
            return polygon;
        }

        /// <summary>
        ///     Test if point is inside polygon.
        ///     <see cref="http://en.wikipedia.org/wiki/Point_in_polygon" />
        ///     <see cref="http://en.wikipedia.org/wiki/Even-odd_rule" />
        ///     <see cref="http://paulbourke.net/geometry/insidepoly/" />
        /// </summary>
        public static bool IsPointInsidePolygon(Vector2d point, List<Vector2d> points)
        {
            var numpoints = points.Count;

            if (numpoints < 3)
                return false;

            var it = 0;
            var first = points[it];
            var oddNodes = false;

            for (var i = 0; i < numpoints; i++)
            {
                var node1 = points[it];
                it++;
                var node2 = i == numpoints - 1 ? first : points[it];

                var x = point.X;
                var y = point.Y;

                if (node1.Y < y && node2.Y >= y || node2.Y < y && node1.Y >= y)
                {
                    if (node1.X + (y - node1.Y)/(node2.Y - node1.Y)*(node2.X - node1.X) < x)
                        oddNodes = !oddNodes;
                }
            }

            return oddNodes;
        }

        /// <summary> Calcs center of polygon. </summary>
        /// <param name="polygon">Polygon.</param>
        /// <returns>Center of polygon.</returns>
        public static Vector2d GetCentroid(List<Vector2d> polygon)
        {
            var centroidX = 0.0;
            var centroidY = 0.0;

            for (int i = 0; i < polygon.Count; i++)
            {
                centroidX += polygon[i].X;
                centroidY += polygon[i].Y;
            }
            centroidX /= polygon.Count;
            centroidY /= polygon.Count;

            return (new Vector2d(centroidX, centroidY));
        }

        /// <summary> Simplifies polygon using Douglas Peucker algorithim. </summary>
        /// <param name="source">Source.</param>
        /// <param name="destination">Destination.</param>
        /// <param name="tolerance">Tolerance.</param>
        /// <param name="objectPool">Object pool.</param>
        public static void Simplify(List<Vector2d> source, List<Vector2d> destination,
            float tolerance, IObjectPool objectPool)
        {
            DouglasPeuckerReduction.Reduce(source, destination, tolerance, objectPool);
        }


    }
}