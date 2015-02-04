using System;
using System.Collections.Generic;
using ActionStreetMap.Core.Scene.Details;
using ActionStreetMap.Core.Scene.Roads;
using ActionStreetMap.Infrastructure.Utilities;

namespace ActionStreetMap.Core.Tiling.Models
{
    /// <summary> Represents canvas (terrain). </summary>
    public class Canvas : Model, IDisposable
    {
        private readonly IRoadGraphBuilder _roadGraphBuilder = new RoadGraphBuilder();
        private readonly IObjectPool _objectPool;

        /// <summary> Flat areas which should be rendered with some texture. </summary>
        public List<Surface> Areas { get; private set; }

        /// <summary> Fixed value elevation surface </summary>
        public List<Surface> Elevations { get; private set; }

        /// <summary> Tree. </summary>
        public List<Tree> Trees { get; private set; }

        /// <summary> Splat array. </summary>
        public float[,,] SplatMap { get; set; }
        
        /// <summary> Terrain details list.  </summary>
        public List<int[,]> Details { get; set; }

        /// <inheritdoc />
        public override bool IsClosed { get { return false; } }

        /// <summary> Creates instance of <see cref="Canvas"/>. </summary>
        /// <param name="objectPool">Object pool.</param>
        public Canvas(IObjectPool objectPool)
        {
            _objectPool = objectPool;
            Areas = objectPool.NewList<Surface>(128);
            Elevations = objectPool.NewList<Surface>(8);
            Trees = objectPool.NewList<Tree>(64);
        }

        /// <inheritdoc />
        public override void Accept(Tile tile, IModelLoader loader)
        {
            loader.CompleteTile(tile);
        }

        /// <summary> Adds road element to terrain. </summary>
        /// <param name="roadElement">Road element</param>
        public void AddRoadElement(RoadElement roadElement)
        {
            lock (_roadGraphBuilder)
            {
                _roadGraphBuilder.Add(roadElement);
            }
        }

        /// <summary> Adds area which should be drawn using different splat index. </summary>
        /// <param name="surface">Area settings.</param>
        public void AddArea(Surface surface)
        {
            lock (Areas)
            {
                Areas.Add(surface);
            }
        }

        /// <summary> Adds area which should be adjuested by height. Processed last. </summary>
        /// <param name="surface">Area settings.</param>
        public void AddElevation(Surface surface)
        {
            lock (Elevations)
            {
                Elevations.Add(surface);
            }
        }

        /// <summary> Adds tree. </summary>
        /// <param name="tree">Tree.</param>
        public void AddTree(Tree tree)
        {
            lock (Trees)
            {
                Trees.Add(tree);
            }
        }

        /// <summary> Builds road graph. </summary>
        /// <returns>Road graph.</returns>
        public RoadGraph BuildRoadGraph()
        {
            return _roadGraphBuilder.Build();
        }

        /// <inheritdoc />
        public void Dispose()
        {
           Dispose(true);
        }

        /// <summary>  Dispose pattern implementation. </summary>
        /// <param name="disposing">True if necessary to dispose managed resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                //Return lists to object pool
                foreach (var area in Areas)
                    _objectPool.StoreList(area.Points);
                foreach (var elevation in Elevations)
                    _objectPool.StoreList(elevation.Points);

                Details.ForEach(array => Array.Clear(array, 0, array.Length));
                _objectPool.StoreList(Details, true);
                _objectPool.StoreArray(SplatMap);

                _objectPool.StoreList(Areas);
                _objectPool.StoreList(Elevations);
                _objectPool.StoreList(Trees);
            }
        }
    }
}