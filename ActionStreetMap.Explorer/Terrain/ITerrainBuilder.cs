using ActionStreetMap.Core.MapCss.Domain;
using ActionStreetMap.Core.Tiling.Models;
using ActionStreetMap.Core.Unity;

namespace ActionStreetMap.Explorer.Terrain
{
    public interface ITerrainBuilder
    {
        IGameObject Build(Tile tile, Rule rule);
    }
}
