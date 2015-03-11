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
            public Content Water;
            public Content Surfaces;
            public Content Roads;
            public Content Bridges;
        }

        public class Content
        {
            public Mesh Mesh;
            public List<MeshRegion> Regions;
        }
    }
}
