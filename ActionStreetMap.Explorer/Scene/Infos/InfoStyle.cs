using ActionStreetMap.Explorer.Geometry.Primitives;

namespace ActionStreetMap.Explorer.Scene.Infos
{
    /// <summary>
    ///     Defines style for information node.
    /// </summary>
    public class InfoStyle
    {
        /// <summary>
        ///     Gets or sets path to material
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        ///     Gets or sets Uv map
        /// </summary>
        public Rect UvMap { get; set; }
    }
}
