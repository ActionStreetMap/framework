using System.Collections.Generic;
using ActionStreetMap.Core.Polygons;

namespace ActionStreetMap.Core.Terrain
{
    internal class MeshGrid
    {
        public GeoCoordinate RelativeNullPoint;
        public MeshCell[,] Cells;
    }

    internal class MeshData
    {
        public Mesh Mesh;
        public List<MeshRegion> Regions;
    }

    internal class MeshCell
    {
        public MeshData Water;
        public MeshData Surfaces;
        public MeshData Roads;
        public MeshData Bridges;
    }
}
