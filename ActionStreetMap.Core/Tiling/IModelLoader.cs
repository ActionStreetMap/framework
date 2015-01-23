using ActionStreetMap.Core.Tiling.Models;

namespace ActionStreetMap.Core.Tiling
{
    /// <summary>
    ///     Defines behavior of class which should load given models into tile.
    /// </summary>
    public interface IModelLoader
    {
        /// <summary>
        ///     Visits tile. Called first.
        /// </summary>
        /// <param name="tile">Tile.</param>
        void PrepareTile(Tile tile);

        /// <summary>
        ///     Visits relation.
        /// </summary>
        /// <param name="tile">Tile.</param>
        /// <param name="relation">Relation.</param>
        void LoadRelation(Tile tile, Relation relation);

        /// <summary>
        ///     Visits area.
        /// </summary>
        /// <param name="tile">Tile.</param>
        /// <param name="area">Area.</param>
        void LoadArea(Tile tile, Area area);

        /// <summary>
        ///     Visits way.
        /// </summary>
        /// <param name="tile">Tile.</param>
        /// <param name="way">Way.</param>
        void LoadWay(Tile tile, Way way);

        /// <summary>
        ///     Visits node.
        /// </summary>
        /// <param name="tile">Tile.</param>
        /// <param name="node">Node.</param>
        void LoadNode(Tile tile, Node node);

        /// <summary>
        ///     Visits canvas. Called last.
        /// </summary>
        /// <param name="tile">Tile.</param>
        void CompleteTile(Tile tile);
    }
}
