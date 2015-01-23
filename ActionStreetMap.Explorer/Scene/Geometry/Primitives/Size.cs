namespace ActionStreetMap.Explorer.Scene.Geometry.Primitives
{
    /// <summary>
    ///     Represents size type.
    /// </summary>
    public class Size
    {
        /// <summary>
        ///     Width.
        /// </summary>
        public int Width;

        /// <summary>
        ///     Height.
        /// </summary>
        public int Height;

        /// <summary>
        ///     Creates Size.
        /// </summary>
        /// <param name="width">Width.</param>
        /// <param name="height">Height.</param>
        public Size(int width, int height)
        {
            Width = width;
            Height = height;
        }
    }
}
