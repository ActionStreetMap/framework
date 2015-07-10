using ActionStreetMap.Core.Geometry.StraightSkeleton.Primitives;

namespace ActionStreetMap.Core.Geometry.StraightSkeleton.Utils
{
    internal class Vector2dUtil
    {
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
        ///     <see cref="http://en.wikipedia.org/wiki/Vector_projection" />
        /// </summary>
        public static Vector2d orthogonalProjection(Vector2d unitVector, Vector2d vectorToProject)
        {
            var n = new Vector2d(unitVector);
            n.Normalize();

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
            {
                e1v.Add(e2v);
                return e1v;
            }

            // 0 - 180
            var ret = new Vector2d(norm1);
            ret.Negate();
            ret.Add(norm2);

            if (e1v.Dot(norm2) < 0)
                // 270 - 360
                ret.Negate();
            return ret;
        }
    }
}