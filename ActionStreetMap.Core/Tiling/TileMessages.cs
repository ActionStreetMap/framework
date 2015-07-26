
using ActionStreetMap.Core.Geometry;
using ActionStreetMap.Core.Tiling.Models;

namespace ActionStreetMap.Core.Tiling
{
    #region Loading
    /// <summary> Defines "Tile load start" message. </summary>
    public sealed class TileLoadStartMessage
    {
        /// <summary> Gets tile center. </summary>
        public Vector2d TileCenter { get; private set; }

        /// <summary> Creates message. </summary>
        /// <param name="tileCenter">center of tile.</param>
        public TileLoadStartMessage(Vector2d tileCenter) { TileCenter = tileCenter; }
    }
    /// <summary> Defines "Tile load finish" message. </summary>
    public sealed class TileLoadFinishMessage
    {
        /// <summary> Gets tile. </summary>
        public Tile Tile { get; private set; }

        /// <summary> Creates message. </summary>
        /// <param name="tile">Tile.</param>
        public TileLoadFinishMessage(Tile tile) { Tile = tile; }
    }

    /// <summary> Defines "Tile destroy" message. </summary>
    public sealed class TileDestroyMessage
    {
        /// <summary> Gets tile. </summary>
        public Tile Tile { get; private set; }

        /// <summary> Creates message. </summary>
        /// <param name="tile">Tile.</param>
        public TileDestroyMessage(Tile tile) { Tile = tile; }
    }
    #endregion
}
