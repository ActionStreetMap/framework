using System;
using System.Collections.Generic;
using System.Linq;
using ActionStreetMap.Core.Scene.Details;
using ActionStreetMap.Core.Unity;
using ActionStreetMap.Infrastructure.Dependencies;
using ActionStreetMap.Infrastructure.Diagnostic;
using ActionStreetMap.Infrastructure.Reactive;
using ActionStreetMap.Infrastructure.Utilities;
using ActionStreetMap.Explorer.Scene.Roads;
using ActionStreetMap.Explorer.Scene.Utils;

using UnityEngine;

namespace ActionStreetMap.Explorer.Scene.Terrain
{
    /// <summary> Defines terrain builder. </summary>
    public interface ITerrainBuilder
    {
        /// <summary> Builds terrain. </summary>
        /// <param name="parent">Parent game object, usually Tile.</param>
        /// <param name="settings">Terrain settings.</param>
        /// <returns>Terrain game object.</returns>
        IGameObject Build(IGameObject parent, TerrainSettings settings);
    }

    /// <summary>  Creates Unity Terrain object using given settings. </summary>
    public class TerrainBuilder : ITerrainBuilder
    {
        private const string LogTag = "terrain";

        private readonly IGameObjectFactory _gameObjectFactory;
        private readonly IResourceProvider _resourceProvider;
        private readonly IRoadBuilder _roadBuilder;
        private readonly IObjectPool _objectPool;
        private readonly SurfaceBuilder _surfaceBuilder;
        private readonly HeightMapProcessor _heightMapProcessor;

        private SplatPrototype[] _splatPrototypes;
        private DetailPrototype[] _detailPrototypes;

        /// <summary>  Gets or sets trace. </summary>
        [Dependency]
        public ITrace Trace { get; set; }

        /// <summary> Creates TerrainBuilder. </summary>
        /// <param name="gameObjectFactory">Game object factory.</param>
        /// <param name="resourceProvider">Resource provider.</param>
        /// <param name="roadBuilder">Road builder.</param>
        /// <param name="objectPool">Object pool.</param>
        /// <param name="heightMapProcessor">Heightmap processor.</param>
        [Dependency]
        public TerrainBuilder(IGameObjectFactory gameObjectFactory, IResourceProvider resourceProvider,
            IRoadBuilder roadBuilder, IObjectPool objectPool, HeightMapProcessor heightMapProcessor)
        {
            _gameObjectFactory = gameObjectFactory;
            _resourceProvider = resourceProvider;
            _roadBuilder = roadBuilder;
            _objectPool = objectPool;

            _heightMapProcessor = heightMapProcessor;
            _surfaceBuilder = new SurfaceBuilder(objectPool);
        }

        #region ITerrainBuilder implementation

        /// <inheritdoc />
        public IGameObject Build(IGameObject parent, TerrainSettings settings)
        {
            Trace.Info(LogTag, "starting build");
            ProcessTerrainObjects(settings);

            var canvas = settings.Tile.Canvas;
            var htmap = settings.Tile.HeightMap.Data;

            // normalize
            var resolution = settings.Tile.HeightMap.Resolution;
            var maxElevation = settings.Tile.HeightMap.MaxElevation;
            htmap.Parallel((start, end) =>
            {
                for (int j = start; j < end; j++)
                    for (int i = 0; i < resolution; i++)
                        htmap[j, i] /= maxElevation;
            });

            var size = new Vector3(settings.Tile.Size, settings.Tile.HeightMap.MaxElevation, settings.Tile.Size);
            var layers = settings.SplatParams.Count;

            settings.Tile.Canvas.SplatMap = _objectPool
                .NewArray<float>(settings.Resolution, settings.Resolution, layers);

            canvas.Details = _objectPool.NewList<int[,]>(settings.DetailParams.Count);
            // this list should be kept untouched
            if (!canvas.Details.Any())
                for (int i = 0; i < settings.DetailParams.Count; i++)
                    canvas.Details.Add(new int[settings.Resolution, settings.Resolution]);
         
            // fill alphamap
            var alphaMapElements = CreateElements(settings, canvas.Areas,
                settings.Resolution / size.x,
                settings.Resolution / size.z,
                t => t.SplatIndex);

            _surfaceBuilder.Build(settings, alphaMapElements, settings.Tile.Canvas.SplatMap, canvas.Details);

            var gameObject = _gameObjectFactory.CreateNew("terrain");
            Trace.Debug(LogTag, "scheduling on main thread..");
            Scheduler.MainThread.Schedule(() =>
            {
                CreateTerrainGameObject(gameObject, parent, settings, size, canvas.Details);
                canvas.Dispose();
                Trace.Info(LogTag, "build finished");
            });
            return gameObject;
        }

