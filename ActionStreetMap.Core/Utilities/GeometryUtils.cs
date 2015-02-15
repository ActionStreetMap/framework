using System;
using System.Collections.Generic;

namespace ActionStreetMap.Core.Utilities
{
    /// <summary> Contains some generic geometry utility methods. </summary>
    internal class GeometryUtils
    {
        #region Triangle specific functions

        /// <summary>
        ///     Checks whether point is located in triangle
        ///     http://stackoverflow.com/questions/13300904/determine-whether-point-lies-inside-triangle
        /// </summary>
        public static bool IsPointInTriangle(MapPoint p, MapPoint p1, MapPoint p2, MapPoint p3)
        {
            float alpha = ((p2.Y - p3.Y) * (p.X - p3.X) + (p3.X - p2.X) * (p.Y - p3.Y)) /
                          ((p2.Y - p3.Y) * (p1.X - p3.X) + (p3.X - p2.X) * (p1.Y - p3.Y));
            float beta = ((p3.Y - p1.Y) * (p.X - p3.X) + (p1.X - p3.X) * (p.Y - p3.Y)) /
                         ((p2.Y - p3.Y) * (p1.X - p3.X) + (p3.X - p2.X) * (p1.Y - p3.Y));
            float gamma = 1.0f - alpha - beta;

            return alpha > 0 && beta > 0 && gamma > 0;
        }

        #endregion

        #region Angle specific functions

        /// <summary> Sorts segments by angle. </summary>
        /// <param name="original">Original point.</param>
        /// <param name="pivot">Pivot point.</param>
        /// <param name="choices">List of choice points.</param>
        /// <returns>Sorted list.</returns>
        public static void SortByAngle(MapPoint original, MapPoint pivot, List<MapPoint> choices)
        {
            choices.Sort((v1, v2) => GetTurnAngle(original, pivot, v1).CompareTo(GetTurnAngle(original, pivot, v2)));
        }

        /// <summary> Gets angle in degrees between sigments created by points. </summary>
        public static double GetTurnAngle(MapPoint original, MapPoint pivot, MapPoint choice)
        {
            var angle1 = Math.Atan2(original.Y - pivot.Y, original.X - pivot.X);
            var angle2 = Math.Atan2(choice.Y - pivot.Y, choice.X - pivot.X);
            var angleDiff = (180 / Math.PI * (angle2 - angle1));

            return angleDiff > 0 ? 360 - angleDiff : -angleDiff;
        }

        #endregion

        // NOTE not used but keep it so far.
        /*
        #region Arc specific functions

        /// <summary> 
        ///     Creates rounded corner between segments. 
        ///     See http://stackoverflow.com/questions/24771828/algorithm-for-creating-rounded-corners-in-a-polygon
        /// </summary>
        public static List<MapPoint> CreateRoundedCorner(MapPoint angularPoint, MapPoint p1, MapPoint p2, 
            float elevation, float radius, float degreePerPoint, IObjectPool objectPool)
        {
            // vector 1
            double dx1 = angularPoint.X - p1.X;
            double dy1 = angularPoint.Y - p1.Y;

            // vector 2
            double dx2 = angularPoint.X - p2.X;
            double dy2 = angularPoint.Y - p2.Y;

            // angle between vector 1 and vector 2 divided by 2
            double angle = (Math.Atan2(dy1, dx1) - Math.Atan2(dy2, dx2))/2;

            // the length of segment between angular point and the points of intersection with the circle of a given radius
            double tan = Math.Abs(Math.Tan(angle));
            double segment = radius/tan;

            // check the segment
            double length1 = GetLength(dx1, dy1);
            double length2 = GetLength(dx2, dy2);

            double length = Math.Min(length1, length2);

            if (segment > length)
            {
                segment = length;
                radius = (float) (length*tan);
            }

            // points of intersection are calculated by the proportion between the coordinates of the vector, length of vector and the length of the segment.
            var p1Cross = GetProportionPoint(angularPoint, elevation, segment, length1, dx1, dy1);
            var p2Cross = GetProportionPoint(angularPoint, elevation, segment, length2, dx2, dy2);

            // calculation of the coordinates of the circle center by the addition of angular vectors.
            double dx = angularPoint.X*2 - p1Cross.X - p2Cross.X;
            double dy = angularPoint.Y*2 - p1Cross.Y - p2Cross.Y;

            double L = GetLength(dx, dy);
            double d = GetLength(segment, radius);

            var circlePoint = GetProportionPoint(angularPoint, elevation, d, L, dx, dy);

            // startAngle and EndAngle of arc
            var startAngle = Math.Atan2(p1Cross.Y - circlePoint.Y, p1Cross.X - circlePoint.X);
            var endAngle = Math.Atan2(p2Cross.Y - circlePoint.Y, p2Cross.X - circlePoint.X);

            // sweep angle
            var sweepAngle = endAngle - startAngle;

            // some additional checks
            if (sweepAngle < 0)
            {
                startAngle = endAngle;
                sweepAngle = -sweepAngle;
            }

            if (sweepAngle > Math.PI)
                sweepAngle = Math.PI - sweepAngle;

            return CreateArc(circlePoint, p1Cross, p2Cross, elevation, startAngle, sweepAngle, radius, degreePerPoint, objectPool);
        }

        private static double GetLength(double dx, double dy)
        {
            return Math.Sqrt(dx * dx + dy * dy);
        }

        private static MapPoint GetProportionPoint(MapPoint point, float elevation, double segment, double length, double dx, double dy)
        {
            double factor = segment / length;
            return new MapPoint((float)(point.X - dx * factor), (float)(point.Y - dy * factor), elevation);
        }

        private static List<MapPoint> CreateArc(MapPoint circlePoint, MapPoint startPoint, MapPoint endPoint,
            float elevation, double startAngle, double sweepAngle, float radius, float degreePerPoint, IObjectPool objectPool)
        {
            const double degreeInRad = 180 / Math.PI;

            var degrees = Math.Abs(sweepAngle * degreeInRad);
            int sign = Math.Sign(sweepAngle);
            if (sign < 0) degrees = 180 - degrees;

            int pointCount = (int)Math.Ceiling(degrees / degreePerPoint);
            var points = objectPool.NewList<MapPoint>(pointCount + 1);
            points.Add(sign < 0 ? endPoint : startPoint);
            for (int i = 1; i < pointCount; i++)
            {
                var pointX = (float)(circlePoint.X + Math.Cos(startAngle + sign * (double)(i * degreePerPoint) / degreeInRad) * radius);
                var pointY = (float)(circlePoint.Y + Math.Sin(startAngle + sign * (double)(i * degreePerPoint) / degreeInRad) * radius);
                points.Add(new MapPoint(pointX, pointY, elevation));
            }
            points.Add(sign < 0 ? startPoint : endPoint);
            // NOTE this is important as we want to to keep the same point traversal order
            if(sign < 0) points.Reverse();

            return points;
        }

        #endregion
*/
        
