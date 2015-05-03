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
using ActionStreetMap.Explorer.Interactions;
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
                    var rectangle = new MapRectangle(
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

        private void BuildCell(Rule rule, IGameObject terrainObject, MeshCell cell, MapRectangle cellRect, string name)
        {
            var cellGameObject = _gameObjectFactory.CreateNew(name, terrainObject);

            var rect = new MapRectangle((float)cellRect.Left, (float)cellRect.Bottom,
                (float)cellRect.Width, (float)cellRect.Height);

            var meshData = _objectPool.CreateMeshData();           
            meshData.GameObject = cellGameObject;
            meshData.Index = new TerrainMeshIndex(16, 16, rect, meshData.Triangles);

            // build canvas
            BuildBackground(rule, meshData, cell.Background);
            // build extra layers
            BuildWater(rule, meshData, cell.Water);
            BuildCarRoads(rule, meshData, cell.CarRoads);
            BuildPedestrianLayers(rule, meshData, cell.WalkRoads);
            foreach (var surfaceRegion in cell.Surfaces)
                BuildSurface(rule, meshData, surfaceRegion);

            Trace.Debug(LogTag, "Total triangles: {0}", meshData.Triangles.Count);

            meshData.Index.Build();

            Vector3[] vertices;
            int[] triangles;
            Color[] colors;
            meshData.GenerateObjectData(out vertices, out triangles, out colors);

            _objectPool.RecycleMeshData(meshData);

            Scheduler.MainThread.Schedule(() => BuildObject(cellGameObject, rule,
                meshData, vertices, triangles, colors));
        }

        #region Water layer

        protected void BuildWater(Rule rule, MeshData meshData, MeshRegion meshRegion)
        {
            if (meshRegion.Mesh == null) return;

            float colorNoiseFreq = rule.GetWaterLayerColorNoiseFreq();
            float eleNoiseFreq = rule.GetWaterLayerEleNoiseFreq();

            // TODO allocate from pool with some size
            var waterVertices = new List<Vector3>();
            var waterTriangles = new List<int>();
            var waterColors = new List<Color>();

            var meshTriangles = meshData.Triangles;

            var bottomGradient = rule.GetBackgroundLayerGradient(_resourceProvider);
            var waterSurfaceGradient = rule.GetWaterLayerGradient(_resourceProvider);
            var waterBottomLevelOffset = rule.GetWaterLayerBottomLevel();
            var waterSurfaceLevelOffset = rule.GetWaterLayerSurfaceLevel();

            int count = 0;
            foreach (var triangle in meshRegion.Mesh.Triangles)
            {
                // bottom
                AddTriangle(rule, meshData, triangle, bottomGradient, eleNoiseFreq, colorNoiseFreq, -waterBottomLevelOffset);

                var meshTriangle = meshTriangles[meshTriangles.Count - 1];

                var p0 = meshTriangle.Vertex0;
                var p1 = meshTriangle.Vertex1;
                var p2 = meshTriangle.Vertex2;

                // reuse just added vertices
                waterVertices.Add(new Vector3(p0.X, p0.Elevation + waterBottomLevelOffset - waterSurfaceLevelOffset, p0.Y));
                waterVertices.Add(new Vector3(p1.X, p1.Elevation + waterBottomLevelOffset - waterSurfaceLevelOffset, p1.Y));
                waterVertices.Add(new Vector3(p2.X, p2.Elevation + waterBottomLevelOffset - waterSurfaceLevelOffset, p2.Y));

                var color = GradientUtils.GetColor(waterSurfaceGradient, waterVertices[count], colorNoiseFreq);
                waterColors.Add(color);
                waterColors.Add(color);
                waterColors.Add(color);

                waterTriangles.Add(count);
                waterTriangles.Add(count + 2);
                waterTriangles.Add(count + 1);
                count += 3;
            }
            BuildOffsetShape(rule, meshData, meshRegion, rule.GetBackgroundLayerGradient(_resourceProvider),
                colorNoiseFreq, waterBottomLevelOffset);
            var vs = waterVertices.ToArray();
            var ts = waterTriangles.ToArray();
            var cs = waterColors.ToArray();
            Scheduler.MainThread.Schedule(() => BuildWaterObject(rule, meshData, vs, ts, cs));
        }

        protected void BuildOffsetShape(Rule rule, MeshData meshData, MeshRegion region, GradientWrapper gradient,
            float colorNoiseFreq, float deepLevel)
        {
            const float divideStep = 1f;
            const float errorTopFix = 0.02f;
            const float errorBottomFix = 0.1f;

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

                        meshData.AddTriangle(
                            new MapPoint(p1.X, p1.Y, ele1 + errorTopFix),
                            new MapPoint(p2.X, p2.Y, ele2 - deepLevel - errorBottomFix),
                            new MapPoint(p2.X, p2.Y, ele2 + errorTopFix),
                            firstColor);

                        meshData.AddTriangle(
                            new MapPoint(p1.X, p1.Y, ele1 - deepLevel - errorBottomFix),
                            new MapPoint(p2.X, p2.Y, ele2 - deepLevel - errorBottomFix),
                            new MapPoint(p1.X, p1.Y, ele1 + errorTopFix),
                            secondColor);
                    }

                    pointList.Clear();
                }
            }
            _objectPool.StoreList(pointList);
        }


        private void BuildWaterObject(Rule rule, MeshData meshData, Vector3[] vertices, int[] triangles, Color[] colors)
        {
            var gameObject = new GameObject("water");
            gameObject.transform.parent = meshData.GameObject.GetComponent<GameObject>().transform;
            var mesh = new Mesh();
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.colors = colors;
            mesh.RecalculateNormals();

            // NOTE this script is too expensive to run!
            //gameObject.AddComponent<NoiseWaveBehavior>();
            gameObject.AddComponent<MeshRenderer>().material = rule.GetMaterial("material_water", _resourceProvider);
            gameObject.AddComponent<MeshFilter>().mesh = mesh;
        }

        #endregion

        #region Background layer

        protected void BuildBackground(Rule rule, MeshData meshData, MeshRegion meshRegion)
        {
            if (meshRegion.Mesh == null) return;
            var gradient = rule.GetBackgroundLayerGradient(_resourceProvider);

            float eleNoiseFreq = rule.GetBackgroundLayerEleNoiseFreq();
            float colorNoiseFreq = rule.GetBackgroundLayerColorNoiseFreq();
            foreach (var triangle in meshRegion.Mesh.Triangles)
                AddTriangle(rule, meshData, triangle, gradient, eleNoiseFreq, colorNoiseFreq);
        }

        #endregion

        #region Car roads layer

        protected void BuildCarRoads(Rule rule, MeshData meshData, MeshRegion meshRegion)
        {
            float eleNoiseFreq = rule.GetCarLayerEleNoiseFreq();
            float colorNoiseFreq = rule.GetCarLayerColorNoiseFreq();

            if (meshRegion.Mesh == null) return;
            var gradient = rule.GetCarLayerGradient(_resourceProvider);

            foreach (var triangle in meshRegion.Mesh.Triangles)
                AddTriangle(rule, meshData, triangle, gradient, eleNoiseFreq, colorNoiseFreq, 0);
        }

        #endregion

        #region Pedestrian roads layer

        protected void BuildPedestrianLayers(Rule rule, MeshData meshData, MeshRegion meshRegion)
        {
            if (meshRegion.Mesh == null) return;
            var gradient = rule.GetPedestrianLayerGradient(_resourceProvider);
            float eleNoiseFreq = rule.GetPedestrianLayerEleNoiseFreq();
            float colorNoiseFreq = rule.GetPedestrianLayerColorNoiseFreq();
            foreach (var triangle in meshRegion.Mesh.Triangles)
                AddTriangle(rule, meshData, triangle, gradient, eleNoiseFreq, colorNoiseFreq);
        }

        #endregion

        #region Surface layer

        protected void BuildSurface(Rule rule, MeshData meshData, MeshRegion meshRegion)
        {
            if (meshRegion.Mesh == null) return;
            float colorNoiseFreq = rule.GetColorNoiseFreq();
            float eleNoiseFreq = rule.GetEleNoiseFreq();
            var gradient = _resourceProvider.GetGradient(meshRegion.GradientKey);

            if (meshRegion.ModifyMeshAction != null)
                meshRegion.ModifyMeshAction(meshRegion.Mesh);

            foreach (var triangle in meshRegion.Mesh.Triangles)
                AddTriangle(rule, meshData, triangle, gradient, eleNoiseFreq, colorNoiseFreq);
        }

        #endregion

        #region Layer builder helper methods

        protected void AddTriangle(Rule rule, MeshData meshData, Triangle triangle, GradientWrapper gradient,
            float eleNoiseFreq, float colorNoiseFreq, float yOffset = 0)
        {
            var useEleNoise = Math.Abs(eleNoiseFreq) > 0.0001;

            var v0 = GetVertex(triangle.GetVertex(0), eleNoiseFreq, useEleNoise, yOffset);
            var v1 = GetVertex(triangle.GetVertex(1), eleNoiseFreq, useEleNoise, yOffset);
            var v2 = GetVertex(triangle.GetVertex(2), eleNoiseFreq, useEleNoise, yOffset);

            var triangleColor = GradientUtils.GetColor(gradient, new Vector3(v0.X, v0.Elevation, v0.Y), colorNoiseFreq);

            meshData.AddTriangle(v0, v1, v2, triangleColor);
        }

        private MapPoint GetVertex(Vertex v, float eleNoiseFreq, bool useEleNoise, float yOffset)
        {
            var p2 = new MapPoint((float)v.X, (float)v.Y);
            var useEleNoise2 = v.Type == VertexType.FreeVertex && useEleNoise;
            var ele2 = _elevationProvider.GetElevation(p2);
            if (useEleNoise2)
                ele2 += Noise.Perlin3D(new Vector3(p2.X, ele2, p2.Y), eleNoiseFreq);
            return new MapPoint(p2.X, p2.Y, ele2 + yOffset);
        }

        #endregion

        private void BuildObject(IGameObject goWrapper, Rule rule, MeshData meshData,
            Vector3[] vertices, int[] triangles, Color[] colors)
        {
            var gameObject = goWrapper.GetComponent<GameObject>();

            var mesh = new Mesh();
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.colors = colors;
            mesh.RecalculateNormals();

            gameObject.AddComponent<MeshRenderer>().material = rule.GetMaterial("material_background", _resourceProvider);
            gameObject.AddComponent<MeshFilter>().mesh = mesh;
            gameObject.AddComponent<MeshCollider>();

            gameObject.AddComponent<MeshIndexBehaviour>().Index = meshData.Index;
        }

        public void Configure(IConfigSection configSection)
        {
            _maxCellSize = configSection.GetFloat("cell.size", 100);
            var maxArea = configSection.GetFloat("maxArea", 10);

            _meshCellBuilder.SetMaxArea(maxArea);
        }
    }
}