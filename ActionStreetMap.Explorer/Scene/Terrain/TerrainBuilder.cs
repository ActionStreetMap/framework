using System;
using System.Collections.Generic;
using ActionStreetMap.Core;
using ActionStreetMap.Core.Geometry;
using ActionStreetMap.Core.Geometry.Triangle;
using ActionStreetMap.Core.Geometry.Triangle.Geometry;
using ActionStreetMap.Core.Geometry.Triangle.Topology;
using ActionStreetMap.Core.Geometry.Utils;
using ActionStreetMap.Core.MapCss.Domain;
using ActionStreetMap.Core.Scene.Terrain;
using ActionStreetMap.Core.Tiling;
using ActionStreetMap.Core.Tiling.Models;
using ActionStreetMap.Core.Unity;
using ActionStreetMap.Core.Utils;
using ActionStreetMap.Explorer.Helpers;
using ActionStreetMap.Explorer.Infrastructure;
using ActionStreetMap.Explorer.Interactions;
using ActionStreetMap.Explorer.Scene.Indices;
using ActionStreetMap.Explorer.Utils;
using ActionStreetMap.Infrastructure.Config;
using ActionStreetMap.Infrastructure.Dependencies;
using ActionStreetMap.Infrastructure.Diagnostic;
using ActionStreetMap.Infrastructure.Reactive;
using ActionStreetMap.Infrastructure.Utilities;
using ActionStreetMap.Unity.Wrappers;
using UnityEngine;
using Canvas = ActionStreetMap.Core.Tiling.Models.Canvas;
using Mesh = UnityEngine.Mesh;
using RenderMode = ActionStreetMap.Core.RenderMode;

namespace ActionStreetMap.Explorer.Scene.Terrain
{
    /// <summary> Defines terrain builder API. </summary>
    public interface ITerrainBuilder
    {
        /// <summary> Builds terrain from tile. </summary>
        /// <param name="tile">Tile.</param>
        /// <param name="rule">Rule.</param>
        /// <returns>Game object.</returns>
        IGameObject Build(Tile tile, Rule rule);
    }

    /// <summary> Default implementation of <see cref="ITerrainBuilder"/>. </summary>
    internal class TerrainBuilder : ITerrainBuilder, IConfigurable
    {
        private const string LogTag = "mesh.terrain";

        private readonly BehaviourProvider _behaviourProvider;
        private readonly IElevationProvider _elevationProvider;
        private readonly IResourceProvider _resourceProvider;
        private readonly IGameObjectFactory _gameObjectFactory;
        private readonly IObjectPool _objectPool;
        private readonly MeshCellBuilder _meshCellBuilder;

        [Dependency]
        public ITrace Trace { get; set; }

        private float _maxCellSize = 100;

        /// <summary> Creates instance of <see cref="TerrainBuilder"/>. </summary>
        [Dependency]
        public TerrainBuilder(BehaviourProvider behaviourProvider,
                              IElevationProvider elevationProvider,
                              IResourceProvider resourceProvider,
                              IGameObjectFactory gameObjectFactory,
                              IObjectPool objectPool)
        {
            _behaviourProvider = behaviourProvider;
            _elevationProvider = elevationProvider;
            _resourceProvider = resourceProvider;
            _gameObjectFactory = gameObjectFactory;
            _objectPool = objectPool;
            _meshCellBuilder = new MeshCellBuilder(_objectPool);
        }

        public IGameObject Build(Tile tile, Rule rule)
        {
            Trace.Debug(LogTag, "Started to build terrain");
            var sw = new System.Diagnostics.Stopwatch();
            sw.Start();

            var renderMode = tile.RenderMode;
            var terrainObject = _gameObjectFactory.CreateNew("terrain", tile.GameObject);

            // NOTE detect grid parameters for scene mode. For overview use 1x1 grid
            var cellRowCount = renderMode == RenderMode.Scene ?
                (int) Math.Ceiling(tile.Height/_maxCellSize) : 1;

            var cellColumnCount = renderMode == RenderMode.Scene ?
                (int) Math.Ceiling(tile.Width/_maxCellSize) : 1;

            var cellHeight = tile.Height/cellRowCount;
            var cellWidth = tile.Width/cellColumnCount;

            Trace.Debug(LogTag, "Building mesh canvas..");
            var meshCanvas = new MeshCanvasBuilder(_objectPool)
                .SetTile(tile)
                .SetScale(MeshCellBuilder.Scale)
                .Build(renderMode);

            Trace.Debug(LogTag, "Building mesh cells..");
            // NOTE keeping this code single threaded dramatically reduces memory presure
            for (int j = 0; j < cellRowCount; j++)
                for (int i = 0; i < cellColumnCount; i++)
                {
                    var tileBottomLeft = tile.Rectangle.BottomLeft;
                    var rectangle = new Rectangle2d(
                        tileBottomLeft.X + i*cellWidth,
                        tileBottomLeft.Y + j*cellHeight,
                        cellWidth,
                        cellHeight);
                    var name = String.Format("cell {0}_{1}", i, j);
                    var cell = _meshCellBuilder.Build(rectangle, meshCanvas);
                    BuildCell(tile.Canvas, rule, terrainObject, cell, rectangle, renderMode, name);
                }
            terrainObject.IsBehaviourAttached = true;
            sw.Stop();
            Trace.Debug(LogTag, "Terrain is build in {0}ms", sw.ElapsedMilliseconds.ToString());         
            return terrainObject;
        }

