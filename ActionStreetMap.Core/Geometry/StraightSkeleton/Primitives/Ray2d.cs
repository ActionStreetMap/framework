using System;

namespace ActionStreetMap.Core.Geometry.StraightSkeleton.Primitives
{
    public class Ray2d : LineParametric2d
    {
        public Ray2d(Vector2d pA, Vector2d pU)
            : base(pA, pU)
        {
        }

        public static Vector2d Collide(Ray2d ray, LineLinear2d line, double epsilon)
        {
            var collide = LineLinear2d.Collide(ray.CreateLinearForm(), line);
            if (collide.Equals(Vector2d.Empty))
                return Vector2d.Empty;

            var collideVector = collide - ray.A;
            return ray.U.Dot(collideVector) < epsilon ? Vector2d.Empty : collide;
        }

        public override String ToString()
        {
            return "Ray2d [A=" + A + ", U=" + U + "]";
        }
    }
}