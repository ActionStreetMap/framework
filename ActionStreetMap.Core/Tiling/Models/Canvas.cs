using ActionStreetMap.Core.Scene.Details;
using ActionStreetMap.Core.Scene.Roads;
using ActionStreetMap.Infrastructure.Utilities;
using System;
using System.Collections.Generic;

namespace ActionStreetMap.Core.Tiling.Models
{
    /// <summary>
    ///     Represents canvas (terrain).
    /// </summary>
    public class Canvas : Model, IDisposable
    {
        private readonly IRoadGraphBuilder _roadGraphBuilder = new RoadGraphBuilder();
        private readonly IObjectPool _objectPool;

        public List<Surface> Areas { get; private set; }
        public List<Surface> Elevations { get; private set; }
        public List<Tree> Trees { get; private set; }

        /// <inheritdoc />
        public override bool IsClosed { get { return false; } }

        public Canvas(IObjectPool objectPool)
        {
            _objectPool = objectPool;
            Areas = new List<Surface>();
            Elevations = new List<Surface>();
            Trees = new List<Tree>();
        } 

        /// <inheritdoc />
        public override void Accept(IModelVisitor visitor)
        {
            visitor.VisitCanvas(this);
        }

        /// <summary>
        ///     Adds road element to terrain.
        /// </summary>
        /// <param name="roadElement">Road element</param>
        public void AddRoadElement(RoadElement roadElement)
        {
            lock (_roadGraphBuilder)
            {
                _roadGraphBuilder.Add(roadElement);
            }
        }

        /// <summary>
        ///     Adds area which should be drawn using different splat index.
        /// </summary>
        /// <param name="areaSettings">Area settings.</param>
        public void AddArea(Surface surface)
        {
            lock (Areas)
            {
                Areas.Add(surface);
            }
        }

        /// <summary>
        ///     Adds area which should be adjuested by height. Processed last.
        /// </summary>
        /// <param name="areaSettings">Area settings.</param>
        public void AddElevation(Surface surface)
        {
            lock (Elevations)
            {
                Elevations.Add(surface);
            }
        }

        /// <summary>
        ///     Adds tree.
        /// </summary>
        /// <param name="tree">Tree.</param>
        public void AddTree(Tree tree)
        {
            lock (Trees)
            {
                Trees.Add(tree);
            }
        }

        public RoadGraph BuildRoadGraph()
        {
            return _roadGraphBuilder.Build();
        }

        public void Dispose()
        {
            //Return lists to object pool
            foreach (var area in Areas)
                _objectPool.Store(area.Points);
            foreach (var elevation in Elevations)
                _objectPool.Store(elevation.Points);
        }
    }
}
