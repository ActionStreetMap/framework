using ActionStreetMap.Core.Geometry.Triangle;
using VertexPaths = System.Collections.Generic.List<System.Collections.Generic.List<ActionStreetMap.Core.Geometry.Triangle.Geometry.Vertex>>;

namespace ActionStreetMap.Core.Terrain
{
    internal class MeshRegion
    {
        public string GradientKey;
        public Mesh Mesh;
        public VertexPaths Contours;
    }
}
