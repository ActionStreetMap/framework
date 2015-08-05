using System;
using System.Collections.Generic;
using ActionStreetMap.Core.Geometry;
using ActionStreetMap.Core.Geometry.Utils;
using ActionStreetMap.Core.MapCss.Domain;
using ActionStreetMap.Core.Tiling.Models;
using ActionStreetMap.Core.Unity;
using ActionStreetMap.Explorer.Helpers;
using ActionStreetMap.Explorer.Scene.Indices;
using ActionStreetMap.Explorer.Utils;
using ActionStreetMap.Unity.Wrappers;
using UnityEngine;

namespace ActionStreetMap.Explorer.Scene.Builders
{
    /// <summary> Provides logic to build various barriers. </summary>
    public class BarrierModelBuilder: ModelBuilder
    {
        /// <inheritdoc />
        public override string Name { get { return "barrier"; } }

        /// <inheritdoc />
        public override IGameObject BuildWay(Tile tile, Rule rule, Way way)
        {
            if (way.Points.Count < 2)
            {
                Trace.Warn("model.barrier", Strings.InvalidPolyline);
                return null;
            }

            if (tile.Registry.Contains(way.Id)) 
                return null;
            
            tile.Registry.RegisterGlobal(way.Id);

            var gameObjectWrapper = GameObjectFactory.CreateNew(GetName(way));
            var maxWidth = 4f;

            var points = ObjectPool.NewList<Vector2d>(way.Points.Count);
            PointUtils.SetPolygonPoints(tile.RelativeNullPoint, way.Points, points);

            var vertexCount = GetVertexCount(points, maxWidth);
            var meshIndex = new MultiPlaneMeshIndex(points.Count - 1, vertexCount);
            var meshData = new MeshData(meshIndex)
            {
                MaterialKey = rule.GetMaterialKey(),
                GameObject = gameObjectWrapper,
            };
            meshData.Initialize(vertexCount, true);
            
            meshData.Index = meshIndex;
            var context = new SegmentBuilderContext()
            {
                MeshData = meshData,
                Gradient = ResourceProvider.GetGradient(rule.GetFillColor()),
                ColorNoiseFreq = rule.GetColorNoiseFreq(),
                Height = rule.GetHeight(),
                MaxWidth = maxWidth,
            };

            var index = 0;
            for (int i = 0; i < points.Count - 1; i++)
            {
                var p1 = points[i];
                var p2 = points[i + 1];

                var start = new Vector3((float)p1.X, ElevationProvider.GetElevation(p1), (float) p1.Y);
                var end = new Vector3((float)p2.X, ElevationProvider.GetElevation(p2), (float)p2.Y);
                
                meshIndex.AddPlane(new Vector3((float)p1.X, 0, (float)p1.Y), start, end, meshData.NextIndex);
                
                BuildBarrierSegment(context, start, end, ref index);
            }
            ObjectPool.StoreList(points);
            BuildObject(tile.GameObject, meshData, rule, way);

            return gameObjectWrapper;
        }

        private int GetVertexCount(List<Vector2d> points, float maxWidth)
        {
            int count = 0;
            for (int i = 0; i < points.Count - 1; i++)
            {
                var p1 = points[i];
                var p2 = points[i + 1];

                var start = new Vector3((float) p1.X, ElevationProvider.GetElevation(p1), (float) p1.Y);
                var end = new Vector3((float) p2.X, ElevationProvider.GetElevation(p2), (float) p2.Y);
                var distance = Vector3.Distance(start, end);
                count += (int) Math.Ceiling(distance / maxWidth) * 12;
            }
            return count;
        }

        private void BuildBarrierSegment(SegmentBuilderContext context, Vector3 start, Vector3 end,
            ref int startIndex)
        {
            var distance = Vector3.Distance(start, end);
            var direction = (end - start).normalized;

            var stepCount = (int) Math.Ceiling(distance/context.MaxWidth);
            var width = distance / stepCount;

            var startEle = ElevationProvider.GetElevation(start.x, start.z);

            // read context properties
            var gradient = context.Gradient;
            var colorNoiseFreq = context.ColorNoiseFreq;
            var vertCount = context.MeshData.Vertices.Length;
            var vertices = context.MeshData.Vertices;
            var triangles = context.MeshData.Triangles;
            var colors = context.MeshData.Colors;

            for (int z = 0; z < stepCount; z++)
            {
                // get next points
                end = start + direction * width;
                var middle = start + direction * (0.5f * width);
                float endEle = ElevationProvider.GetElevation(end.x, end.z);

                var p0 = new Vector3(start.x, startEle, start.z);
                var p1 = new Vector3(end.x, endEle, end.z);
                var p2 = new Vector3(end.x, endEle + context.Height, end.z);
                var p3 = new Vector3(start.x, startEle + context.Height, start.z);
                var pc = new Vector3(middle.x, startEle + (endEle - startEle) / 2, middle.z);

                var count = startIndex;

                vertices[count] = p3;
                vertices[++count] = pc;
                vertices[++count] = p0;

                vertices[++count] = p0;
                vertices[++count] = pc;
                vertices[++count] = p1;

                vertices[++count] = p1;
                vertices[++count] = pc;
                vertices[++count] = p2;

                vertices[++count] = p2;
                vertices[++count] = pc;
                vertices[++count] = p3;

                // triangles for outer part
                var lastIndex = startIndex + 12;
                for (int i = startIndex; i < lastIndex; i++)
                {
                    triangles[i] = i;
                    var rest = i%3;
                    triangles[vertCount + i] = rest == 0 ? i : (rest == 1 ? i + 1 : i - 1);
                }

                count = startIndex;

                var color = GetColor(gradient, colorNoiseFreq, p3);
                colors[count] = color;
                colors[++count] = color;
                colors[++count] = color;

                color = GetColor(gradient, colorNoiseFreq, p0);
                colors[++count] = color;
                colors[++count] = color;
                colors[++count] = color;

                color = GetColor(gradient, colorNoiseFreq, p1);
                colors[++count] = color;
                colors[++count] = color;
                colors[++count] = color;

                color = GetColor(gradient, colorNoiseFreq, p2);
                colors[++count] = color;
                colors[++count] = color;
                colors[++count] = color;

                // reuse last
                start = end;
                startEle = endEle;
                startIndex = lastIndex;
            }
        }

        protected Color GetColor(GradientWrapper gradient, float colorNoiseFreq, Vector3 point)
        {
            var value = (Noise.Perlin3D(point, colorNoiseFreq) + 1f) / 2f;
            return gradient.Evaluate(value);
        }

        #region Nested class

        private class SegmentBuilderContext
        {
            public MeshData MeshData;
            public GradientWrapper Gradient;
            public float MaxWidth;
            public float Height;
            public float ColorNoiseFreq;
        }

        #endregion
    }
}
