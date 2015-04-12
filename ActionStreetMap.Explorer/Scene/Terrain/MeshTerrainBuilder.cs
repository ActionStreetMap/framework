using System;
using System.Collections.Generic;
using System.IO;
using ActionStreetMap.Core;
using ActionStreetMap.Core.Geometry.Triangle;
using ActionStreetMap.Core.Geometry.Triangle.Geometry;
using ActionStreetMap.Core.Geometry.Triangle.Topology;
using ActionStreetMap.Core.MapCss.Domain;
using ActionStreetMap.Core.Scene.Terrain;
using ActionStreetMap.Core.Tiling.Models;
using ActionStreetMap.Core.Unity;
using ActionStreetMap.Explorer.Geometry;
using ActionStreetMap.Explorer.Geometry.Utils;
using ActionStreetMap.Explorer.Helpers;
using ActionStreetMap.Explorer.Infrastructure;
using ActionStreetMap.Explorer.Utils;
using ActionStreetMap.Infrastructure.Config;
using ActionStreetMap.Infrastructure.Dependencies;
using ActionStreetMap.Infrastructure.Diagnostic;
using ActionStreetMap.Infrastructure.Reactive;
using ActionStreetMap.Infrastructure.Utilities;
using ActionStreetMap.Unity.Wrappers;
using UnityEngine;
using Mesh = UnityEngine.Mesh;

namespace ActionStreetMap.Explorer.Scene.Terrain
{
    public interface ITerrainBuilder
    {
        IGameObject Build(Tile tile, Rule rule);
    }

    internal class MeshTerrainBuilder : ITerrainBuilder, IConfigurable
    {
        private const string LogTag = "mesh.terrain";

        private const float WaterBottomLevelOffset = 5f;
        private const float WaterSurfaceLevelOffset = 2.5f;

        private readonly IElevationProvider _elevationProvider;
        private readonly IResourceProvider _resourceProvider;
        private readonly IGameObjectFactory _gameObjectFactory;
        private readonly IObjectPool _objectPool;
        private readonly MeshCellBuilder _meshCellBuilder;

        [Dependency]
        public ITrace Trace { get; set; }

        private float _maxCellSize = 100;

        [Dependency]
        public MeshTerrainBuilder(IElevationProvider elevationProvider,
                                  IResourceProvider resourceProvider,
                                  IGameObjectFactory gameObjectFactory,
                                  IObjectPool objectPool)
        {
            _elevationProvider = elevationProvider;
            _resourceProvider = resourceProvider;
            _gameObjectFactory = gameObjectFactory;
            _objectPool = objectPool;
            _meshCellBuilder = new MeshCellBuilder();
        }

        public IGameObject Build(Tile tile, Rule rule)
        {
            Trace.Debug(LogTag, "Started to build terrain");
            var sw = new System.Diagnostics.Stopwatch();
            sw.Start();

            var terrainObject = _gameObjectFactory.CreateNew("terrain", tile.GameObject);

            // detect grid parameters
            var cellRowCount = (int) Math.Ceiling(tile.Height/_maxCellSize);
            var cellColumnCount = (int) Math.Ceiling(tile.Width/_maxCellSize);
            var cellHeight = tile.Height/cellRowCount;
            var cellWidth = tile.Width/cellColumnCount;

            Trace.Debug(LogTag, "Building mesh canvas..");
            var meshCanvas = new MeshCanvasBuilder()
                .SetTile(tile)
                .SetScale(MeshCellBuilder.Scale)
                .Build();

            Trace.Debug(LogTag, "Building mesh cells..");
            var tasks = new List<IObservable<Unit>>(cellRowCount*cellColumnCount);
            for (int j = 0; j < cellRowCount; j++)
                for (int i = 0; i < cellColumnCount; i++)
                {
                    var tileBottomLeft = tile.Rectangle.BottomLeft;
                    var rectangle = new Rectangle(
                        tileBottomLeft.X + i*cellWidth,
                        tileBottomLeft.Y + j*cellHeight,
                        cellWidth,
                        cellHeight);
                    var name = String.Format("cell {0}_{1}", i, j);
                    tasks.Add(Observable.Start(() =>
                    {
                        var cell = _meshCellBuilder.Build(rectangle, meshCanvas);
                        BuildCell(rule, terrainObject, cell, rectangle, name);
                    }));
                }

            tasks.WhenAll().Wait();

            sw.Stop();
            Trace.Debug(LogTag, "Terrain is build in {0}ms", sw.ElapsedMilliseconds);

            return terrainObject;
        }

