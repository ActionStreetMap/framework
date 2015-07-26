using System;

namespace ActionStreetMap.Core.Geometry
{
    /// <summary> Represents line segment in 2D space defined by start and end point. </summary>
    public struct LineSegment2d
    {
        /// <summary> Start point. </summary>
        public Vector2d Start;

        /// <summary> End point </summary>
        public Vector2d End;

        /// <summary> Creates instance of <see cref="LineSegment2d"/>. </summary>
        public LineSegment2d(Vector2d start, Vector2d end)
        {
            Start = start;
            End = end;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return String.Format("[({0},{1}) ({2},{3})]", Start.X, Start.Y, End.X, End.Y);
        }
    }
}
