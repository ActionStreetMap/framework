using System.Collections.Generic;
using ActionStreetMap.Core.Geometry.StraightSkeleton.Primitives;

namespace ActionStreetMap.Core.Geometry.StraightSkeleton.Utils
{
    internal class PolygonUtil
    {
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
    }
}