using ActionStreetMap.Core.Polygons;
using ActionStreetMap.Core.Polygons.Meshing.Iterators;
using ActionStreetMap.Core.Polygons.Tools;
using ActionStreetMap.Core.Polygons.Topology;
using ActionStreetMap.Core.Terrain;
using ActionStreetMap.Explorer.Terrain;
using ActionStreetMap.Explorer.Utils;
using ActionStreetMap.Infrastructure.Dependencies;
using ActionStreetMap.Infrastructure.Diagnostic;
using UnityEngine;

namespace ActionStreetMap.Explorer.Terrain.Layers
{
    internal class CarRoadLayerBuilder : LayerBuilder
    {
        private const string LogTag = "layer.car";
        private const float RoadDeepLevel = 0.2f;

        public string Name { get { return "car"; } }

        [Dependency]
        public ITrace Trace { get; set; }

        public void Build(MeshContext context, MeshRegion meshRegion)
        {
            var colors = context.Colors;
            var vertices = context.Vertices;
            foreach (var region in meshRegion.FillRegions)
            {
                var point = region.Anchor;
                var start = (Triangle)context.Tree.Query(point.X, point.Y);
                if (start == null)
                {
                    Trace.Warn(LogTag, "Broken car road region");
                    continue;
                }

                int count = 0;
                var color = Color.red;
                context.Iterator.Process(start, triangle =>
                {
                    var index = hashMap[triangle.GetHashCode()];
                    colors[index] = color;
                    colors[index + 1] = color;
                    colors[index + 2] = color;

                    var p1 = vertices[index];
                    float ele1 = 0;
                    if (triangle.GetVertex(0).Type == VertexType.FreeVertex)
                        ele1 = Noise.Perlin3D(new Vector3(p1.x, 0, p1.z), 0.1f);
                    vertices[index] = new Vector3(p1.x, p1.y - RoadDeepLevel - ele1, p1.z);

                    var p2 = vertices[index + 1];
                    float ele2 = 0;
                    if (triangle.GetVertex(1).Type == VertexType.FreeVertex)
                        ele2 = Noise.Perlin3D(new Vector3(p2.x, 0, p2.z), 0.1f);

                    vertices[index + 1] = new Vector3(p2.x, p2.y - RoadDeepLevel - ele2, p2.z);

                    var p3 = vertices[index + 2];
                    float ele3 = 0;
                    if (triangle.GetVertex(2).Type == VertexType.FreeVertex)
                        ele3 = Noise.Perlin3D(new Vector3(p3.x, 0, p3.z), 0.1f);
                    vertices[index + 2] = new Vector3(p3.x, p3.y - RoadDeepLevel - ele3, p3.z);

                    count++;
                });
                Trace.Debug(LogTag, "Car road region processed: {0}", count);
            }

            BuildOffsetShape(context, meshRegion, RoadDeepLevel);
        }
    }
}