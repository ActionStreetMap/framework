using System.Collections.Generic;
using ActionStreetMap.Core;
using ActionStreetMap.Maps.Visitors;

namespace ActionStreetMap.Maps.Entities
{
    /// <summary>
    ///     Represents a simple way.
    /// </summary>
    public class Way : Element
    {
        /// <summary>
        ///     Holds the list of nodes.
        /// </summary>
        public List<long> NodeIds { get; set; }

        /// <summary>
        ///     GeoCoordinates of way.
        /// </summary>
        public List<GeoCoordinate> Coordinates { get; set; }

        /// <inheritdoc />
        public override void Accept(IElementVisitor elementVisitor)
        {
            elementVisitor.VisitWay(this);
        }

        /// <summary>
        ///     Returns all the ponts in this way in the same order as the nodes.
        /// </summary>
        public void FillPoints(List<GeoCoordinate> coordinates)
        {
            for (int idx = 0; idx < Coordinates.Count; idx++)
            {
                if (idx > 0 && Coordinates[idx - 1] == Coordinates[idx])
                    continue;
                coordinates.Add(Coordinates[idx]);
            }
        }

        /// <summary>
        ///     True if way is polygon.
        /// </summary>
        public bool IsPolygon
        {
            get { return Coordinates.Count > 2; }
        }
    }
}