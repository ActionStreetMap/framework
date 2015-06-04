using System;
using ActionStreetMap.Core.Geometry.Triangle.Meshing;
using VertexPaths = System.Collections.Generic.List<System.Collections.Generic.List<ActionStreetMap.Core.Geometry.Triangle.Geometry.Vertex>>;

namespace ActionStreetMap.Core.Scene.Terrain
{
    internal class MeshRegion
    {
        public string GradientKey;
        public float ElevationNoiseFreq;
        public float ColorNoiseFreq;
        public Action<IMesh> ModifyMeshAction;

        public IMesh Mesh;

        // TODO should be refactored: this looks like workaround
        public VertexPaths Contours;
    }
}
