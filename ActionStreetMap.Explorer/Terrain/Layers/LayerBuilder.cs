using ActionStreetMap.Core;
using ActionStreetMap.Core.Terrain;
using ActionStreetMap.Explorer.Geometry;
using ActionStreetMap.Explorer.Geometry.Utils;
using ActionStreetMap.Explorer.Scene;
using ActionStreetMap.Explorer.Utils;
using ActionStreetMap.Infrastructure.Dependencies;
using ActionStreetMap.Infrastructure.Diagnostic;
using ActionStreetMap.Infrastructure.Utilities;
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
        public IObjectPool ObjectPool { get; set; }

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
            var colorNoiseFreq = 0.2f;
            var divideStep = 1f;
            var errorTopFix = 0.02f;
            var errorBottomFix = 0.1f;

            var vertices = context.Vertices;
            var triangles = context.Triangles;
            var colors = context.Colors;
            var pointList = ObjectPool.NewList<MapPoint>(64);
            foreach (var contour in region.Contours)
            {
                var length = contour.Count;
                for (int i = 0; i < length; i++)
                {
                    var v2DIndex = i == (length - 1) ? 0 : i + 1;
                    var start = new MapPoint((float) contour[i].X, (float) contour[i].Y);
                    var end = new MapPoint((float) contour[v2DIndex].X, (float) contour[v2DIndex].Y);

                    LineUtils.DivideLine(ElevationProvider, start, end, pointList, divideStep);

                    for (int k = 1; k < pointList.Count; k++)
                    {
                        var p1 = pointList[k - 1];
                        var p2 = pointList[k];

                        // vertices
                        var ele1 = ElevationProvider.GetElevation(p1);
                        var ele2 = ElevationProvider.GetElevation(p2);
                        vertices.Add(new Vector3(p1.X, ele1 + errorTopFix, p1.Y));
                        vertices.Add(new Vector3(p2.X, ele2 + errorTopFix, p2.Y));
                        vertices.Add(new Vector3(p2.X, ele2 - deepLevel - errorBottomFix, p2.Y));
                        vertices.Add(new Vector3(p1.X, ele1 - deepLevel - errorBottomFix, p1.Y));

                        // colors
                        var firstColor = GetColor(gradient, new Vector3(p1.X, 0, p1.Y), colorNoiseFreq);
                        var secondColor = GetColor(gradient, new Vector3(p2.X, 0, p2.Y), colorNoiseFreq);

                        colors.Add(firstColor);
                        colors.Add(secondColor);
                        colors.Add(secondColor);
                        colors.Add(firstColor);
                        
                        // triangles
                        var vIndex = vertices.Count - 4;
                        triangles.Add(vIndex);
                        triangles.Add(vIndex + 2);
                        triangles.Add(vIndex + 1);

                        triangles.Add(vIndex + 3);
                        triangles.Add(vIndex + 2);
                        triangles.Add(vIndex + 0);
                    }

                    pointList.Clear();
                }
            }
            ObjectPool.StoreList(pointList);
        }
    }
}