using System;
using System.Collections.Generic;
using ActionStreetMap.Core;
using ActionStreetMap.Core.Geometry.Triangle;
using ActionStreetMap.Core.Geometry.Triangle.Geometry;
using ActionStreetMap.Core.Geometry.Triangle.Topology;
using ActionStreetMap.Core.MapCss.Domain;
using ActionStreetMap.Core.Scene.Terrain;
using ActionStreetMap.Core.Tiling.Models;
using ActionStreetMap.Core.Unity;
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
        private const float RoadDeepLevel = 0.2f;

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
                        BuildCell(rule, terrainObject, rectangle, cell, name);
                    }));
                }

            tasks.WhenAll().Wait();

            sw.Stop();
            Trace.Debug(LogTag, "Terrain is build in {0}ms", sw.ElapsedMilliseconds);

            return terrainObject;
        }

        private void BuildCell(Rule rule, IGameObject terrainObject, Rectangle cellRect, MeshCell cell, string name)
        {
            var rect = new MapRectangle((float)cellRect.Left, (float)cellRect.Bottom, 
                (float)cellRect.Width, (float)cellRect.Height);

            var cellGameObject = _gameObjectFactory.CreateNew(name, terrainObject);

            // TODO detect optimal values
            var meshData = _objectPool.CreateMeshData(1024, 4096, 1024);
            meshData.GameObject = cellGameObject;

            var context = new MeshContext
            {
                Rule = rule,
                Data = meshData,
                Rectangle = rect,
            };

            // build canvas
            BuildBackground(context, cell.Background);
            // build extra layers
            BuildWater(context, cell.Water);
            BuildCarRoads(context, cell.CarRoads);
            BuildPedestrianLayers(context, cell.WalkRoads);
            foreach (var surfaceRegion in cell.Surfaces)
                BuildSurface(context, surfaceRegion);

            Trace.Debug(LogTag, "Total triangles: {0}", context.Data.Triangles.Count);

            // copy on non-UI thread
            var vertices = context.Data.Vertices.ToArray();
            var triangles = context.Data.Triangles.ToArray();
            var colors = context.Data.Colors.ToArray();
            _objectPool.RecycleMeshData(context.Data);

            Scheduler.MainThread.Schedule(() => BuildObject(cellGameObject, rule,
                vertices, triangles, colors));
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

            var vertices = context.Data.Vertices;

            var bottomGradient = context.Rule.GetBackgroundLayerGradient(_resourceProvider);
            var waterSurfaceGradient = context.Rule.GetWaterLayerGradient(_resourceProvider);
            int count = 0;
            foreach (var triangle in meshRegion.Mesh.Triangles)
            {
                // bottom
                AddTriangle(context, triangle, bottomGradient, eleNoiseFreq, colorNoiseFreq, -WaterBottomLevelOffset);

                var p0 = vertices[vertices.Count - 3];
                var p1 = vertices[vertices.Count - 2];
                var p2 = vertices[vertices.Count - 1];

                // reuse just added vertices
                waterVertices.Add(new Vector3(p0.x, p0.y + WaterBottomLevelOffset - WaterSurfaceLevelOffset, p0.z));
                waterVertices.Add(new Vector3(p1.x, p1.y + WaterBottomLevelOffset - WaterSurfaceLevelOffset, p1.z));
                waterVertices.Add(new Vector3(p2.x, p2.y + WaterBottomLevelOffset - WaterSurfaceLevelOffset, p2.z));

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
                AddTriangle(context, triangle, gradient, eleNoiseFreq, colorNoiseFreq, -RoadDeepLevel);
            BuildOffsetShape(context, meshRegion, gradient, RoadDeepLevel);
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
            var vertices = context.Data.Vertices;
            var triangles = context.Data.Triangles;
            var colors = context.Data.Colors;

            var v0 = triangle.GetVertex(0);
            var p0 = new MapPoint((float)v0.X, (float)v0.Y);
            var ele0 = _elevationProvider.GetElevation(p0);
            if (v0.Type == VertexType.FreeVertex && eleNoiseFreq != 0f)
                ele0 += Noise.Perlin3D(new Vector3(p0.X, 0, p0.Y), eleNoiseFreq);
            vertices.Add(new Vector3(p0.X, ele0 + yOffset, p0.Y));

            var v1 = triangle.GetVertex(1);
            var p1 = new MapPoint((float)v1.X, (float)v1.Y);
            var ele1 = _elevationProvider.GetElevation(p1);
            if (v1.Type == VertexType.FreeVertex && eleNoiseFreq != 0f)
                ele1 += Noise.Perlin3D(new Vector3(p1.X, 0, p1.Y), eleNoiseFreq);
            vertices.Add(new Vector3(p1.X, ele1 + yOffset, p1.Y));

            var v2 = triangle.GetVertex(2);
            var p2 = new MapPoint((float)v2.X, (float)v2.Y);
            var ele2 = _elevationProvider.GetElevation(p2);
            if (v2.Type == VertexType.FreeVertex && eleNoiseFreq != 0f)
                ele2 += Noise.Perlin3D(new Vector3(p2.X, 0, p2.Y), eleNoiseFreq);
            vertices.Add(new Vector3(p2.X, ele2 + yOffset, p2.Y));

            var index = vertices.Count;
            triangles.Add(--index);
            triangles.Add(--index);
            triangles.Add(--index);

            var triangleColor = GradientUtils.GetColor(gradient, new Vector3((float)v0.X, ele0, (float)v0.Y), colorNoiseFreq);

            colors.Add(triangleColor);
            colors.Add(triangleColor);
            colors.Add(triangleColor);
        }

        protected void BuildOffsetShape(MeshContext context, MeshRegion region, GradientWrapper gradient, float deepLevel)
        {
            const float colorNoiseFreq = 0.2f;
            const float divideStep = 1f;
            const float errorTopFix = 0.02f;
            const float errorBottomFix = 0.1f;

            var vertices = context.Data.Vertices;
            var triangles = context.Data.Triangles;
            var colors = context.Data.Colors;
            var pointList = _objectPool.NewList<MapPoint>(64);
            foreach (var contour in region.Contours)
            {
                var length = contour.Count;
                for (int i = 0; i < length; i++)
                {
                    var v2DIndex = i == (length - 1) ? 0 : i + 1;
                    var start = new MapPoint((float)contour[i].X, (float)contour[i].Y);
                    var end = new MapPoint((float)contour[v2DIndex].X, (float)contour[v2DIndex].Y);

                    LineUtils.DivideLine(_elevationProvider, start, end, pointList, divideStep);

                    for (int k = 1; k < pointList.Count; k++)
                    {
                        var p1 = pointList[k - 1];
                        var p2 = pointList[k];

                        // vertices
                        var ele1 = _elevationProvider.GetElevation(p1);
                        var ele2 = _elevationProvider.GetElevation(p2);
                        vertices.Add(new Vector3(p1.X, ele1 + errorTopFix, p1.Y));
                        vertices.Add(new Vector3(p2.X, ele2 + errorTopFix, p2.Y));
                        vertices.Add(new Vector3(p2.X, ele2 - deepLevel - errorBottomFix, p2.Y));
                        vertices.Add(new Vector3(p1.X, ele1 - deepLevel - errorBottomFix, p1.Y));

                        // colors
                        var firstColor = GradientUtils.GetColor(gradient, new Vector3(p1.X, 0, p1.Y), colorNoiseFreq);
                        var secondColor = GradientUtils.GetColor(gradient, new Vector3(p2.X, 0, p2.Y), colorNoiseFreq);

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
            _objectPool.StoreList(pointList);
        }

        #endregion

        private void BuildObject(IGameObject goWrapper, Rule rule,
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
        }

        public void Configure(IConfigSection configSection)
        {
            _maxCellSize = configSection.GetFloat("cell.size", 100);
            var maxArea = configSection.GetFloat("maxArea", 10);

            _meshCellBuilder.SetMaxArea(maxArea);
        }
    }
}