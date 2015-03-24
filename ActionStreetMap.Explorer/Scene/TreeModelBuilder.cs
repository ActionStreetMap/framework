using ActionStreetMap.Core.MapCss.Domain;
using ActionStreetMap.Core.Tiling.Models;
using ActionStreetMap.Core.Unity;
using ActionStreetMap.Core.Utils;

namespace ActionStreetMap.Explorer.Scene
{
    /// <summary> Provides the way to process trees. </summary>
    public class TreeModelBuilder : ModelBuilder
    {
        /// <inheritdoc />
        public override string Name { get { return "tree"; } }

        /// <inheritdoc />
        public override IGameObject BuildNode(Tile tile, Rule rule, Node node)
        {
            var mapPoint = GeoProjection.ToMapCoordinate(tile.RelativeNullPoint, node.Point);
            // TODO add tree at given point
            /*tile.Canvas.AddTree(new Tree()
            {
                Id = node.Id,
                Point = mapPoint
            });*/

            return null;
        }
    }
}