using System;
using ActionStreetMap.Core;
using ActionStreetMap.Core.Geometry;
using ActionStreetMap.Explorer.Geometry.Primitives;
using ActionStreetMap.Explorer.Geometry.Utils;
using UnityEngine;

namespace ActionStreetMap.Explorer.Geometry.ThickLine
{
    internal static class ThickLineHelper
    {
        public static ThickLineSegment GetThickSegment(Vector2d point1, Vector2d point2, float width)
        {
            var length = point1.DistanceTo(point2);

            var dxLi = (point2.X - point1.X) / length * width;
            var dyLi = (point2.Y - point1.Y) / length * width;

            // segment moved to the left
            var lX1 = point1.X - dyLi;
            var lY1 = point1.Y + dxLi;
            var lX2 = point2.X - dyLi;
            var lY2 = point2.Y + dxLi;

            // segment moved to the right
            var rX1 = point1.X + dyLi;
            var rY1 = point1.Y - dxLi;
            var rX2 = point2.X + dyLi;
            var rY2 = point2.Y - dxLi;

            var leftSegment = new LineSegment2d(new Vector2d(lX1, lY1), new Vector2d(lX2, lY2));
            var rightSegment = new LineSegment2d(new Vector2d(rX1, rY1), new Vector2d(rX2, rY2));

            return new ThickLineSegment(leftSegment, rightSegment);
        }

        public static Direction GetDirection(ThickLineSegment first, ThickLineSegment second)
        {
            // just straight line with shared point
            var area = first.Left.Start.x * (first.Left.End.z - second.Left.End.z) +
                       first.Left.End.x * (second.Left.End.z - first.Left.Start.z) +
                       second.Left.End.x * (first.Left.Start.z - first.Left.End.z);
            if (Math.Abs(area) < 0.1)
                return Direction.Straight;

            if (SegmentUtils.Intersect(first.Left, second.Left))
                return Direction.Left;

            if (SegmentUtils.Intersect(first.Right, second.Right))
                return Direction.Right;

            return Direction.Straight;
        }

        internal enum Direction
        {
            Straight,
            Left,
            Right
        }
    }
}
