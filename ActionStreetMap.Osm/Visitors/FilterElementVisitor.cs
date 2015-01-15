using System;
using ActionStreetMap.Core;
using ActionStreetMap.Osm.Entities;

namespace ActionStreetMap.Osm.Visitors
{
    /// <summary>
    ///     Filters elements.
    /// </summary>
    internal class FilterElementVisitor : IElementVisitor
    {
        private readonly IElementVisitor _nodeVisitor;
        private readonly IElementVisitor _wayVisitor;
        private readonly IElementVisitor _relationVisitor;

        public BoundingBox BoundingBox { private get; set; }

        public FilterElementVisitor(IElementVisitor nodeVisitor, IElementVisitor wayVisitor, 
            IElementVisitor relationVisitor)
        {
            _nodeVisitor = nodeVisitor;
            _wayVisitor = wayVisitor;
            _relationVisitor = relationVisitor;
        }

        #region IElementVisitor implementation

        public void VisitNode(Node node)
        {
            _nodeVisitor.VisitNode(node);
        }

        public void VisitWay(Way way)
        {
            if (IsWayInside(way)) 
                _wayVisitor.VisitWay(way);
        }

        public void VisitRelation(Relation relation)
        {
            _relationVisitor.VisitRelation(relation);
        }

        #endregion

        private bool IsWayInside(Way way)
        {
            for (int i = 0; i < way.Coordinates.Count - 1; i++)
            {
                if (IsLineIntersectsBoundingBox(way.Coordinates[i], way.Coordinates[i + 1]))
                    return true;
            }

            return false;
        }

        private bool IsLineIntersectsBoundingBox(GeoCoordinate first, GeoCoordinate second)
        {
            var x1 = first.Longitude;
            var y1 = first.Latitude;
            var x2 = second.Longitude;
            var y2 = second.Latitude;

            var minX = BoundingBox.MinPoint.Longitude;
            var minY = BoundingBox.MinPoint.Latitude;
            var maxX = BoundingBox.MaxPoint.Longitude;
            var maxY = BoundingBox.MaxPoint.Latitude;

            // Completely outside.
            if ((x1 <= minX && x2 <= minX) || (y1 <= minY && y2 <= minY) || 
                (x1 >= maxX && x2 >= maxX) || (y1 >= maxY && y2 >= maxY))
                return false;

            var m = (y2 - y1) / (x2 - x1);

            var y = m * (minX - x1) + y1;
            if (y > minY && y < maxY) return true;

            y = m * (maxX - x1) + y1;
            if (y > minY && y < maxY) return true;

            var x = (minY - y1) / m + x1;
            if (x > minX && x < maxX) return true;

            x = (maxY - y1) / m + x1;
            if (x > minX && x < maxX) return true;

            return false;
        }
    }
}