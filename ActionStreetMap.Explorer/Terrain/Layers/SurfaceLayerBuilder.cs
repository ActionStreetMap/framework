using ActionStreetMap.Core.Polygons.Meshing.Iterators;
using ActionStreetMap.Core.Polygons.Tools;
using ActionStreetMap.Core.Polygons.Topology;
using ActionStreetMap.Core.Terrain;
using UnityEngine;

namespace ActionStreetMap.Explorer.Terrain.Layers
{
    internal class SurfaceLayerBuilder : LayerBuilder
    {
        private const string LogTag = "layer.surface";
        public string Name { get { return "surface"; } }

        public void Build(MeshContext context, MeshRegion meshRegion)
        {
            var colors = context.Colors;
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
                var color = GetColorBySplatId(fillRegion.SplatId);
                context.Iterator.Process(start, triangle =>
                {
                    var index = hashMap[triangle.GetHashCode()];
                    colors[index] = color;
                    colors[index + 1] = color;
                    colors[index + 2] = color;
                    count++;
                });
                Trace.Debug(LogTag, "Surface region processed: {0}", count);
            }
        }

        private Color GetColorBySplatId(int id)
        {
            switch (id % 3)
            {
                case 0: return Color.yellow;
                case 1: return Color.green;
                default: return Color.blue;
            }
        }
    }
}
