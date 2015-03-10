
using ActionStreetMap.Core.Polygons.Geometry;

namespace ActionStreetMap.Explorer.Terrain
{
    internal class MeshRegion
    {
        public IMeshRegionVisitor Visitor;
        public Vertex Anchor;
    }
}