        #endregion

        private void ProcessTerrainObjects(TerrainSettings settings)
        {
            var canvas = settings.Tile.Canvas;
            var heightMap = settings.Tile.HeightMap;
            var roadStyleProvider = settings.RoadStyleProvider;

            var roadGraph = canvas.BuildRoadGraph();

            // build roads
            var roadObservable = roadGraph.Roads.ToObservable();
            roadObservable.Subscribe(road =>
                {
                    var element = road.Elements.First();
                    road.GameObject = _gameObjectFactory.CreateNew("road", settings.Tile.GameObject);
                    _roadBuilder.BuildRoad(heightMap, road, roadStyleProvider.Get(road));
                });

            // build road junctions
            var junctionObservable = roadGraph.Junctions.ToObservable();
            junctionObservable.Subscribe(junction =>
            {
                junction.GameObject = _gameObjectFactory.CreateNew("junction", settings.Tile.GameObject);
                _roadBuilder.BuildJunction(heightMap, junction, roadStyleProvider.Get(junction));
            });

            // TODO wait for junctions as well
            roadObservable.Wait();

            // process elevations
            // NOTE We have to do this in the last order. Otherwise, new height
            // value can affect other models (e.g. water vs road)
            if (canvas.Elevations.Any())
            {
                var elevationObservable = canvas.Elevations.ToObservable();
                elevationObservable.Subscribe(elevationArea =>
                    _heightMapProcessor.AdjustPolygon(heightMap, elevationArea.Points, elevationArea.AverageElevation));
                elevationObservable.Wait();
            }
        }

        /// <summary>   Creates real game object. </summary>
        protected virtual void CreateTerrainGameObject(IGameObject terrainWrapper, IGameObject parent, 
            TerrainSettings settings, Vector3 size, List<int[,]> detailMapList)
        {
            // create TerrainData
            var terrainData = new TerrainData();
            terrainData.heightmapResolution = settings.Tile.HeightMap.Resolution;
            terrainData.SetHeights(0, 0, settings.Tile.HeightMap.Data);
            terrainData.size = size;
            
            // assume that settings is the same all the time
            if (_splatPrototypes == null)
                _splatPrototypes = GetSplatPrototypes(settings.SplatParams);

            if (_detailPrototypes == null)
                _detailPrototypes = GetDetailPrototype(settings.DetailParams);

            terrainData.splatPrototypes = _splatPrototypes;
            terrainData.detailPrototypes = _detailPrototypes;

            // create Terrain using terrain data
            var gameObject = UnityEngine.Terrain.CreateTerrainGameObject(terrainData);
            gameObject.transform.parent = parent.GetComponent<GameObject>().transform;
            var terrain = gameObject.GetComponent<UnityEngine.Terrain>();

            terrain.transform.position = new Vector3(settings.CornerPosition.x,
                settings.ZIndex, settings.CornerPosition.y);
            terrain.heightmapPixelError = settings.PixelMapError;
            terrain.basemapDistance = settings.BaseMapDist;

            //disable this for better frame rate
            terrain.castShadows = false;

            terrainData.SetAlphamaps(0, 0, settings.Tile.Canvas.SplatMap);

            SetTrees(terrain, settings, size);

            SetDetails(terrain, settings, detailMapList);

            terrainWrapper.AddComponent(gameObject);
        }

        #region Alpha map splats

        private SplatPrototype[] GetSplatPrototypes(List<List<string>> splatParams)
        {
            var splatPrototypes = new SplatPrototype[splatParams.Count];
            for (int i = 0; i < splatParams.Count; i++)
            {
                var splat = splatParams[i];
                var splatPrototype = new SplatPrototype();
                // TODO remove hardcoded path
                splatPrototype.texture = _resourceProvider.GetTexture2D(@"Textures/Terrain/" + splat[1].Trim());
                splatPrototype.tileSize = new Vector2(int.Parse(splat[2]), int.Parse(splat[3]));

                splatPrototypes[i] = splatPrototype;
            }
            return splatPrototypes;
        }

        #endregion

        #region Trees
        private TreePrototype[] GetTreePrototypes()
        {
            // TODO make this configurable
            var treeProtoTypes = new TreePrototype[3];

            treeProtoTypes[0] = new TreePrototype();
            treeProtoTypes[0].prefab = _resourceProvider.GetGameObject(@"Models/Trees/Alder");

            treeProtoTypes[1] = new TreePrototype();
            treeProtoTypes[1].prefab = _resourceProvider.GetGameObject(@"Models/Trees/Banyan");

            treeProtoTypes[2] = new TreePrototype();
            treeProtoTypes[2].prefab = _resourceProvider.GetGameObject(@"Models/Trees/Mimosa");

            return treeProtoTypes;
        }

