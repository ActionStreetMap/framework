
namespace ActionStreetMap.Core.Tiling.Models
{
    /// <summary>
    ///     Represents Node - point on map with associated tags.
    /// </summary>
    public class Node: Model
    {
        /// <summary>
        ///     Gets or sets geo coordinate.
        /// </summary>
        public GeoCoordinate Point { get; set; }

        /// <inheritdoc />
        public override bool IsClosed
        {
            get { return false; }
        }

        /// <inheritdoc />
        public override void Accept(IModelVisitor visitor)
        {
            visitor.VisitNode(this);
        }
    }
}
