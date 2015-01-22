using ActionStreetMap.Core.MapCss.Domain;
using ActionStreetMap.Core.Tiling.Models;
using ActionStreetMap.Core.Unity;
using ActionStreetMap.Core.Utilities;
using ActionStreetMap.Infrastructure.Dependencies;
using ActionStreetMap.Models.Details;
using ActionStreetMap.Models.Terrain;

namespace ActionStreetMap.Explorer.Scene.Builders
{
    /// <summary>
    ///     Provides the way to process trees.
    /// </summary>
    public class TreeModelBuilder : ModelBuilder
    {
        private readonly ITerrainBuilder _terrainBuilder;

        /// <inheritdoc />
        public override string Name
        {
            get { return "tree"; }
        }

        /// <summary>
        ///     Creates TreeModelBuilder.
        /// </summary>
        [Dependency]
        public TreeModelBuilder(ITerrainBuilder terrainBuilder)
        {
            _terrainBuilder = terrainBuilder;
        }

        /// <inheritdoc />
        public override IGameObject BuildNode(Tile tile, Rule rule, Node node)
        {
            var mapPoint = GeoProjection.ToMapCoordinate(tile.RelativeNullPoint, node.Point);
            _terrainBuilder.AddTree(new TreeDetail
            {
                Id = node.Id,
                Point = mapPoint
            });

            return null;
        }
    }
}