using ActionStreetMap.Core.Tiling;
using ActionStreetMap.Core.Tiling.Models;
using ActionStreetMap.Infrastructure.Utilities;
using Node = ActionStreetMap.Osm.Entities.Node;

namespace ActionStreetMap.Osm.Visitors
{
    /// <summary> Node visitor. </summary>
    internal class NodeVisitor: ElementVisitor
    {
        /// <inheritdoc />
        public NodeVisitor(Tile tile, IModelLoader modelLoader, IObjectPool objectPool)
            : base(tile, modelLoader, objectPool)
        {
        }

        /// <inheritdoc />
        public override void VisitNode(Node node)
        {
            if (node.Tags != null)
            {
                ModelLoader.LoadNode(Tile, new Core.Tiling.Models.Node
                {
                    Id = node.Id,
                    Point = node.Coordinate,
                    Tags = node.Tags
                });
            }
        }
    }
}
