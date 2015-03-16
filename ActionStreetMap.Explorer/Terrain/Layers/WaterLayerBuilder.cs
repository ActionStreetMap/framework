using ActionStreetMap.Core.Polygons.Topology;
using ActionStreetMap.Core.Terrain;
using UnityEngine;

namespace ActionStreetMap.Explorer.Terrain.Layers
{
    internal class WaterLayerBuilder : LayerBuilder
    {
        private const string LogTag = "layer.water";
        private const float WaterDeepLevel = 5;

        public override string Name { get { return "water"; } }

        public override void Build(MeshContext context, MeshRegion meshRegion)
        {
            var vertices = context.Vertices;
            var hashMap = context.TriangleMap;
            var gradient = ResourceProvider.GetGradient(0);
            foreach (var region in meshRegion.FillRegions)
            {
                var point = region.Anchor;
                var start = (Triangle) context.Tree.Query(point.X, point.Y);
                if (start == null)
                {
                    Trace.Warn(LogTag, "Broken water region");
                    continue;
                }
                int count = 0;

                context.Iterator.Process(start, triangle =>
                {
                    var index = hashMap[triangle.GetHashCode()];

                    var p0 = vertices[index];
                    vertices[index] = new Vector3(p0.x, p0.y - WaterDeepLevel, p0.z);

                    var p1 = vertices[index + 1];
                    vertices[index + 1] = new Vector3(p1.x, p1.y - WaterDeepLevel, p1.z);

                    var p2 = vertices[index + 2];
                    vertices[index + 2] = new Vector3(p2.x, p2.y - WaterDeepLevel, p2.z);

                    count++;
                });
                Trace.Debug(LogTag, "Water region processed: {0}", count);
            }

            BuildOffsetShape(context, meshRegion, gradient, WaterDeepLevel);
        }
    }
}