        private void BuildCell(Canvas canvas, Rule rule, IGameObject terrainObject, MeshCell cell, 
            Rectangle2d cellRect, RenderMode renderMode, string name)
        {
            var cellGameObject = _gameObjectFactory.CreateNew(name, terrainObject);

            var rect = new Rectangle2d(cellRect.Left, cellRect.Bottom, cellRect.Width, cellRect.Height);

            var meshData = new TerrainMeshData(_objectPool);       
            meshData.GameObject = cellGameObject;
            meshData.Index = renderMode == RenderMode.Scene ?
                new TerrainMeshIndex(16, 16, rect, meshData.Triangles) :
                (IMeshIndex)DummyMeshIndex.Default;

            // build canvas
            BuildBackground(rule, meshData, cell.Background, renderMode);
            // build extra layers
            BuildWater(rule, meshData, cell.Water, renderMode);
            BuildCarRoads(rule, meshData, cell.CarRoads, renderMode);
            BuildPedestrianLayers(rule, meshData, cell.WalkRoads, renderMode);
            foreach (var surfaceRegion in cell.Surfaces)
                BuildSurface(rule, meshData, surfaceRegion, renderMode);

            Trace.Debug(LogTag, "Total triangles: {0}", meshData.Triangles.Count.ToString());

            meshData.Index.Build();

            BuildObject(cellGameObject, canvas, rule, meshData);
        }

        #region Water layer

        private void BuildWater(Rule rule, TerrainMeshData meshData, MeshRegion meshRegion, RenderMode renderMode)
        {
            if (meshRegion.Mesh == null) return;

            float colorNoiseFreq = renderMode == RenderMode.Scene ? rule.GetWaterLayerColorNoiseFreq() : 0;
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

            var elevationOffset = waterBottomLevelOffset - waterSurfaceLevelOffset;
            var surfaceOffset = renderMode == RenderMode.Scene ? -waterBottomLevelOffset : 0;

            // NOTE: substitute gradient in overview mode
            if (renderMode == RenderMode.Overview)
                bottomGradient = waterSurfaceGradient;

            int count = 0;
            foreach (var triangle in meshRegion.Mesh.Triangles)
            {
                // bottom surface
                AddTriangle(rule, meshData, triangle, bottomGradient, eleNoiseFreq, colorNoiseFreq, surfaceOffset);

                // NOTE: build offset shape only in case of Scene mode
                if (renderMode == RenderMode.Overview)
                    continue;

                var meshTriangle = meshTriangles[meshTriangles.Count - 1];

                var p0 = meshTriangle.Vertex0;
                var p1 = meshTriangle.Vertex1;
                var p2 = meshTriangle.Vertex2;

                // reuse just added vertices
                waterVertices.Add(new Vector3(p0.x, p0.y + elevationOffset, p0.z));
                waterVertices.Add(new Vector3(p1.x, p1.y + elevationOffset, p1.z));
                waterVertices.Add(new Vector3(p2.x, p2.y + elevationOffset, p2.z));

                var color = GradientUtils.GetColor(waterSurfaceGradient, waterVertices[count], colorNoiseFreq);
                waterColors.Add(color);
                waterColors.Add(color);
                waterColors.Add(color);

                waterTriangles.Add(count);
                waterTriangles.Add(count + 2);
                waterTriangles.Add(count + 1);
                count += 3;
            }

            // finalizing offset shape
            if (renderMode == RenderMode.Scene)
            {
                BuildOffsetShape(rule, meshData, meshRegion, rule.GetBackgroundLayerGradient(_resourceProvider),
                    colorNoiseFreq, waterBottomLevelOffset);

                var vs = waterVertices.ToArray();
                var ts = waterTriangles.ToArray();
                var cs = waterColors.ToArray();
                Observable.Start(() => BuildWaterObject(rule, meshData, vs, ts, cs), Scheduler.MainThread);
            }
        }

