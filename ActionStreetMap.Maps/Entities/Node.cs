using ActionStreetMap.Core;
using ActionStreetMap.Maps.Visitors;

namespace ActionStreetMap.Maps.Entities
{
    /// <summary> Represents a simple node. </summary>
    public class Node : Element
    {
        /// <summary> Geocoordinate of the node. </summary>
        public GeoCoordinate Coordinate;

        /// <inheritdoc />
        public override void Accept(IElementVisitor elementVisitor)
        {
            elementVisitor.VisitNode(this);
        }

        /// <inheritdoc />
        public override bool IsInside(BoundingBox bbox)
        {
            return bbox.Contains(Coordinate);
        }
    }
}