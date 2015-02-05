using System.Collections.Generic;
using ActionStreetMap.Core;
using ActionStreetMap.Maps.Visitors;

namespace ActionStreetMap.Maps.Entities
{
    /// <summary> Represents a simple way. </summary>
    public class Way : Element
    {
        /// <summary> Holds the list of nodes. </summary>
        internal List<long> NodeIds { get; set; }

        /// <summary> Gets or sets geo coordinates of way. </summary>
        public List<GeoCoordinate> Coordinates { get; set; }

        /// <inheritdoc />
        public override void Accept(IElementVisitor elementVisitor)
        {
            elementVisitor.VisitWay(this);
        }

        /// <summary> True if way is polygon. </summary>
        public bool IsPolygon
        {
            get { return Coordinates.Count > 2; }
        }
    }
}