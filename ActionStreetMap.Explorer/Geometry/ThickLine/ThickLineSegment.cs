using ActionStreetMap.Core.Geometry;

namespace ActionStreetMap.Explorer.Geometry.ThickLine
{
    /// <summary> Represents thick line segment. </summary>
    public struct ThickLineSegment
    {
        /// <summary> Left parallel segment. </summary>
        public LineSegment2d Left;

        /// <summary> Right parallel segment. </summary>
        public LineSegment2d Right;

        /// <summary> Creates ThickLineSegment. </summary>
        /// <param name="left">Left segment.</param>
        /// <param name="right">Right segment.</param>
        public ThickLineSegment(LineSegment2d left, LineSegment2d right)
        {
            Left = left;
            Right = right;
        }
    }
}
