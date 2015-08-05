using System;

namespace ActionStreetMap.Core.Geometry.Utils
{
    internal class Vector2dUtils
    {
        public static bool LineIntersects(Vector2d start1, Vector2d end1, Vector2d start2, Vector2d end2)
        {
            double d;
            return LineIntersects(start1, end1, start2, end2, out d);
        }

        public static bool LineIntersects(Vector2d start1, Vector2d end1, Vector2d start2, Vector2d end2, out double r)
        {
            r = 0;
            double denom = ((end1.X - start1.X) * (end2.Y - start2.Y)) - ((end1.Y - start1.Y) * (end2.X - start2.X));

            //  AB & CD are parallel 
            if (denom == 0)
                return false;

            double numer = ((start1.Y - start2.Y) * (end2.X - start2.X)) - ((start1.X - start2.X) * (end2.Y - start2.Y));

            r = numer / denom;

            double numer2 = ((start1.Y - start2.Y) * (end1.X - start1.X)) - ((start1.X - start2.X) * (end1.Y - start1.Y));

            var s = numer2 / denom;

            if ((r < 0 || r > 1) || (s < 0 || s > 1))
                return false;
            return true;
        }

        public static Vector2d GetPointAlongLine(Vector2d start, Vector2d end, double r)
        {
            return new Vector2d(
                Math.Round(start.X + (r*(end.X - start.X))),
                Math.Round((start.Y + (r*(end.Y - start.Y)))));
        }

        /// <summary>
        ///     Gets point on line. 
        ///     See http://stackoverflow.com/questions/5227373/minimal-perpendicular-vector-between-a-point-and-a-line
        /// </summary>
        public static Vector2d GetPointOnLine(Vector2d a, Vector2d b, Vector2d p)
        {
            var d = (a - b).Normalized();
            var x = a + d * (p - a).Dot(d);
            return x;
        }

        /// <summary> Checks whether point is on segment. Works for point which is on that line. </summary>
        public static bool IsPointOnSegment(Vector2d start, Vector2d end, Vector2d middlePoint)
        {
            return ((Math.Abs(start.X - end.X) > float.Epsilon &&
                     (end.X > start.X && middlePoint.X <= end.X && middlePoint.X >= start.X) ||
                     (end.X < start.X && middlePoint.X >= end.X && middlePoint.X <= start.X)) ||
                    ((end.Y > start.Y && middlePoint.Y <= end.Y && middlePoint.Y >= start.Y) ||
                     (end.Y < start.Y && middlePoint.Y >= end.Y && middlePoint.Y <= start.Y)));
        }

        public static Vector2d FromTo(Vector2d begin, Vector2d end)
        {
            return new Vector2d(end.X - begin.X, end.Y - begin.Y);
        }

        public static Vector2d OrthogonalLeft(Vector2d v)
        {
            return new Vector2d(-v.Y, v.X);
        }

        public static Vector2d OrthogonalRight(Vector2d v)
        {
            return new Vector2d(v.Y, -v.X);
        }

        /// <summary>
        ///     <see href="http://en.wikipedia.org/wiki/Vector_projection" />
        /// </summary>
        public static Vector2d OrthogonalProjection(Vector2d unitVector, Vector2d vectorToProject)
        {
            var n = new Vector2d(unitVector).Normalized();

            var px = vectorToProject.X;
            var py = vectorToProject.Y;

            var ax = n.X;
            var ay = n.Y;

            return new Vector2d(px*ax*ax + py*ax*ay, px*ax*ay + py*ay*ay);
        }

        public static Vector2d BisectorNormalized(Vector2d norm1, Vector2d norm2)
        {
            var e1v = OrthogonalLeft(norm1);
            var e2v = OrthogonalLeft(norm2);

            // 90 - 180 || 180 - 270
            if (norm1.Dot(norm2) > 0)
                return e1v + e2v;

            // 0 - 180
            var ret = new Vector2d(norm1);
            ret.Negate();
            ret += norm2;

            // 270 - 360
            if (e1v.Dot(norm2) < 0)
                ret.Negate();

            return ret;
        }
    }
}