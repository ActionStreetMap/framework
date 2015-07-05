
namespace ActionStreetMap.Core.Geometry.Triangle.Geometry
{
    using System;

    /// <summary> Represents a 2D point. </summary>
    public class Point : IComparable<Point>, IEquatable<Point>
    {
        internal ushort Id;
        internal double X;
        internal double Y;
        internal ushort Mark;

        public Point() : this(0, 0, 0)
        {
        }

        public Point(double x, double y) : this(x, y, 0)
        {
        }

        public Point(double x, double y, ushort mark)
        {
            X = x;
            Y = y;
            Mark = mark;
        }

        #region Operator overloading / overriding Equals

        // Compare "Guidelines for Overriding Equals() and Operator =="
        // http://msdn.microsoft.com/en-us/library/ms173147.aspx

        public static bool operator ==(Point a, Point b)
        {
            // If both are null, or both are same instance, return true.
            if (Object.ReferenceEquals(a, b))
            {
                return true;
            }

            // If one is null, but not both, return false.
            if (((object) a == null) || ((object) b == null))
            {
                return false;
            }

            return a.Equals(b);
        }

        public static bool operator !=(Point a, Point b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            // If parameter is null return false.
            if (obj == null)
            {
                return false;
            }

            Point p = obj as Point;

            if ((object) p == null)
            {
                return false;
            }

            return (X == p.X) && (Y == p.Y);
        }

        public bool Equals(Point p)
        {
            // If vertex is null return false.
            if ((object) p == null)
            {
                return false;
            }

            // Return true if the fields match:
            return (X == p.X) && (Y == p.Y);
        }

        #endregion

        public int CompareTo(Point other)
        {
            if (X == other.X && Y == other.Y)
            {
                return 0;
            }

            return (X < other.X || (X == other.X && Y < other.Y)) ? -1 : 1;
        }

        public override int GetHashCode()
        {
            return Id;
        }

        public override string ToString()
        {
            return String.Format("[{0},{1}]", X, Y);
        }
    }
}
