using ActionStreetMap.Core.Polygons;
using ActionStreetMap.Core.Polygons.Topology;
using ActionStreetMap.Core.Terrain;
using ActionStreetMap.Explorer.Utils;
using UnityEngine;

namespace ActionStreetMap.Explorer.Terrain.Layers
{
    internal class CarRoadLayerBuilder : LayerBuilder
    {
        private const string LogTag = "layer.car";
        private const float RoadDeepLevel = 0.2f;

        public override string Name { get { return "car"; } }

        public override void Build(MeshContext context, MeshRegion meshRegion)
        {
            var colors = context.Colors;
            var vertices = context.Vertices;
            var hashMap = context.TriangleMap;
            var gradient = ResourceProvider.GetGradient(1);
            var eleNoiseFreq = 0.5f;
            foreach (var region in meshRegion.FillRegions)
            {
                var point = region.Anchor;
                var start = (Triangle) context.Tree.Query(point.X, point.Y);
                if (start == null)
                {
                    Trace.Warn(LogTag, "Broken car road region");
                    continue;
                }

                int count = 0;
                context.Iterator.Process(start, triangle =>
                {
                    var index = hashMap[triangle.GetHashCode()];

                    var p0 = vertices[index];
                    float ele0 = 0;
                    if (triangle.GetVertex(0).Type == VertexType.FreeVertex)
                        ele0 = Noise.Perlin3D(new Vector3(p0.x, 0, p0.z), eleNoiseFreq);
                    vertices[index] = new Vector3(p0.x, p0.y - RoadDeepLevel - ele0, p0.z);

                    var p1 = vertices[index + 1];
                    float ele1 = 0;
                    if (triangle.GetVertex(1).Type == VertexType.FreeVertex)
                        ele1 = Noise.Perlin3D(new Vector3(p1.x, 0, p1.z), eleNoiseFreq);

                    vertices[index + 1] = new Vector3(p1.x, p1.y - RoadDeepLevel - ele1, p1.z);

                    var p2 = vertices[index + 2];
                    float ele2 = 0;
                    if (triangle.GetVertex(2).Type == VertexType.FreeVertex)
                        ele2 = Noise.Perlin3D(new Vector3(p2.x, 0, p2.z), eleNoiseFreq);
                    vertices[index + 2] = new Vector3(p2.x, p2.y - RoadDeepLevel - ele2, p2.z);

                    // use position of first point only
                    var triangleColor = GetColor(gradient, vertices[index], 0.2f);

                    colors[index] = triangleColor;
                    colors[index + 1] = triangleColor;
                    colors[index + 2] = triangleColor;

                    count++;
                });
                Trace.Debug(LogTag, "Car road region processed: {0}", count);
            }

            BuildOffsetShape(context, meshRegion, gradient, RoadDeepLevel);
        }
    }
}