        protected void BuildOffsetShape(Rule rule, TerrainMeshData meshData, MeshRegion region, GradientWrapper gradient,
            float colorNoiseFreq, float deepLevel)
        {
            const float divideStep = 1f;
            const float errorTopFix = 0.02f;
            const float errorBottomFix = 0.1f;

            var pointList = _objectPool.NewList<Vector2d>(64);
            foreach (var contour in region.Contours)
            {
                var length = contour.Count;
                for (int i = 0; i < length; i++)
                {
                    var v2DIndex = i == (length - 1) ? 0 : i + 1;
                    var start = new Vector2d((float)contour[i].X, (float)contour[i].Y);
                    var end = new Vector2d((float)contour[v2DIndex].X, (float)contour[v2DIndex].Y);

                    GeometryUtils.DivideLine(start, end, pointList, divideStep);

                    for (int k = 1; k < pointList.Count; k++)
                    {
                        var p1 = pointList[k - 1];
                        var p2 = pointList[k];

                        // vertices
                        var ele1 = _elevationProvider.GetElevation(p1);
                        var ele2 = _elevationProvider.GetElevation(p2);

                        var firstColor = GradientUtils.GetColor(gradient,
                            new Vector3((float)p1.X, ele1 + errorTopFix, (float)p1.Y), colorNoiseFreq);

                        var secondColor = GradientUtils.GetColor(gradient,
                            new Vector3((float)p2.X, ele1 - deepLevel - errorBottomFix, (float)p2.Y), colorNoiseFreq);

                        meshData.AddTriangle(
                            new Vector3((float)p1.X, ele1 + errorTopFix, (float)p1.Y),
                            new Vector3((float)p2.X, ele2 - deepLevel - errorBottomFix, (float)p2.Y),
                            new Vector3((float)p2.X, ele2 + errorTopFix, (float)p2.Y),
                            firstColor);

                        meshData.AddTriangle(
                            new Vector3((float)p1.X, ele1 - deepLevel - errorBottomFix, (float)p1.Y),
                            new Vector3((float)p2.X, ele2 - deepLevel - errorBottomFix, (float)p2.Y),
                            new Vector3((float)p1.X, ele1 + errorTopFix, (float)p1.Y),
                            secondColor);
                    }

                    pointList.Clear();
                }
            }
            _objectPool.StoreList(pointList);
        }


        private void BuildWaterObject(Rule rule, TerrainMeshData meshData, Vector3[] vertices, int[] triangles, Color[] colors)
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

        private void BuildBackground(Rule rule, TerrainMeshData meshData, MeshRegion meshRegion, RenderMode renderMode)
        {
            if (meshRegion.Mesh == null) return;
            var gradient = rule.GetBackgroundLayerGradient(_resourceProvider);

            float eleNoiseFreq = rule.GetBackgroundLayerEleNoiseFreq();
            float colorNoiseFreq = renderMode == RenderMode.Scene ? rule.GetBackgroundLayerColorNoiseFreq() : 0;
            foreach (var triangle in meshRegion.Mesh.Triangles)
                AddTriangle(rule, meshData, triangle, gradient, eleNoiseFreq, colorNoiseFreq);
            
            meshRegion.Dispose();
        }

        #endregion

        #region Car roads layer

        private void BuildCarRoads(Rule rule, TerrainMeshData meshData, MeshRegion meshRegion, RenderMode renderMode)
        {
            float eleNoiseFreq = rule.GetCarLayerEleNoiseFreq();
            float colorNoiseFreq = renderMode == RenderMode.Scene ? rule.GetCarLayerColorNoiseFreq(): 0;

            if (meshRegion.Mesh == null) return;
            var gradient = rule.GetCarLayerGradient(_resourceProvider);

            foreach (var triangle in meshRegion.Mesh.Triangles)
                AddTriangle(rule, meshData, triangle, gradient, eleNoiseFreq, colorNoiseFreq, 0);

            meshRegion.Dispose();
        }

        #endregion

        #region Pedestrian roads layer

        private void BuildPedestrianLayers(Rule rule, TerrainMeshData meshData, MeshRegion meshRegion, RenderMode renderMode)
        {
            if (meshRegion.Mesh == null) return;
            var gradient = rule.GetPedestrianLayerGradient(_resourceProvider);
            float eleNoiseFreq = rule.GetPedestrianLayerEleNoiseFreq();
            float colorNoiseFreq = renderMode == RenderMode.Scene ? rule.GetPedestrianLayerColorNoiseFreq() : 0;
            foreach (var triangle in meshRegion.Mesh.Triangles)
                AddTriangle(rule, meshData, triangle, gradient, eleNoiseFreq, colorNoiseFreq);

            meshRegion.Dispose();
        }

