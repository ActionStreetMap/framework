using ActionStreetMap.Core;
using ActionStreetMap.Core.Terrain;
using ActionStreetMap.Explorer.Scene.Utils;
using ActionStreetMap.Explorer.Utils;
using ActionStreetMap.Infrastructure.Dependencies;
using ActionStreetMap.Infrastructure.Diagnostic;
using ActionStreetMap.Unity.Wrappers;
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

        [Dependency]
        public IResourceProvider ResourceProvider { get; set; }

        public abstract string Name { get; }
        public abstract void Build(MeshContext context, MeshRegion meshRegion);

        public Color GetColor(GradientWrapper gradientWrapper, Vector3 point, float freq)
        {
            var value = (Noise.Perlin3D(point, freq) + 1f) / 2f;
            return gradientWrapper.Evaluate(value);
        }

        protected void BuildOffsetShape(MeshContext context, MeshRegion region, GradientWrapper gradient, float deepLevel)
        {
            var vertices = context.Vertices;
            var triangles = context.Triangles;
            var colors = context.Colors;
            var colorNoiseFreq = 0.2f;
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

                    var firstColor = GetColor(gradient, new Vector3(p1.X, 0, p1.Y), colorNoiseFreq);
                    var secondColor = GetColor(gradient, new Vector3(p2.X, 0, p2.Y), colorNoiseFreq);

                    colors.Add(firstColor);
                    colors.Add(secondColor);
                    colors.Add(secondColor);
                    colors.Add(firstColor);
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