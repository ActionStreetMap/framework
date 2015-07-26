using System;
using ActionStreetMap.Core.Geometry;
using UnityEngine;

namespace ActionStreetMap.Explorer.Geometry.ThickLine
{
    /// <summary> Segment utils. </summary>
    public class SegmentUtils
    {
        /// <summary> Gets intersection point of two segments. </summary>
        public static Vector3 IntersectionPoint(LineSegment2d first, LineSegment2d second, float elevation)
        {
            var a1 = first.End.Y - first.Start.Y;
            var b1 = first.Start.X - first.End.X;
            var c1 = a1 * first.Start.X + b1 * first.Start.Y;

            // Get A,B,C of second line - points : ps2 to pe2
            var a2 = second.End.Y - second.Start.Y;
            var b2 = second.Start.X - second.End.X;
            var c2 = a2 * second.Start.X + b2 * second.Start.Y;

            // Get delta and check if the lines are parallel
            var delta = a1 * b2 - a2 * b1;
            if (Math.Abs(delta) < double.Epsilon)
            {
                // should share the same point - we will use it
                if (first.End == second.Start)
                    return new Vector3((float)first.End.X, elevation, (float)first.End.Y);
                throw new ArgumentException("Segments are parallel");
            }

            return new Vector3(
                (float)((b2*c1 - b1*c2)/delta),
                elevation,
                (float)((a1 * c2 - a2 * c1) / delta));
        }

        /// <summary> Returns true if segmens intersect. </summary>
        public static bool Intersect(LineSegment2d first, LineSegment2d second)
        {
            Vector2d a = first.End - first.Start;
            Vector2d b = second.Start - second.End;
            Vector2d c = first.Start - second.Start;

            var alphaNumerator = b.Y * c.X - b.X * c.Y;
            var alphaDenominator = a.Y * b.X - a.X * b.Y;
            var betaNumerator = a.X * c.Y - a.Y * c.X;
            var betaDenominator = alphaDenominator;

            bool doIntersect = true;

            if (Math.Abs(alphaDenominator) < float.MinValue || Math.Abs(betaDenominator) < float.MinValue)
            {
                doIntersect = false;
            }
            else
            {
                if (alphaDenominator > 0)
                {
                    if (alphaNumerator < 0 || alphaNumerator > alphaDenominator)
                    {
                        doIntersect = false;
                    }
                }
                else if (alphaNumerator > 0 || alphaNumerator < alphaDenominator)
                {
                    doIntersect = false;
                }

                if (doIntersect && betaDenominator > 0)
                {
                    if (betaNumerator < 0 || betaNumerator > betaDenominator)
                    {
                        doIntersect = false;
                    }
                }
                else if (betaNumerator > 0 || betaNumerator < betaDenominator)
                {
                    doIntersect = false;
                }
            }
            return doIntersect;
        }

        /// <summary> Gets parallel segment with given offset. </summary>
        public static LineSegment2d GetParallel(LineSegment2d segment, float offset)
        {
            double x1 = segment.Start.X, x2 = segment.End.X, z1 = segment.Start.Y, z2 = segment.End.Y;
            var l = (float)Math.Sqrt((x1 - x2) * (x1 - x2) + (z1 - z2) * (z1 - z2));

            var x1p = x1 + offset*(z2 - z1)/l;
            var x2p = x2 + offset*(z2 - z1)/l;
            var z1p = z1 + offset*(x1 - x2)/l;
            var z2p = z2 + offset*(x1 - x2)/l;

            return new LineSegment2d(new Vector2d(x1p, z1p), new Vector2d(x2p, z2p));
        }
    }
}