        private void SetTrees(UnityEngine.Terrain terrain, TerrainSettings settings, Vector3 size)
        {
            var canvas = settings.Tile.Canvas;
            terrain.terrainData.treePrototypes = GetTreePrototypes();
            foreach (var treeDetail in canvas.Trees)
            {
                var position = new Vector3((treeDetail.Point.X - settings.CornerPosition.x) / size.x, 1,
                    (treeDetail.Point.Y - settings.CornerPosition.y) / size.z);

                // TODO investigate, why we get nodes out of current bbox for trees
                // probably, it's better to filter them in osm level (however, they should be filtered!)
                if (position.x > 1 || position.x < 0 || position.z > 1 || position.z < 0)
                    continue;

                TreeInstance temp = new TreeInstance();
                temp.position = position;
                temp.prototypeIndex = UnityEngine.Random.Range(0, 3);
                temp.widthScale = 1;
                temp.heightScale = 1;
                temp.color = Color.white;
                temp.lightmapColor = Color.white;

                terrain.AddTreeInstance(temp);
            }
        }
        #endregion

        #region Details
        private DetailPrototype[] GetDetailPrototype(List<List<string>> detailParams)
        {
            // TODO make this configurable
            var detailMode = DetailRenderMode.GrassBillboard;
            Color grassHealthyColor = Color.white;
            Color grassDryColor = Color.white;

            var detailProtoTypes = new DetailPrototype[detailParams.Count];
            for (int i = 0; i < detailParams.Count; i++)
            {
                var detail = detailParams[i];
                detailProtoTypes[i] = new DetailPrototype();
                // TODO remove hardcoded path
                detailProtoTypes[i].prototypeTexture = _resourceProvider.GetTexture2D(@"Textures/Terrain/" + detail[1].Trim());
                detailProtoTypes[i].renderMode = detailMode;
                detailProtoTypes[i].healthyColor = grassHealthyColor;
                detailProtoTypes[i].dryColor = grassDryColor;
            }

            return detailProtoTypes;
        }

        private void SetDetails(UnityEngine.Terrain terrain, TerrainSettings settings, List<int[,]> detailMapList)
        {
            // TODO make this configurable
            int detailMapSize = settings.Resolution; //Resolutions of detail (Grass) layers
            int detailObjectDistance = 400;   //The distance at which details will no longer be drawn
            float detailObjectDensity = 1f; //Creates more dense details within patch
            int detailResolutionPerPatch = 128; //The size of detail patch. A higher number may reduce draw calls as details will be batch in larger patches
            float wavingGrassStrength = 0.4f;
            float wavingGrassAmount = 0.2f;
            float wavingGrassSpeed = 0.4f;
            Color wavingGrassTint = Color.white;

            terrain.terrainData.wavingGrassStrength = wavingGrassStrength;
            terrain.terrainData.wavingGrassAmount = wavingGrassAmount;
            terrain.terrainData.wavingGrassSpeed = wavingGrassSpeed;
            terrain.terrainData.wavingGrassTint = wavingGrassTint;
            terrain.detailObjectDensity = detailObjectDensity;
            terrain.detailObjectDistance = detailObjectDistance;
            terrain.terrainData.SetDetailResolution(detailMapSize, detailResolutionPerPatch);

            for (int i = 0; i < detailMapList.Count; i++)
                terrain.terrainData.SetDetailLayer(0, 0, i, detailMapList[i]);
            
        }
        #endregion

        private TerrainElement[] CreateElements(TerrainSettings settings,
            IEnumerable<Surface> areas, float widthRatio, float heightRatio, Func<TerrainElement, float> orderBy)
        {
            return areas.Select(a => new TerrainElement
            {
                ZIndex = a.ZIndex,
                SplatIndex = a.SplatIndex,
                DetailIndex = a.DetailIndex,
                Points = a.Points.Select(p =>
                    ConvertWorldToTerrain(p.X, p.Elevation, p.Y, settings.CornerPosition, widthRatio, heightRatio)).ToArray()
            }).OrderBy(orderBy).ToArray();
        }

        private static Vector3 ConvertWorldToTerrain(float x, float y, float z, Vector2 terrainPosition, float widthRatio, float heightRatio)
        {
            return new Vector3
            {
                // NOTE Coords are inverted here!
                z = (x - terrainPosition.x) * widthRatio,
                x = (z - terrainPosition.y) * heightRatio,
                y = y,
            };
        }
    }
}