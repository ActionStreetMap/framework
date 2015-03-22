using ActionStreetMap.Core.Polygons.Topology;
using ActionStreetMap.Core.Terrain;
using ActionStreetMap.Explorer.Utils;
using UnityEngine;

namespace ActionStreetMap.Explorer.Scene.Terrain
{
    internal class SurfaceLayerBuilder : LayerBuilder
    {
        private const string LogTag = "layer.surface";

        public override string Name { get { return "surface"; } }

        public override void Build(MeshContext context, MeshRegion meshRegion)
        {
            var colors = context.Data.Colors;
            var hashMap = context.TriangleMap;
            var colorNoiseFreq = 0.2f;
            foreach (var fillRegion in meshRegion.FillRegions)
            {
                var point = fillRegion.Anchor;
                var start = (Triangle) context.Tree.Query(point.X, point.Y);
                if (start == null)
                {
                    Trace.Warn(LogTag, "Broken surface region");
                    continue;
                }
                int count = 0;
                var gradient = ResourceProvider.GetGradient(fillRegion.GradientKey);
                context.Iterator.Process(start, triangle =>
                {
                    var index = hashMap[triangle.GetHashCode()];
                    var vertex = triangle.GetVertex(0);
                    var color = GradientUtils.GetColor(gradient, new Vector3((float)vertex.X, 0, (float)vertex.Y), colorNoiseFreq);
                    colors[index] = color;
                    colors[index + 1] = color;
                    colors[index + 2] = color;
                    count++;
                });
                Trace.Debug(LogTag, "Surface region processed: {0}", count);
            }
        }
    }
}