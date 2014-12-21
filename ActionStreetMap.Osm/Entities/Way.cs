using System;
using System.Collections.Generic;
using ActionStreetMap.Core;
using ActionStreetMap.Osm.Visitors;

namespace ActionStreetMap.Osm.Entities
{
    /// <summary>
    ///     Represents a simple way.
    /// </summary>
    public class Way : Element
    {
        [Obsolete]
        /// <summary>
        ///     Holds the list of nodes.
        /// </summary>
        public List<long> NodeIds { get; set; }

        [Obsolete]
        /// <summary>
        ///     Nodes.
        /// </summary>
        public List<Node> Nodes { get; set; }


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
            for (int idx = 0; idx < Nodes.Count; idx++)
            {
                if (idx > 0 && Nodes[idx - 1].Coordinate == Nodes[idx].Coordinate)
                {
                    continue;
                }
                coordinates.Add(Nodes[idx].Coordinate);
            }
        }

        /// <summary>
        ///     True if way is polygon.
        /// </summary>
        public bool IsPolygon
        {
            get { return Nodes.Count > 2; }
        }

        /// <summary>
        ///     True if way is fully loaded.
        /// </summary>
        public bool IsComplete
        {
            get { return Nodes.Count == NodeIds.Count; }
        }

        /// <summary>
        ///     True if way is closed polygon.
        /// </summary>
        public bool IsClosed
        {
            get { return Nodes[0].Id == Nodes[Nodes.Count - 1].Id; }
        }
    }
}