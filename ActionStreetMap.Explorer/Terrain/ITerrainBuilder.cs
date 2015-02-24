using ActionStreetMap.Core.MapCss.Domain;
using ActionStreetMap.Core.Tiling.Models;
using ActionStreetMap.Core.Unity;

namespace ActionStreetMap.Explorer.Terrain
{
    /// <summary> Defines terrain builder. </summary>
    public interface ITerrainBuilder
    {
        /// <summary> Builds terrain. </summary>
        /// <param name="tile">Tile.</param>
        /// <param name="rule">Terrain style rule.</param>
        /// <returns>Terrain game object.</returns>
        IGameObject Build(Tile tile, Rule rule);
    }
}
