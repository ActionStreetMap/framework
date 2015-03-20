using System;
using ActionStreetMap.Explorer.Geometry.Primitives;
using UnityEngine;

namespace ActionStreetMap.Explorer.Geometry.Utils
{
    /// <summary> Segment utils. </summary>
    public class SegmentUtils
    {
        /// <summary> Gets intersection point of two segments. </summary>
        public static Vector3 IntersectionPoint(Segment first, Segment second)
        {
            float a1 = first.End.z - first.Start.z;
            float b1 = first.Start.x - first.End.x;
            float c1 = a1*first.Start.x + b1*first.Start.z;

            // Get A,B,C of second line - points : ps2 to pe2
            float a2 = second.End.z - second.Start.z;
            float b2 = second.Start.x - second.End.x;
            float c2 = a2*second.Start.x + b2*second.Start.z;

            // Get delta and check if the lines are parallel
            float delta = a1*b2 - a2*b1;
            if (Math.Abs(delta) < float.Epsilon)
            {
                // should share the same point - we will use it
                if (first.End == second.Start)
                    return first.End;
                throw new ArgumentException("Segments are parallel");
            }

            return new Vector3(
                (b2*c1 - b1*c2)/delta,
                first.End.y,
                (a1*c2 - a2*c1)/delta
                );
        }

        /// <summary> Returns true if segmens intersect. </summary>
        public static bool Intersect(Segment first, Segment second)
        {
            Vector2 a = new Vector2(first.End.x - first.Start.x, first.End.z - first.Start.z);
            Vector2 b = new Vector2(second.Start.x - second.End.x, second.Start.z - second.End.z);
            Vector2 c = new Vector2(first.Start.x - second.Start.x, first.Start.z - second.Start.z);

            float alphaNumerator = b.y*c.x - b.x*c.y;
            float alphaDenominator = a.y*b.x - a.x*b.y;
            float betaNumerator = a.x*c.y - a.y*c.x;
            float betaDenominator = alphaDenominator;

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
        public static Segment GetParallel(Segment segment, float offset)
        {
            float x1 = segment.Start.x, x2 = segment.End.x, z1 = segment.Start.z, z2 = segment.End.z;
            float l = (float) Math.Sqrt((x1 - x2)*(x1 - x2) + (z1 - z2)*(z1 - z2));

            var x1p = x1 + offset*(z2 - z1)/l;
            var x2p = x2 + offset*(z2 - z1)/l;
            var z1p = z1 + offset*(x1 - x2)/l;
            var z2p = z2 + offset*(x1 - x2)/l;

            return new Segment(new Vector3(x1p, segment.Start.y, z1p),
                new Vector3(x2p, segment.End.y, z2p));
        }
    }
}