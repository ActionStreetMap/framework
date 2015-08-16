using System;
using System.Collections.Generic;
using ActionStreetMap.Core.Geometry.Triangle;
using ActionStreetMap.Core.Geometry.Triangle.Geometry;

namespace ActionStreetMap.Core.Scene.Terrain
{
    internal sealed class MeshRegion: IDisposable
    {
        public string GradientKey;
        public float ElevationNoiseFreq;
        public float ColorNoiseFreq;
        public Action<Mesh> ModifyMeshAction;

        public Mesh Mesh;

        public List<List<Point>> Contours;

        /// <inheritdoc />
        public void Dispose()
        {
            TrianglePool.FreeMesh(Mesh);
        }
    }
}
