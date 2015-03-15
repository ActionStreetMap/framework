using System.Collections.Generic;
using ActionStreetMap.Core.Polygons;

namespace ActionStreetMap.Core.Terrain
{
    internal class MeshCell
    {
        public Mesh Mesh;

        public MeshRegion Water;

        public MeshRegion CarRoads;
        public MeshRegion WalkRoads;

        public List<MeshRegion> Surfaces;
        public List<MeshRegion> Bridges;
    }
}
