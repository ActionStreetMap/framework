using System;
using ActionStreetMap.Core;
using ActionStreetMap.Maps.Entities;

namespace ActionStreetMap.Maps.Visitors
{
    /// <summary> Filters elements. </summary>
    internal class FilterElementVisitor : IElementVisitor
    {
        private readonly BoundingBox _boundingBox;
        private readonly IElementVisitor _nodeVisitor;
        private readonly IElementVisitor _wayVisitor;
        private readonly IElementVisitor _relationVisitor;

        public FilterElementVisitor(BoundingBox boundingBox, IElementVisitor nodeVisitor, 
            IElementVisitor wayVisitor, IElementVisitor relationVisitor)
        {
            _boundingBox = boundingBox;
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
            _wayVisitor.VisitWay(way);
        }

        public void VisitRelation(Relation relation)
        {
            _relationVisitor.VisitRelation(relation);
        }

        #endregion
    }
}