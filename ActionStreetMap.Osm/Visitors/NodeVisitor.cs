using ActionStreetMap.Core.Tiling;
using ActionStreetMap.Infrastructure.Utilities;
using ActionStreetMap.Osm.Entities;

namespace ActionStreetMap.Osm.Visitors
{
    /// <summary>
    ///     Node visitor.
    /// </summary>
    public class NodeVisitor: ElementVisitor
    {
        /// <inheritdoc />
        public NodeVisitor(IModelVisitor modelVisitor, IObjectPool objectPool)
            : base(modelVisitor, objectPool)
        {
        }

        /// <inheritdoc />
        public override void VisitNode(Node node)
        {
            if (node.Tags != null)
            {
                ModelVisitor.VisitNode(new Core.Tiling.Models.Node
                {
                    Id = node.Id,
                    Point = node.Coordinate,
                    Tags = node.Tags
                });
            }
        }
    }
}
