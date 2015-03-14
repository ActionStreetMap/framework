using System.Collections.Generic;
using ActionStreetMap.Core.Polygons;

namespace ActionStreetMap.Core.Terrain
{
    internal class MeshGridCell
    {
        public Mesh Mesh;

        public MeshRegion Water;
        public MeshRegion Roads;

        public List<MeshRegion> Surfaces;
        public List<MeshRegion> Bridges;
    }
}
