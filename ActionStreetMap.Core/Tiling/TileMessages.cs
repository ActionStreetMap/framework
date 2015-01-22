
using ActionStreetMap.Core.Tiling.Models;

namespace ActionStreetMap.Core.Tiling
{
    #region Lookup
    /// <summary>
    ///     Defines "Tile found" message
    /// </summary>
    public sealed class TileFoundMessage
    {
        /// <summary>
        ///     Gets tile.
        /// </summary>
        public Tile Tile { get; private set; }

        /// <summary>
        ///     Gets position
        /// </summary>
        public MapPoint Position { get; private set; }

        /// <summary>
        ///     Creates message.
        /// </summary>
        /// <param name="tile">Tile.</param>
        /// <param name="position">Position.</param>
        public TileFoundMessage(Tile tile, MapPoint position)
        {
            Tile = tile;
            Position = position;
        }
    }
    #endregion

    #region Loading
    /// <summary>
    ///     Defines "Tile load start" message
    /// </summary>
    public sealed class TileLoadStartMessage
    {
        /// <summary>
        ///     Gets tile center.
        /// </summary>
        public MapPoint TileCenter { get; private set; }

        /// <summary>
        ///     Creates message.
        /// </summary>
        /// <param name="tileCenter">center of tile.</param>
        public TileLoadStartMessage(MapPoint tileCenter)
        {
            TileCenter = tileCenter;
        }
    }
    /// <summary>
    ///     Defines "Tile load finish" message
    /// </summary>
    public sealed class TileLoadFinishMessage
    {
        /// <summary>
        ///     Gets tile.
        /// </summary>
        public Tile Tile { get; private set; }

        /// <summary>
        ///  Creates message.
        /// </summary>
        /// <param name="tile">Tile.</param>
        public TileLoadFinishMessage(Tile tile)
        {
            Tile = tile;
        }
    }
    #endregion

    #region Activation
    /// <summary>
    ///     Defines "Tile activate" message
    /// </summary>
    public sealed class TileActivateMessage
    {
        /// <summary>
        ///     Gets tile.
        /// </summary>
        public Tile Tile { get; private set; }

        /// <summary>
        ///  Creates message.
        /// </summary>
        /// <param name="tile">Tile.</param>
        public TileActivateMessage(Tile tile)
        {
            Tile = tile;
        }
    }
    /// <summary>
    ///     Defines "Tile deactivate" message
    /// </summary>
    public sealed class TileDeactivateMessage
    {
        /// <summary>
        ///     Gets tile.
        /// </summary>
        public Tile Tile { get; private set; }

        /// <summary>
        ///  Creates message.
        /// </summary>
        /// <param name="tile">Tile.</param>
        public TileDeactivateMessage(Tile tile)
        {
            Tile = tile;
        }
    }
    /// <summary>
    ///     Defines "Tile destroy" message
    /// </summary>
    public sealed class TileDestroyMessage
    {
        /// <summary>
        ///     Gets tile.
        /// </summary>
        public Tile Tile { get; private set; }

        /// <summary>
        ///  Creates message.
        /// </summary>
        /// <param name="tile">Tile.</param>
        public TileDestroyMessage(Tile tile)
        {
            Tile = tile;
        }
    }

    #endregion
}
