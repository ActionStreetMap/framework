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
        private readonly IObjectPool _objectPool;

        public List<RoadElement> Roads { get; private set; }
        public List<Surface> Areas { get; private set; }
        public List<Surface> Water { get; private set; }

        /// <inheritdoc />
        public override bool IsClosed { get { return false; } }

        /// <summary> Creates instance of <see cref="Canvas"/>. </summary>
        /// <param name="objectPool">Object pool.</param>
        public Canvas(IObjectPool objectPool)
        {
            _objectPool = objectPool;
            Areas = objectPool.NewList<Surface>(128);
            Roads = objectPool.NewList<RoadElement>(64);
            Water = objectPool.NewList<Surface>(64);
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

        /// <summary> Adds area which should be drawn using different splat index. </summary>
        /// <param name="surface">Area settings.</param>
        public void AddArea(Surface surface)
        {
            lock (Areas)
            {
                Areas.Add(surface);
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
                foreach (var area in Areas)
                    _objectPool.StoreList(area.Points);
                foreach (var water in Water)
                    _objectPool.StoreList(water.Points);
                foreach (var road in Roads)
                    _objectPool.StoreList(road.Points);
            }
        }
    }
}