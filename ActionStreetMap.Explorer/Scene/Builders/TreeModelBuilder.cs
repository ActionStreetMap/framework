using ActionStreetMap.Core.MapCss.Domain;
using ActionStreetMap.Core.Scene.Details;
using ActionStreetMap.Core.Tiling.Models;
using ActionStreetMap.Core.Unity;
using ActionStreetMap.Core.Utilities;
using ActionStreetMap.Infrastructure.Dependencies;
using ActionStreetMap.Explorer.Scene.Terrain;

namespace ActionStreetMap.Explorer.Scene.Builders
{
    /// <summary>
    ///     Provides the way to process trees.
    /// </summary>
    public class TreeModelBuilder : ModelBuilder
    {
        /// <inheritdoc />
        public override string Name { get { return "tree"; } }

        /// <inheritdoc />
        public override IGameObject BuildNode(Tile tile, Rule rule, Node node)
        {
            var mapPoint = GeoProjection.ToMapCoordinate(tile.RelativeNullPoint, node.Point);
            tile.Canvas.AddTree(new Tree()
            {
                Id = node.Id,
                Point = mapPoint
            });

            return null;
        }
    }
}