using ActionStreetMap.Core.Scene.World.Roads;
using Color32 = ActionStreetMap.Core.Unity.Color32;
using Rect = ActionStreetMap.Models.Geometry.Primitives.Rect;

namespace ActionStreetMap.Models.Roads
{
    /// <summary>
    ///     Defines road style.
    /// </summary>
    public class RoadStyle
    {
        /// <summary>
        ///     Path to Resource material
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        ///     Gets or sets road width.
        /// </summary>
        public int Width { get; set; }

        /// <summary>
        ///     Gets or sets road type.
        /// </summary>
        public RoadType Type { get; set; }

        /// <summary>
        ///    Gets or sets material of road.
        /// </summary>
        public string Material { get; set; }

        /// <summary>
        ///     Gets or sets color.
        /// </summary>
        public Core.Unity.Color32 Color { get; set; }

        /// <summary>
        ///     Gets or sets main uv map.
        /// </summary>
        public Geometry.Primitives.Rect MainUvMap { get; set; }

        /// <summary>
        ///     Gets or sets  turn uv map.
        /// </summary>
        public Geometry.Primitives.Rect TurnUvMap { get; set; }
    }
}
