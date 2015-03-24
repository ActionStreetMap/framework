using System.Collections.Generic;

namespace ActionStreetMap.Core.Terrain
{
    internal class MeshCell
    {
        public MeshRegion Water;
        public MeshRegion CarRoads;
        public MeshRegion WalkRoads;
        public List<MeshRegion> Surfaces;
        public MeshRegion Background;
    }
}
