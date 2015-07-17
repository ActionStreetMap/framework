using System;
using System.Collections.Generic;
using ActionStreetMap.Core.Geometry.Clipping;

namespace ActionStreetMap.Core.Geometry.Utils
{
    internal static class ClipperUtils
    {
        public static double CalcMinDistance(IntPoint point, List<IntPoint> footPrint)
        {
            var distance = double.MaxValue;

            var lastIndex = footPrint.Count - 1;
            for (var i = 0; i <= lastIndex; i++)
            {
                var start = footPrint[i];
                var end = footPrint[i == lastIndex ? 0 : i + 1];
                var currDistance = LineToPointDistance2D(start, end, point, true);
                if (currDistance < distance)
                    distance = currDistance;
            }
            return distance;
        }

        /// <summary>
        ///     Compute the distance from AB to C. if isSegment is true,
        ///     AB is a segment, not a line.
        /// </summary>
        public static double LineToPointDistance2D(IntPoint pointA, IntPoint pointB,
            IntPoint pointC, bool isSegment)
        {
            var dist = CrossProduct(pointA, pointB, pointC) / Distance(pointA, pointB);
            if (isSegment)
            {
                var dot1 = DotProduct(pointA, pointB, pointC);
                if (dot1 > 0)
                    return Distance(pointB, pointC);

                var dot2 = DotProduct(pointB, pointA, pointC);
                if (dot2 > 0)
                    return Distance(pointA, pointC);
            }
            return Math.Abs(dist);
        }

        private static double DotProduct(IntPoint pointA, IntPoint pointB, IntPoint pointC)
        {
            var AB = new IntPoint(pointB.X - pointA.X, pointB.Y - pointA.Y);
            var BC = new IntPoint(pointC.X - pointB.X, pointC.Y - pointB.Y);
            return AB.X * BC.X + AB.Y * BC.Y;
        }

        /// <summary> Compute the cross product AB x AC </summary>
        private static double CrossProduct(IntPoint pointA, IntPoint pointB, IntPoint pointC)
        {
            var AB = new IntPoint(pointB.X - pointA.X, pointB.Y - pointA.Y);
            var AC = new IntPoint(pointC.X - pointA.X, pointC.Y - pointA.Y);
            return AB.X * AC.Y - AB.Y * AC.X;
        }

        public static double Distance(IntPoint p1, IntPoint p2)
        {
            var dx = p1.X - p2.X;
            var dy = p1.Y - p2.Y;
            return Math.Sqrt(dx * dx + dy * dy);
        }
    }
}