        #endregion

        #region Surface layer

        private void BuildSurface(Rule rule, TerrainMeshData meshData, MeshRegion meshRegion, RenderMode renderMode)
        {
            if (meshRegion.Mesh == null) return;

            float colorNoiseFreq = renderMode == RenderMode.Scene ? meshRegion.ColorNoiseFreq : 0;
            float eleNoiseFreq = renderMode == RenderMode.Scene ? meshRegion.ElevationNoiseFreq: 0;
            var gradient = _resourceProvider.GetGradient(meshRegion.GradientKey);

            if (meshRegion.ModifyMeshAction != null)
                meshRegion.ModifyMeshAction(meshRegion.Mesh);

            foreach (var triangle in meshRegion.Mesh.Triangles)
                AddTriangle(rule, meshData, triangle, gradient, eleNoiseFreq, colorNoiseFreq);

            meshRegion.Dispose();
        }

        #endregion

        #region Layer builder helper methods

        private void AddTriangle(Rule rule, TerrainMeshData meshData, Triangle triangle, GradientWrapper gradient,
            float eleNoiseFreq, float colorNoiseFreq, float yOffset = 0)
        {
            var useEleNoise = Math.Abs(eleNoiseFreq) > 0.0001;

            var v0 = GetVertex(triangle.GetVertex(0), eleNoiseFreq, useEleNoise, yOffset);
            var v1 = GetVertex(triangle.GetVertex(1), eleNoiseFreq, useEleNoise, yOffset);
            var v2 = GetVertex(triangle.GetVertex(2), eleNoiseFreq, useEleNoise, yOffset);

            var triangleColor = GradientUtils.GetColor(gradient, v0, colorNoiseFreq);

            meshData.AddTriangle(v0, v1, v2, triangleColor);
        }

        private Vector3 GetVertex(Vertex v, float eleNoiseFreq, bool useEleNoise, float yOffset)
        {
            var point = new Vector2d(
                Math.Round(v.X, MathUtils.RoundDigitCount),
                Math.Round(v.Y, MathUtils.RoundDigitCount));

            var useEleNoise2 = v.Type == VertexType.FreeVertex && useEleNoise;
            var ele = _elevationProvider.GetElevation(point);
            if (useEleNoise2)
                ele += Noise.Perlin3D(new Vector3((float) point.X, ele, (float) point.Y), eleNoiseFreq);
            return new Vector3((float) point.X, ele + yOffset, (float) point.Y);
        }

        #endregion

        /// <summary> Builds real game object. </summary>
        protected virtual void BuildObject(IGameObject cellGameObject, Canvas canvas, Rule rule, 
            TerrainMeshData meshData)
        {
            Vector3[] vertices;
            int[] triangles;
            Color[] colors;
            meshData.GenerateObjectData(out vertices, out triangles, out colors);

            Observable.Start(() => BuildObject(cellGameObject, canvas, rule,
                meshData, vertices, triangles, colors), Scheduler.MainThread);
        }

        private void BuildObject(IGameObject goWrapper, Canvas canvas, Rule rule, TerrainMeshData meshData,
            Vector3[] vertices, int[] triangles, Color[] colors)
        {
            var gameObject = goWrapper.GetComponent<GameObject>();

            var mesh = new Mesh();
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.colors = colors;
            mesh.RecalculateNormals();

            gameObject.AddComponent<MeshRenderer>().material = rule
                .GetMaterial("material_background", _resourceProvider);
            gameObject.AddComponent<MeshFilter>().mesh = mesh;
            gameObject.AddComponent<MeshCollider>();

            gameObject.AddComponent<MeshIndexBehaviour>().Index = meshData.Index;
            meshData.Dispose();

            var behaviourTypes = rule.GetModelBehaviours(_behaviourProvider);
            foreach (var behaviourType in behaviourTypes)
            {
                var behaviour = gameObject.AddComponent(behaviourType) as IModelBehaviour;
                if (behaviour != null)
                    behaviour.Apply(goWrapper, canvas);
            }
        }

        /// <inheritdoc />
        public void Configure(IConfigSection configSection)
        {
            _maxCellSize = configSection.GetFloat("cell_size", 100);
            var maxArea = configSection.GetFloat("tri_area", 6);

            _meshCellBuilder.SetMaxArea(maxArea);
        }
    }
}