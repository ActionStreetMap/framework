using System;

namespace ActionStreetMap.Core.Geometry.StraightSkeleton.Primitives
{
    public struct Vector2d
    {
        public static Vector2d Empty = new Vector2d(double.MinValue, double.MinValue);

        public double X;
        public double Y;

        public Vector2d(Vector2d var1)
        {
            X = var1.X;
            Y = var1.Y;
        }

        public Vector2d(double var1, double var3)
        {
            X = var1;
            Y = var3;
        }

        public void Add(Vector2d var1)
        {
            X += var1.X;
            Y += var1.Y;
        }

        public void Sub(Vector2d var1)
        {
            X -= var1.X;
            Y -= var1.Y;
        }

        public void Negate()
        {
            X = -X;
            Y = -Y;
        }

        public void Sub(Vector2d var1, Vector2d var2)
        {
            X = var1.X - var2.X;
            Y = var1.Y - var2.Y;
        }

        public void Scale(double var1)
        {
            X *= var1;
            Y *= var1;
        }

        public void Normalize()
        {
            var var1 = 1.0D/Math.Sqrt(X*X + Y*Y);
            X *= var1;
            Y *= var1;
        }

        public double Dot(Vector2d var1)
        {
            return X*var1.X + Y*var1.Y;
        }

        public double DistanceSquared(Vector2d var1)
        {
            var var2 = X - var1.X;
            var var4 = Y - var1.Y;
            return var2*var2 + var4*var4;
        }

        public static bool operator ==(Vector2d left, Vector2d right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Vector2d left, Vector2d right)
        {
            return !(left == right);
        }

        public bool Equals(Vector2d obj)
        {
            return X.Equals(obj.X) && Y.Equals(obj.Y);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is Vector2d && Equals((Vector2d)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (X.GetHashCode()*397) ^ Y.GetHashCode();
            }
        }

        public override string ToString()
        {
            return string.Format("({0}, {1})", X, Y);
        }
    }
}