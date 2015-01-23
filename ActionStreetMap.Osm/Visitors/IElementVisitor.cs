using System;
using ActionStreetMap.Core.Tiling;
using ActionStreetMap.Core.Tiling.Models;
using ActionStreetMap.Infrastructure.Utilities;
using Node = ActionStreetMap.Osm.Entities.Node;
using Relation = ActionStreetMap.Osm.Entities.Relation;
using Way = ActionStreetMap.Osm.Entities.Way;

namespace ActionStreetMap.Osm.Visitors
{
    /// <summary>
    /// Visitor for OSM elements. </summary>
    public interface IElementVisitor
    {
        /// <summary> Visits node. </summary>
        /// <param name="node">OSM node element.</param>
        void VisitNode(Node node);

        /// <summary> Visits way. </summary>
        /// <param name="way">OSM way element.</param>
        void VisitWay(Way way);

        /// <summary> Visits relation. </summary>
        /// <param name="relation">OSM relation element.</param>
        void VisitRelation(Relation relation);
    }

    internal class ElementVisitor : IElementVisitor
    {
        protected readonly Tile Tile;
        protected readonly IModelLoader ModelLoader;
        protected readonly IObjectPool ObjectPool;

        public ElementVisitor(Tile tile, IModelLoader modelLoader, IObjectPool objectPool)
        {
            Tile = tile;
            ModelLoader = modelLoader;
            ObjectPool = objectPool;
        }

        /// <inheritdoc />
        public virtual void VisitNode(Node node) { }
        /// <inheritdoc />
        public virtual void VisitWay(Way way) { }
        /// <inheritdoc />
        public virtual void VisitRelation(Relation relation) { }
    }
}