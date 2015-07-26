using System;
using System.Collections.Generic;
using ActionStreetMap.Core.Geometry;

namespace ActionStreetMap.Explorer.Geometry.Utils
{
    internal static class LineUtils
    {
        #region Intermediate points

        public static List<Vector2d> DividePolyline(List<Vector2d> original, float maxDistance)
        {
            return DividePolyline(original, maxDistance, 5f);
        }

        public static List<Vector2d> DividePolyline(List<Vector2d> original, float maxDistance, float threshold)
        {
            var result = new List<Vector2d>(original.Count);
            for (int i = 1; i < original.Count; i++)
            {
                var point1 = original[i - 1];
                var point2 = original[i];

                result.Add(point1);

                var distance = point1.DistanceTo(point2);
                while (distance > maxDistance)
                {
                    var ration = maxDistance / distance;
                    point1 = new Vector2d(
                        point1.X + ration * (point2.X - point1.X),
                        point1.Y + ration * (point2.Y - point1.Y));

                    distance = point1.DistanceTo(point2);
                    // we should prevent us to have small distances between points when we have turn
                    if (distance < threshold)
                        break;

                    result.Add(point1);
                }

            }
            // add last as we checked previous item in cycle
            var last = original[original.Count - 1];
            result.Add(last);
            return result;
        }

        public static void DivideLine(Vector2d start, Vector2d end, List<Vector2d> result, float maxDistance)
        {
            var point1 = start;
            var point2 = end;

            result.Add(point1);

            var distance = point1.DistanceTo(point2);
            while (distance > maxDistance)
            {
                var ration = maxDistance / distance;
                point1 = new Vector2d(point1.X + ration*(point2.X - point1.X),
                    point1.Y + ration*(point2.Y - point1.Y));

                distance = point1.DistanceTo(point2);
                result.Add(point1);
            }

            result.Add(end);
        }

        public static Vector2d GetNextIntermediatePoint(Vector2d point1, Vector2d point2, float maxDistance)
        {
            var distance = point1.DistanceTo(point2);
            if (distance > maxDistance)
            {
                var ration = maxDistance / distance;
                var next = new Vector2d(point1.X + ration*(point2.X - point1.X),
                    point1.Y + ration*(point2.Y - point1.Y));

                return next;
            }

            return point2; // NOTE should we return point2?
        }

        #endregion

        #region Distance

        /// <summary> Compute the distance from AB to C if isSegment is true, AB is a segment, not a line. </summary>
        public static double LineToPointDistance2D(Vector2d pointA, Vector2d pointB, Vector2d pointC,
            bool isSegment)
        {
            var dist = PointUtils.CrossProduct(pointA, pointB, pointC) / pointA.DistanceTo(pointB);
            if (isSegment)
            {
                var dot1 = PointUtils.DotProduct(pointA, pointB, pointC);
                if (dot1 > 0)
                    return pointB.DistanceTo(pointC);

                var dot2 = PointUtils.DotProduct(pointB, pointA, pointC);
                if (dot2 > 0)
                    return pointA.DistanceTo(pointC);
            }
            return Math.Abs(dist);
        } 

        #endregion
    }
}
