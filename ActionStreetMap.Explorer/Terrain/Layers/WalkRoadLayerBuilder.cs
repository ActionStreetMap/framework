using ActionStreetMap.Core.Polygons.Meshing.Iterators;
using ActionStreetMap.Core.Polygons.Tools;
using ActionStreetMap.Core.Polygons.Topology;
using ActionStreetMap.Core.Terrain;
using ActionStreetMap.Infrastructure.Dependencies;
using ActionStreetMap.Infrastructure.Diagnostic;
using UnityEngine;

namespace ActionStreetMap.Explorer.Terrain.Layers
{
    internal class WalkRoadLayerBuilder : LayerBuilder
    {
        private const string LogTag = "layer.walk";

        public string Name { get { return "walk"; } }

        public void Build(MeshContext context, MeshRegion meshRegion)
        {
            var colors = context.Colors;
            foreach (var region in meshRegion.FillRegions)
            {
                var point = region.Anchor;
                var start = (Triangle) context.Tree.Query(point.X, point.Y);
                if (start == null)
                {
                    Trace.Warn(LogTag, "Broken walk road region");
                    continue;
                }
                int count = 0;
                var color = Color.yellow;

                context.Iterator.Process(start, triangle =>
                {
                    var index = hashMap[triangle.GetHashCode()];
                    colors[index] = color;
                    colors[index + 1] = color;
                    colors[index + 2] = color;

                    count++;
                });
                Trace.Debug(LogTag, "Walk road region processed: {0}", count);
            }
        }
    }
}