using System.Collections.Generic;
using ActionStreetMap.Core.Polygons;
using ActionStreetMap.Core.Polygons.Geometry;

namespace ActionStreetMap.Core.Terrain
{
    internal class MeshGridCell
    {
        public Mesh Mesh;

        public List<MeshRegion> Water;
        public List<MeshRegion> Surfaces;
        public List<MeshRegion> Roads;
        public List<MeshRegion> Bridges;
    }
}