        #region Line specific functions

        public static MapPoint GetIntersectionPoint(MapSegment first, MapSegment second, float elevation)
        {
            float a1 = first.End.Y - first.Start.Y;
            float b1 = first.Start.X - first.End.X;
            float c1 = a1 * first.Start.X + b1 * first.Start.Y;

            // Get A,B,C of second line - points : ps2 to pe2
            float a2 = second.End.Y - second.Start.Y;
            float b2 = second.Start.X - second.End.X;
            float c2 = a2 * second.Start.X + b2 * second.Start.Y;

            // Get delta and check if the lines are parallel
            float delta = a1 * b2 - a2 * b1;
            if (Math.Abs(delta) < float.Epsilon)
            {
                // should share the same point - we will use it
                if (first.End == second.Start)
                    return first.End;
                throw new ArgumentException("Segments are parallel");
            }

            return new MapPoint(
                (b2 * c1 - b1 * c2) / delta,
                (a1 * c2 - a2 * c1) / delta,
                elevation);
        }

        /// <summary> Gets offset line with given width for needed side. </summary>
        public static MapSegment GetOffsetLine(MapPoint point1, MapPoint point2, float width, bool isLeft)
        {
            float length = point1.DistanceTo(point2);
            float dxLi = (point2.X - point1.X) / length * width;
            float dyLi = (point2.Y - point1.Y) / length * width;

            if (isLeft)
            {
                // segment moved to the left
                float lX1 = point1.X - dyLi;
                float lY1 = point1.Y + dxLi;
                float lX2 = point2.X - dyLi;
                float lY2 = point2.Y + dxLi;
                return new MapSegment(new MapPoint(lX1, lY1), new MapPoint(lX2, lY2));
            }

            // segment moved to the right
            float rX1 = point1.X + dyLi;
            float rY1 = point1.Y - dxLi;
            float rX2 = point2.X + dyLi;
            float rY2 = point2.Y - dxLi;
            
            return new MapSegment(new MapPoint(rX1, rY1), new MapPoint(rX2, rY2));
        }

        /// <summary> Gets T line. </summary>
        public static MapSegment GetTSegment(List<MapPoint> points, float width, float elevation, bool connectedToEnd)
        {
            var point1 = points[connectedToEnd ? 0 : points.Count - 1];
            var point2 = points[connectedToEnd ? points.Count - 1: 0];

            float length = point1.DistanceTo(point2);
            float dxLi = (point2.X - point1.X) / length * width;
            float dyLi = (point2.Y - point1.Y) / length * width;

            float lX2 = point2.X - dyLi;
            float lY2 = point2.Y + dxLi;
            float rX2 = point2.X + dyLi;
            float rY2 = point2.Y - dxLi;
            return new MapSegment(new MapPoint(lX2, lY2, elevation), new MapPoint(rX2, rY2, elevation));
        }

        /// <summary> Gets the closest point (perpendicular) on line. </summary>
        public static MapPoint GetClosestPointOnLine(MapPoint point, MapPoint lineStart, MapPoint lineEnd)
        {
            // first convert line to normalized unit vector
            var dx = lineEnd.X - lineStart.X;
            var dy = lineEnd.Y - lineStart.Y;
            var mag = (float) Math.Sqrt(dx * dx + dy * dy);
            dx /= mag;
            dy /= mag;

            // translate the point and get the dot product
            var lambda = (dx * (point.X - lineStart.X)) + (dy * (point.Y - lineStart.Y));

            return new MapPoint((dx * lambda) + lineStart.X, (dy * lambda) + lineStart.Y);
        }

        #endregion

        #region Helper primitives

        internal struct MapSegment
        {
            public MapPoint Start;
            public MapPoint End;

            public MapSegment(MapPoint start, MapPoint end)
            {
                Start = start;
                End = end;
            }
        }

        #endregion
    }
}
