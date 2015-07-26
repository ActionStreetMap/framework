using System;
using ActionStreetMap.Core.Geometry.Triangle;
using ActionStreetMap.Core.Geometry.Triangle.Meshing;
using VertexPaths = System.Collections.Generic.List<System.Collections.Generic.List<ActionStreetMap.Core.Geometry.Triangle.Geometry.Vertex>>;

namespace ActionStreetMap.Core.Scene.Terrain
{
    internal sealed class MeshRegion: IDisposable
    {
        public string GradientKey;
        public float ElevationNoiseFreq;
        public float ColorNoiseFreq;
        public Action<Mesh> ModifyMeshAction;

        public Mesh Mesh;

        // TODO should be refactored: this looks like workaround
        public VertexPaths Contours;

        /// <inheritdoc />
        public void Dispose()
        {
            TrianglePool.FreeMesh(Mesh);
        }
    }
}
