using ActionStreetMap.Core.Geometry.StraightSkeleton.Utils;

namespace ActionStreetMap.Core.Geometry.StraightSkeleton.Primitives
{
    public class LineParametric2d
    {
        public Vector2d A;
        public Vector2d U;

        public LineParametric2d(Vector2d pA, Vector2d pU)
        {
            A = pA;
            U = pU;
        }

        public LineLinear2d CreateLinearForm()
        {
            var x = this.A.X;
            var y = this.A.Y;

            var B = -U.X;
            var A = U.Y;

            var C = -(A*x + B*y);
            return new LineLinear2d(A, B, C);
        }

        public bool IsOnLeftSite(Vector2d point, double epsilon)
        {
            var direction = point - A;
            return Vector2dUtil.OrthogonalRight(U).Dot(direction) < epsilon;
        }

        public bool IsOnRightSite(Vector2d point, double epsilon)
        {
            var direction = point - A;
            return Vector2dUtil.OrthogonalRight(U).Dot(direction) > -epsilon;
        }
    }
}