        private void BuildCell(Rule rule, IGameObject terrainObject, MeshCell cell, Rectangle cellRect, string name)
        {
            var cellGameObject = _gameObjectFactory.CreateNew(name, terrainObject);

            var meshData = _objectPool.CreateMeshData();
            meshData.GameObject = cellGameObject;

            var rect = new MapRectangle((float) cellRect.Left, (float) cellRect.Bottom,
                (float) cellRect.Width, (float) cellRect.Height);

            var index = new MeshIndex(16, 16, rect);
            var context = new MeshContext
            {
                Rule = rule,
                Data = meshData,
                Index = index
            };

            // build canvas
            BuildBackground(context, cell.Background);
            // build extra layers
            BuildWater(context, cell.Water);
            BuildCarRoads(context, cell.CarRoads);
            BuildPedestrianLayers(context, cell.WalkRoads);
            foreach (var surfaceRegion in cell.Surfaces)
                BuildSurface(context, surfaceRegion);

            Trace.Debug(LogTag, "Total triangles: {0}", meshData.Triangles.Count);

            index.BuiltIndex(meshData.Triangles);

            Vector3[] vertices;
            int[] triangles;
            Color[] colors;
            meshData.GenerateObjectData(out vertices, out triangles, out colors);

            _objectPool.RecycleMeshData(meshData);

            Scheduler.MainThread.Schedule(() => BuildObject(cellGameObject, rule,
                index, vertices, triangles, colors));
        }

        #region Water layer

        protected void BuildWater(MeshContext context, MeshRegion meshRegion)
        {
            if (meshRegion.Mesh == null) return;

            const float colorNoiseFreq = 0.2f;
            const float eleNoiseFreq = 0.2f;

            // TODO allocate from pool with some size
            var waterVertices = new List<Vector3>();
            var waterTriangles = new List<int>();
            var waterColors = new List<Color>();

            var meshTriangles = context.Data.Triangles;

            var bottomGradient = context.Rule.GetBackgroundLayerGradient(_resourceProvider);
            var waterSurfaceGradient = context.Rule.GetWaterLayerGradient(_resourceProvider);
            int count = 0;
            foreach (var triangle in meshRegion.Mesh.Triangles)
            {
                // bottom
                AddTriangle(context, triangle, bottomGradient, eleNoiseFreq, colorNoiseFreq, -WaterBottomLevelOffset);

                var meshTriangle = meshTriangles[meshTriangles.Count - 1];

                var p0 = meshTriangle.Vertex0;
                var p1 = meshTriangle.Vertex1;
                var p2 = meshTriangle.Vertex2;

                // reuse just added vertices
                waterVertices.Add(new Vector3(p0.X, p0.Elevation + WaterBottomLevelOffset - WaterSurfaceLevelOffset, p0.Y));
                waterVertices.Add(new Vector3(p1.X, p1.Elevation + WaterBottomLevelOffset - WaterSurfaceLevelOffset, p1.Y));
                waterVertices.Add(new Vector3(p2.X, p2.Elevation + WaterBottomLevelOffset - WaterSurfaceLevelOffset, p2.Y));

                var color = GradientUtils.GetColor(waterSurfaceGradient, waterVertices[count], colorNoiseFreq);
                waterColors.Add(color);
                waterColors.Add(color);
                waterColors.Add(color);

                waterTriangles.Add(count);
                waterTriangles.Add(count + 2);
                waterTriangles.Add(count + 1);
                count += 3;
            }
            BuildOffsetShape(context, meshRegion, context.Rule.GetBackgroundLayerGradient(_resourceProvider), WaterBottomLevelOffset);
            var vs = waterVertices.ToArray();
            var ts = waterTriangles.ToArray();
            var cs = waterColors.ToArray();
            Scheduler.MainThread.Schedule(() => BuildWaterObject(context, vs, ts, cs));
        }

