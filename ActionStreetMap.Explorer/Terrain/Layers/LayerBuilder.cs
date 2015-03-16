using ActionStreetMap.Core;
using ActionStreetMap.Core.Terrain;
using ActionStreetMap.Infrastructure.Dependencies;
using ActionStreetMap.Infrastructure.Diagnostic;
using UnityEngine;

namespace ActionStreetMap.Explorer.Terrain.Layers
{
    internal interface ILayerBuilder
    {
        string Name { get; }

        void Build(MeshContext context, MeshRegion meshRegion);
    }

    internal abstract class LayerBuilder : ILayerBuilder
    {
        [Dependency]
        public ITrace Trace { get; set; }

        [Dependency]
        public IElevationProvider ElevationProvider { get; set; }

        public abstract string Name { get; }
        public abstract void Build(MeshContext context, MeshRegion meshRegion);

        protected void BuildOffsetShape(MeshContext context, MeshRegion region, float deepLevel)
        {
            var vertices = context.Vertices;
            var triangles = context.Triangles;
            var colors = context.Colors;
            foreach (var contour in region.Contours)
            {
                var length = contour.Count;
                var vertOffset = vertices.Count;
                // vertices
                for (int i = 0; i < length; i++)
                {
                    var v2DIndex = i == (length - 1) ? 0 : i + 1;

                    var p1 = new MapPoint((float) contour[i].X, (float) contour[i].Y);
                    var p2 = new MapPoint((float) contour[v2DIndex].X, (float) contour[v2DIndex].Y);
                    var ele1 = ElevationProvider.GetElevation(p1);
                    var ele2 = ElevationProvider.GetElevation(p2);

                    vertices.Add(new Vector3(p1.X, ele1, p1.Y));
                    vertices.Add(new Vector3(p2.X, ele2, p2.Y));
                    vertices.Add(new Vector3(p2.X, ele2 - deepLevel, p2.Y));
                    vertices.Add(new Vector3(p1.X, ele1 - deepLevel,  p1.Y));

                    // TODO detect color
                    colors.Add(Color.magenta);
                    colors.Add(Color.magenta);
                    colors.Add(Color.magenta);
                    colors.Add(Color.magenta);
                }

                // triangles
                for (int i = 0; i < length; i++)
                {
                    var vIndex = vertOffset + i*4;
                    triangles.Add(vIndex);
                    triangles.Add(vIndex + 2);
                    triangles.Add(vIndex + 1);

                    triangles.Add(vIndex + 3);
                    triangles.Add(vIndex + 2);
                    triangles.Add(vIndex + 0);
                }
            }
        }
    }
}