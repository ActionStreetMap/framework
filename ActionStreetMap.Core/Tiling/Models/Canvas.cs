using System;
using System.Collections.Generic;
using ActionStreetMap.Core.Geometry.Triangle;
using ActionStreetMap.Core.Scene;
using ActionStreetMap.Infrastructure.Reactive;
using ActionStreetMap.Infrastructure.Utilities;

namespace ActionStreetMap.Core.Tiling.Models
{
    /// <summary> Represents canvas (terrain). </summary>
    public class Canvas : Model, IDisposable
    {
        private readonly IObjectPool _objectPool;

        public List<RoadElement> Roads { get; private set; }
        public List<Tuple<Surface, Action<Mesh>>> Surfaces { get; private set; }
        public List<Surface> Water { get; private set; }

        /// <inheritdoc />
        public override bool IsClosed { get { return false; } }

        /// <summary> Creates instance of <see cref="Canvas"/>. </summary>
        /// <param name="objectPool">Object pool.</param>
        public Canvas(IObjectPool objectPool)
        {
            _objectPool = objectPool;
            Surfaces = objectPool.NewList<Tuple<Surface, Action<Mesh>>>(32);
            Roads = objectPool.NewList<RoadElement>(32);
            Water = objectPool.NewList<Surface>(32);
        }

        /// <inheritdoc />
        public override void Accept(Tile tile, IModelLoader loader)
        {
            loader.CompleteTile(tile);
        }

        /// <summary> Adds road element to terrain. </summary>
        /// <param name="roadElement">Road element</param>
        public void AddRoad(RoadElement roadElement)
        {
            lock (Roads)
            {
                Roads.Add(roadElement);
            }
        }

        /// <summary> Adds surface. </summary>
        public void AddSurface(Surface surface, Action<Mesh> modifyMeshAction = null)
        {
            lock (Surfaces)
            {
                Surfaces.Add(new Tuple<Surface, Action<Mesh>>(surface, modifyMeshAction));
            }
        }

        /// <summary> Adds water. </summary>
        /// <param name="surface">Water settings.</param>
        public void AddWater(Surface surface)
        {
            lock (Water)
            {
                Water.Add(surface);
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
        }

        /// <summary> Dispose pattern implementation. </summary>
        /// <param name="disposing">True if necessary to dispose managed resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // return lists to object pool
                foreach (var tuple in Surfaces)
                    _objectPool.StoreList(tuple.Item1.Points);
                foreach (var water in Water)
                    _objectPool.StoreList(water.Points);
                foreach (var road in Roads)
                    _objectPool.StoreList(road.Points);
            }
        }
    }
}