using System;
using ActionStreetMap.Core.Geometry.Triangle;
using VertexPaths = System.Collections.Generic.List<System.Collections.Generic.List<ActionStreetMap.Core.Geometry.Triangle.Geometry.Vertex>>;

namespace ActionStreetMap.Core.Scene.Terrain
{
    internal class MeshRegion
    {
        public string GradientKey;
        public Action<Mesh> ModifyMeshAction;

        public Mesh Mesh;

        // TODO should be refactored: this looks like workaround
        public VertexPaths Contours;
    }
}
