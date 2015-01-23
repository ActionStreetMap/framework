
using ActionStreetMap.Explorer.Scene.Geometry.Primitives;

namespace ActionStreetMap.Explorer.Scene.Geometry.ThickLine
{
    /// <summary>
    ///     Represents thick line segment.
    /// </summary>
    public class ThickLineSegment
    {
        /// <summary>
        ///     Left parallel segment.
        /// </summary>
        public Segment Left;

        /// <summary>
        ///     Right parallel segment.
        /// </summary>
        public Segment Right;

        /// <summary>
        ///     Creates ThickLineSegment.
        /// </summary>
        /// <param name="left">Left segment.</param>
        /// <param name="right">Right segment.</param>
        public ThickLineSegment(Segment left, Segment right)
        {
            Left = left;
            Right = right;
        }
    }
}
