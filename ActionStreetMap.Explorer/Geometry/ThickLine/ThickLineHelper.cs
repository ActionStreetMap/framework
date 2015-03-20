using System;
using ActionStreetMap.Core;
using ActionStreetMap.Explorer.Geometry.Primitives;
using ActionStreetMap.Explorer.Geometry.Utils;
using UnityEngine;

namespace ActionStreetMap.Explorer.Geometry.ThickLine
{
    internal static class ThickLineHelper
    {
        public static ThickLineSegment GetThickSegment(MapPoint point1, MapPoint point2, float width)
        {
            float length = point1.DistanceTo(point2);

            float dxLi = (point2.X - point1.X) / length * width;
            float dyLi = (point2.Y - point1.Y) / length * width;

            // segment moved to the left
            float lX1 = point1.X - dyLi;
            float lY1 = point1.Y + dxLi;
            float lX2 = point2.X - dyLi;
            float lY2 = point2.Y + dxLi;

            // segment moved to the right
            float rX1 = point1.X + dyLi;
            float rY1 = point1.Y - dxLi;
            float rX2 = point2.X + dyLi;
            float rY2 = point2.Y - dxLi;

            var leftSegment = new Segment(new Vector3(lX1, point1.Elevation, lY1), new Vector3(lX2, point2.Elevation, lY2));
            var rightSegment = new Segment(new Vector3(rX1, point1.Elevation, rY1), new Vector3(rX2, point2.Elevation, rY2));

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
