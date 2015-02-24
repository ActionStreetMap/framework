using System;
using ActionStreetMap.Core.MapCss.Domain;
using ActionStreetMap.Core.Tiling.Models;
using ActionStreetMap.Core.Unity;

namespace ActionStreetMap.Explorer.Terrain.FlatShade
{
    /// <summary> Provides way to build terrain in flat shading style. </summary>
    internal class FlatShadeTerrainBuilder: ITerrainBuilder
    {
        /// <inheritdoc />
        public IGameObject Build(Tile tile, Rule rule)
        {
            throw new NotImplementedException();
        }
    }
}
