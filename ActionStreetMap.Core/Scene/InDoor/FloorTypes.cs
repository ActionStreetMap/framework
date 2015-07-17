using System;
using System.Collections.Generic;
using ActionStreetMap.Core.Geometry;
using ActionStreetMap.Infrastructure.Utilities;

namespace ActionStreetMap.Core.Scene.InDoor
{
    /// <summary> Represents building's floor. </summary>
    public sealed class Floor: IDisposable
    {
        private readonly IObjectPool _objectPool;

        /// <summary> Floor entrances. </summary>
        public readonly List<MapLine> Entrances;

        /// <summary> List of apartments </summary>
        public readonly List<Apartment> Apartments;

        /// <summary> Outer walls. </summary>
        public readonly List<MapLine> OuterWalls;

        /// <summary> Walls which separate apartments. </summary>
        public readonly List<MapLine> PartitionWalls;

        /// <summary> Transit area walls. </summary>
        public readonly List<MapLine> TransitWalls;

        /// <summary> Stairway or elevator areas. </summary>
        public readonly List<Vector2d> Stairs;

        public Floor(IObjectPool objectPool)
        {
            _objectPool = objectPool;

            Entrances = objectPool.NewList<MapLine>(1);
            Apartments = objectPool.NewList<Apartment>(16);
            Stairs = objectPool.NewList<Vector2d>(32);
            OuterWalls = objectPool.NewList<MapLine>(32);
            PartitionWalls = objectPool.NewList<MapLine>(32);
            TransitWalls = objectPool.NewList<MapLine>(32);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _objectPool.StoreList(Entrances);
            _objectPool.StoreList(Apartments);
            _objectPool.StoreList(Stairs);
            _objectPool.StoreList(OuterWalls);
            _objectPool.StoreList(PartitionWalls);
            _objectPool.StoreList(TransitWalls);
        }
    }

    /// <summary> Represents building's apartment. </summary>
    public struct Apartment: IDisposable
    {
        private readonly IObjectPool _objectPool;

        /// <summary> Outer wall indices. </summary>
        public readonly List<int> OuterWalls;

        /// <summary> First transit wall index. </summary>
        public readonly List<int> TransitWalls;
        
        /// <summary> Partition wall index. </summary>
        public readonly List<int> PartitionWalls;

        /// <summary> Creates instance of <see cref="Apartment"/>. </summary>
        public Apartment(IObjectPool objectPool)
        {
            _objectPool = objectPool;

            OuterWalls = objectPool.NewList<int>(16);
            TransitWalls = objectPool.NewList<int>(16);
            PartitionWalls = objectPool.NewList<int>(16);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _objectPool.StoreList(OuterWalls);
            _objectPool.StoreList(TransitWalls);
            _objectPool.StoreList(PartitionWalls);
        }
    }
}
