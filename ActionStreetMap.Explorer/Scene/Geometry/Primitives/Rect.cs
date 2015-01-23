using UnityEngine;

namespace ActionStreetMap.Explorer.Scene.Geometry.Primitives
{
    /// <summary>
    ///     Represent rectangle.
    /// </summary>
    public class Rect
    {
        /// <summary>
        ///     Left bottom corner.
        /// </summary>
        public Vector2 LeftBottom;

        /// <summary>
        ///     Right upper corner.
        /// </summary>
        public Vector2 RightUpper;

        /// <summary>
        ///     Creates Rect.
        /// </summary>
        /// <param name="leftBottom">Left bottom corner.</param>
        /// <param name="rightUpper">Right upper corner.</param>
        public Rect(Vector2 leftBottom, Vector2 rightUpper)
        {
            LeftBottom = leftBottom;
            RightUpper = rightUpper;
        }
    }
}
