
using ActionStreetMap.Core.Polygons.Geometry;

namespace ActionStreetMap.Core.Terrain
{
    internal class MeshRegion
    {
        public IMeshRegionVisitor Visitor;
        public Vertex Anchor;
    }
}