        protected void BuildOffsetShape(MeshContext context, MeshRegion region, GradientWrapper gradient,
            float deepLevel)
        {
            const float colorNoiseFreq = 0.2f;
            const float divideStep = 1f;
            const float errorTopFix = 0.02f;
            const float errorBottomFix = 0.1f;

            var triangles = context.Data.Triangles;

            var pointList = _objectPool.NewList<MapPoint>(64);
            foreach (var contour in region.Contours)
            {
                var length = contour.Count;
                for (int i = 0; i < length; i++)
                {
                    var v2DIndex = i == (length - 1) ? 0 : i + 1;
                    var start = new MapPoint((float) contour[i].X, (float) contour[i].Y);
                    var end = new MapPoint((float) contour[v2DIndex].X, (float) contour[v2DIndex].Y);

                    LineUtils.DivideLine(_elevationProvider, start, end, pointList, divideStep);

                    for (int k = 1; k < pointList.Count; k++)
                    {
                        var p1 = pointList[k - 1];
                        var p2 = pointList[k];

                        // vertices
                        var ele1 = _elevationProvider.GetElevation(p1);
                        var ele2 = _elevationProvider.GetElevation(p2);

                        var firstColor = GradientUtils.GetColor(gradient, new Vector3(p1.X, 0, p1.Y), colorNoiseFreq);
                        var secondColor = GradientUtils.GetColor(gradient, new Vector3(p2.X, 0, p2.Y), colorNoiseFreq);

                        context.Data.AddTriangle(
                            new MapPoint(p1.X, p1.Y, ele1 + errorTopFix),
                            new MapPoint(p2.X, p2.Y, ele2 - deepLevel - errorBottomFix),
                            new MapPoint(p2.X, p2.Y, ele2 + errorTopFix),
                            firstColor);

                        context.Data.AddTriangle(
                            new MapPoint(p1.X, p1.Y, ele1 - deepLevel - errorBottomFix),
                            new MapPoint(p2.X, p2.Y, ele2 - deepLevel - errorBottomFix),
                            new MapPoint(p1.X, p1.Y, ele1 + errorTopFix),
                            secondColor);

                        // TODO refactor this
                        context.Index.AddToIndex(triangles[triangles.Count - 2]);
                        context.Index.AddToIndex(triangles[triangles.Count - 1]);
                    }

                    pointList.Clear();
                }
            }
            _objectPool.StoreList(pointList);
        }


        private void BuildWaterObject(MeshContext context, Vector3[] vertices, int[] triangles, Color[] colors)
        {
            var gameObject = new GameObject("water");
            gameObject.transform.parent = context.Data.GameObject.GetComponent<GameObject>().transform;
            var meshData = new Mesh();
            meshData.vertices = vertices;
            meshData.triangles = triangles;
            meshData.colors = colors;
            meshData.RecalculateNormals();

            // NOTE this script is too expensive to run!
            //gameObject.AddComponent<NoiseWaveBehavior>();
            gameObject.AddComponent<MeshRenderer>().material = context.Rule.GetMaterial("material_water", _resourceProvider);
            gameObject.AddComponent<MeshFilter>().mesh = meshData;
        }

        #endregion

        #region Background layer

        protected void BuildBackground(MeshContext context, MeshRegion meshRegion)
        {
            if (meshRegion.Mesh == null) return;
            var gradient = context.Rule.GetBackgroundLayerGradient(_resourceProvider);

            const float eleNoiseFreq = 0.2f;
            const float colorNoiseFreq = 0.2f;
            foreach (var triangle in meshRegion.Mesh.Triangles)
                AddTriangle(context, triangle, gradient, eleNoiseFreq, colorNoiseFreq);
        }

        #endregion

        #region Car roads layer

        protected void BuildCarRoads(MeshContext context, MeshRegion meshRegion)
        {
            const float eleNoiseFreq = 0f;
            const float colorNoiseFreq = 0.2f;

            if (meshRegion.Mesh == null) return;
            var gradient = context.Rule.GetCarLayerGradient(_resourceProvider);

            foreach (var triangle in meshRegion.Mesh.Triangles)
                AddTriangle(context, triangle, gradient, eleNoiseFreq, colorNoiseFreq, 0);
        }

