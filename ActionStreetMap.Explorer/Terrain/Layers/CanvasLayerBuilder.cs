using ActionStreetMap.Core;
using ActionStreetMap.Core.Polygons;
using ActionStreetMap.Core.Terrain;
using ActionStreetMap.Explorer.Utils;
using UnityEngine;

namespace ActionStreetMap.Explorer.Terrain.Layers
{
    internal class CanvasLayerBuilder: LayerBuilder
    {
        public override string Name { get { return "canvas"; } }

        public override void Build(MeshContext context, MeshRegion meshRegion)
        {
            var gradient = ResourceProvider.GetGradient("canvas");
            var terrainMesh = context.Mesh;
            var colors = context.Data.Colors;
            var vertices = context.Data.Vertices;
            var triangles = context.Data.Triangles;
            var triangleIndexMap = context.TriangleMap;
            var eleNoiseFreq = 0.2f;
            var colorNoiseFreq = 0.2f;
            foreach (var triangle in terrainMesh.Triangles)
            {
                var v0 = triangle.GetVertex(0);
                var p0 = new MapPoint((float)v0.X, (float)v0.Y);
                var ele0 = ElevationProvider.GetElevation(p0);
                if (v0.Type == VertexType.FreeVertex)
                    ele0 += Noise.Perlin3D(new Vector3(p0.X, 0, p0.Y), eleNoiseFreq);
                vertices.Add(new Vector3(p0.X, ele0, p0.Y));

                var v1 = triangle.GetVertex(1);
                var p1 = new MapPoint((float)v1.X, (float)v1.Y);
                var ele1 = ElevationProvider.GetElevation(p1);
                if (v1.Type == VertexType.FreeVertex)
                    ele1 += Noise.Perlin3D(new Vector3(p1.X, 0, p1.Y), eleNoiseFreq);
                vertices.Add(new Vector3(p1.X, ele1, p1.Y));

                var v2 = triangle.GetVertex(2);
                var p2 = new MapPoint((float)v2.X, (float)v2.Y);
                var ele2 = ElevationProvider.GetElevation(p2);
                if (v2.Type == VertexType.FreeVertex)
                    ele2 += Noise.Perlin3D(new Vector3(p2.X, 0, p2.Y), eleNoiseFreq);
                vertices.Add(new Vector3(p2.X, ele2, p2.Y));

                var index = vertices.Count;
                triangles.Add(--index);
                triangles.Add(--index);
                triangles.Add(--index);

                var triangleColor = GetColor(gradient, new Vector3((float)v0.X, ele0, (float)v0.Y), colorNoiseFreq);

                colors.Add(triangleColor);
                colors.Add(triangleColor);
                colors.Add(triangleColor);

                triangleIndexMap.Add(triangle.GetHashCode(), index);
            }
        }
    }
}
