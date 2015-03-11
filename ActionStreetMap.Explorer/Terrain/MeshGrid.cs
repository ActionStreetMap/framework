using System;
using System.Collections.Generic;
using ActionStreetMap.Core.Polygons;
using ActionStreetMap.Core;

namespace ActionStreetMap.Explorer.Terrain
{
    internal class MeshGrid
    {
        public GeoCoordinate RelativeNullPoint;
        public Cell[,] Cells;

        public class Cell
        {
            public Data Water;
            public Data Surfaces;
            public Data Roads;
            public Data Bridges;
        }

        public class Data
        {
            public Mesh Mesh;
            public List<MeshRegion> Regions;
        }
    }
}