        #endregion

        #region Pedestrian roads layer

        protected void BuildPedestrianLayers(MeshContext context, MeshRegion meshRegion)
        {
            if (meshRegion.Mesh == null) return;
            var gradient = context.Rule.GetPedestrianLayerGradient(_resourceProvider);
            const float eleNoiseFreq = 0f;
            const float colorNoiseFreq = 1f;
            foreach (var triangle in meshRegion.Mesh.Triangles)
                AddTriangle(context, triangle, gradient, eleNoiseFreq, colorNoiseFreq);
        }

        #endregion

        #region Surface layer

        protected void BuildSurface(MeshContext context, MeshRegion meshRegion)
        {
            if (meshRegion.Mesh == null) return;
            const float colorNoiseFreq = 0.2f;
            const float eleNoiseFreq = 0.2f;
            var gradient = _resourceProvider.GetGradient(meshRegion.GradientKey);

            if (meshRegion.ModifyMeshAction != null)
                meshRegion.ModifyMeshAction(meshRegion.Mesh);

            foreach (var triangle in meshRegion.Mesh.Triangles)
                AddTriangle(context, triangle, gradient, eleNoiseFreq, colorNoiseFreq);
        }

        #endregion

        #region Layer builder helper methods

        protected void AddTriangle(MeshContext context, Triangle triangle, GradientWrapper gradient,
            float eleNoiseFreq, float colorNoiseFreq, float yOffset = 0)
        {
            var meshTriangle = new MeshTriangle();
            var useEleNoise = eleNoiseFreq != 0f;

            meshTriangle.Vertex0 = GetVertex(triangle.GetVertex(0), eleNoiseFreq, useEleNoise, yOffset);
            meshTriangle.Vertex1 = GetVertex(triangle.GetVertex(1), eleNoiseFreq, useEleNoise, yOffset);
            meshTriangle.Vertex2 = GetVertex(triangle.GetVertex(2), eleNoiseFreq, useEleNoise, yOffset);

            var v0 = meshTriangle.Vertex0;
            var triangleColor = GradientUtils.GetColor(gradient, new Vector3(v0.X, 0, v0.Y), colorNoiseFreq);

            meshTriangle.Color0 = triangleColor;
            meshTriangle.Color1 = triangleColor;
            meshTriangle.Color2 = triangleColor;

            context.Index.AddToIndex(meshTriangle);

            context.Data.Triangles.Add(meshTriangle);
        }

        private MapPoint GetVertex(Vertex v2, float eleNoiseFreq, bool useEleNoise, float yOffset)
        {
            var p2 = new MapPoint((float)v2.X, (float)v2.Y);
            var useEleNoise2 = v2.Type == VertexType.FreeVertex && useEleNoise;
            var ele2 = _elevationProvider.GetElevation(p2);
            if (useEleNoise2)
                ele2 += Noise.Perlin3D(new Vector3(p2.X, 0, p2.Y), eleNoiseFreq);
            return new MapPoint(p2.X, p2.Y, ele2 + yOffset);
        }

        #endregion

        private void BuildObject(IGameObject goWrapper, Rule rule, MeshIndex index,
            Vector3[] vertices, int[] triangles, Color[] colors)
        {
            var gameObject = goWrapper.GetComponent<GameObject>();

            var meshData = new Mesh();
            meshData.vertices = vertices;
            meshData.triangles = triangles;
            meshData.colors = colors;
            meshData.RecalculateNormals();

            gameObject.AddComponent<MeshRenderer>().material = rule.GetMaterial("material_background", _resourceProvider);
            gameObject.AddComponent<MeshFilter>().mesh = meshData;
            gameObject.AddComponent<MeshCollider>();

            gameObject.AddComponent<MeshBehaviour>().Index = index;
        }

        public void Configure(IConfigSection configSection)
        {
            _maxCellSize = configSection.GetFloat("cell.size", 100);
            var maxArea = configSection.GetFloat("maxArea", 10);

            _meshCellBuilder.SetMaxArea(maxArea);
        }
    }
}