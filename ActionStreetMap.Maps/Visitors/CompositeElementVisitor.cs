using ActionStreetMap.Maps.Entities;

namespace ActionStreetMap.Maps.Visitors
{
    /// <summary> Delegates element processing to the corresponding visitor. </summary>
    internal class CompositeElementVisitor : IElementVisitor
    {
        private readonly IElementVisitor _nodeVisitor;
        private readonly IElementVisitor _wayVisitor;
        private readonly IElementVisitor _relationVisitor;

        public CompositeElementVisitor(IElementVisitor nodeVisitor, 
            IElementVisitor wayVisitor, IElementVisitor relationVisitor)
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
            _wayVisitor.VisitWay(way);
        }

        public void VisitRelation(Relation relation)
        {
            _relationVisitor.VisitRelation(relation);
        }

        #endregion
    }
}