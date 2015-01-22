using System;
using ActionStreetMap.Core.Tiling;
using ActionStreetMap.Infrastructure.Dependencies;
using ActionStreetMap.Infrastructure.Utilities;
using ActionStreetMap.Osm.Entities;

namespace ActionStreetMap.Osm.Visitors
{
    /// <summary>
    ///     Visitor for OSM elements.
    /// </summary>
    public interface IElementVisitor
    {
        /// <summary>
        ///     Visits node.
        /// </summary>
        /// <param name="node">OSM node element.</param>
        void VisitNode(Node node);

        /// <summary>
        ///     Visits way.
        /// </summary>
        /// <param name="way">OSM way element.</param>
        void VisitWay(Way way);

        /// <summary>
        ///     Visits relation.
        /// </summary>
        /// <param name="relation">OSM relation element.</param>
        void VisitRelation(Relation relation);
    }

    /// <summary>
    ///     Helper class which is used for implementing separate element visitors
    /// </summary>
    public class ElementVisitor : IElementVisitor
    {
        /// <summary>
        ///     Current ModelVisitor.
        /// </summary>
        protected readonly IModelVisitor ModelVisitor;

        /// <summary>
        ///     Current object pool.
        /// </summary>
        protected readonly IObjectPool ObjectPool;

        /// <summary>
        ///     Creates ElementVisitor.
        /// </summary>
        /// <param name="modelVisitor">Model visitor.</param>
        /// <param name="objectPool">Object pool.</param>
        [Dependency]
        public ElementVisitor(IModelVisitor modelVisitor, IObjectPool objectPool)
        {
            ModelVisitor = modelVisitor;
            ObjectPool = objectPool;
        }

        /// <inheritdoc />
        public virtual void VisitNode(Node node)
        {
        }

        /// <inheritdoc />
        public virtual void VisitWay(Way way)
        {
        }

        /// <inheritdoc />
        public virtual void VisitRelation(Relation relation)
        {
        }
    }

    /// <summary>
    ///     Helper class which provides the way to use actions instead of subclassing
    /// </summary>
    internal class ActionElementVisitor : IElementVisitor
    {
        private readonly Action<Node> _visitNode;
        private readonly Action<Relation> _visitRelation;
        private readonly Action<Way> _visitWay;

        public ActionElementVisitor(Action<Node> visitNode, Action<Way> visitWay, Action<Relation> visitRelation)
        {
            _visitNode = visitNode;
            _visitWay = visitWay;
            _visitRelation = visitRelation;
        }

        public void VisitNode(Node node)
        {
            _visitNode(node);
        }

        public void VisitWay(Way way)
        {
            _visitWay(way);
        }

        public void VisitRelation(Relation relation)
        {
            _visitRelation(relation);
        }
    }
}