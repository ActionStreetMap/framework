using System;
using System.Collections.Generic;
using ActionStreetMap.Core.Polygons;

namespace ActionStreetMap.Explorer.Terrain
{
    internal class MeshGrid
    {
        public Cell[,] Cells;

        public class Cell
        {
            public Mesh Mesh;
            public List<MeshRegion> Regions;
        }
    }
}
