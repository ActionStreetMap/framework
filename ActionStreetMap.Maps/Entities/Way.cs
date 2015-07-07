using System.Collections.Generic;
using ActionStreetMap.Core;
using ActionStreetMap.Maps.Visitors;

namespace ActionStreetMap.Maps.Entities
{
    /// <summary> Represents a simple way. </summary>
    public class Way : Element
    {
        /// <summary> Holds the list of nodes. </summary>
        internal List<long> NodeIds;

        /// <summary> List of geocoordinates of way. </summary>
        public List<GeoCoordinate> Coordinates;

        /// <inheritdoc />
        public override void Accept(IElementVisitor elementVisitor)
        {
            elementVisitor.VisitWay(this);
        }

        /// <inheritdoc />
        public override bool IsInside(BoundingBox bbox)
        {
            var wayBbox = BoundingBox.Empty();
            foreach (var coordinate in Coordinates)
                wayBbox += coordinate;

            return wayBbox.Intersects(bbox);
        }
    }
}