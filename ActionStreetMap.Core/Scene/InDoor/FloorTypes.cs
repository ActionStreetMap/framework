using System;
using System.Collections.Generic;
using ActionStreetMap.Core.Geometry;
using ActionStreetMap.Infrastructure.Utilities;

namespace ActionStreetMap.Core.Scene.InDoor
{
    public class Floor
    {
        /// <summary> Floor entrances. </summary>
        public List<MapLine> Entrances;

        /// <summary> List of apartments </summary>
        public List<Apartment> Apartments;

        /// <summary> Outer walls. </summary>
        public List<MapLine> OuterWalls;

        /// <summary> Walls which separate apartments. </summary>
        public List<MapLine> PartitionWalls;

        /// <summary> Transit area walls. </summary>
        public List<MapLine> TransitWalls;

        /// <summary> Stairway or elevator areas. </summary>
        public List<Vector2d> Stairs;
    }

    public struct Apartment
    {
        /// <summary> Outer wall indices. </summary>
        public readonly List<int> OuterWalls;

        /// <summary> First transit wall index. </summary>
        public readonly List<int> TransitWalls;
        
        /// <summary> Partition wall index. </summary>
        public readonly List<int> PartitionWalls;

        /// <summary> Creates instance of <see cref="Apartment"/>. </summary>
        public Apartment(IObjectPool objectPool)
        {
            // TODO allocate lists from object pool
            OuterWalls = new List<int>();
            TransitWalls = new List<int>();
            PartitionWalls = new List<int>();
        }
    }

    public struct MapLine
    {
        public Vector2d Start;
        public Vector2d End;

        public MapLine(Vector2d start, Vector2d end)
        {
            Start = start;
            End = end;
        }

        public override string ToString()
        {
            return String.Format("[({0},{1}) ({2},{3})]", 
                Start.X, Start.Y, End.X, End.Y);
        }
    }
}
