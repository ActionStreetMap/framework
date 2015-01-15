using System;
using System.Collections.Generic;
using System.Linq;
using ActionStreetMap.Core.Scene.World.Roads;
using ActionStreetMap.Core.Unity;
using ActionStreetMap.Infrastructure.Dependencies;
using ActionStreetMap.Infrastructure.Diagnostic;
using ActionStreetMap.Infrastructure.Utilities;
using ActionStreetMap.Models.Details;
using ActionStreetMap.Models.Roads;
using ActionStreetMap.Models.Utils;
using UnityEngine;

namespace ActionStreetMap.Models.Terrain
{
    /// <summary>
    ///     Defines terrain builder.
    /// </summary>
    public interface ITerrainBuilder
    {
        /// <summary>
        ///     Builds terrain.
        /// </summary>
        /// <param name="parent">Parent game object, usually Tile.</param>
        /// <param name="settings">Terrain settings.</param>
        /// <returns>Terrain game object.</returns>
        IGameObject Build(IGameObject parent, TerrainSettings settings);

        /// <summary>
        ///     Adds road element to terrain.
        /// </summary>
        /// <param name="roadElement">Road element</param>
        void AddRoadElement(RoadElement roadElement);

        /// <summary>
        ///     Adds area which should be drawn using different splat index.
        /// </summary>
        /// <param name="areaSettings">Area settings.</param>
        void AddArea(AreaSettings areaSettings);

        /// <summary>
        ///     Adds area which should be adjuested by height. Processed last.
        /// </summary>
        /// <param name="areaSettings">Area settings.</param>
        void AddElevation(AreaSettings areaSettings);

        /// <summary>
        ///     Adds tree.
        /// </summary>
        /// <param name="tree">Tree.</param>
        void AddTree(TreeDetail tree);
    }

    /// <summary>
    ///     Creates Unity Terrain object using given settings.
    /// </summary>
    public class TerrainBuilder : ITerrainBuilder
    {
        private readonly IGameObjectFactory _gameObjectFactory;
        private readonly IResourceProvider _resourceProvider;
        private readonly IRoadBuilder _roadBuilder;
        private readonly IObjectPool _objectPool;
        private readonly AreaBuilder _areaBuilder = new AreaBuilder();
        private readonly HeightMapProcessor _heightMapProcessor = new HeightMapProcessor();

        private SplatPrototype[] _splatPrototypes;
        private DetailPrototype[] _detailPrototypes;

        private float[,,] _splatMapBuffer;
        private List<int[,]> _detailListBuffer;

        private readonly IRoadGraphBuilder _roadGraphBuilder;

        private readonly List<AreaSettings> _areas = new List<AreaSettings>();
        private readonly List<AreaSettings> _elevations = new List<AreaSettings>();
        private readonly List<TreeDetail> _trees = new List<TreeDetail>();

        [Dependency]
        public ITrace Trace { get; set; }

        /// <summary>
        ///     Creates TerrainBuilder.
        /// </summary>
        /// <param name="gameObjectFactory">Game object factory.</param>
        /// <param name="resourceProvider">Resource provider.</param>
        /// <param name="roadGraphBuilder">Road graph builder.</param>
        /// <param name="roadBuilder">Road builder.</param>
        /// <param name="objectPool">Object pool.</param>
        [Dependency]
        public TerrainBuilder(IGameObjectFactory gameObjectFactory, IResourceProvider resourceProvider, 
            IRoadGraphBuilder roadGraphBuilder, IRoadBuilder roadBuilder, IObjectPool objectPool)
        {
            _gameObjectFactory = gameObjectFactory;
            _resourceProvider = resourceProvider;
            _roadBuilder = roadBuilder;
            _objectPool = objectPool;
            _roadGraphBuilder = roadGraphBuilder;
        }

        #region ITerrainBuilder implementation

        /// <inheritdoc />
        public IGameObject Build(IGameObject parent, TerrainSettings settings)
        {
            ProcessTerrainObjects(settings);

            var htmap = settings.Tile.HeightMap.Data;

            // normalize
            var resolution = settings.Tile.HeightMap.Resolution;
            var maxElevation = settings.Tile.HeightMap.MaxElevation;
            for (int i = 0; i < resolution; i++)
                for (int j = 0; j < resolution; j++)
                    htmap[i, j] /= maxElevation;

            var size = new Vector3(settings.Tile.Size, settings.Tile.HeightMap.MaxElevation, settings.Tile.Size);
            var layers = settings.SplatParams.Count;

            // NOTE we don't expect buffer size changes after engine is initialized
            if (_splatMapBuffer == null)
                _splatMapBuffer = new float[settings.Resolution, settings.Resolution, layers];

            if (_detailListBuffer == null)
            {
                _detailListBuffer = new List<int[,]>(settings.DetailParams.Count);
                for (int i = 0; i < settings.DetailParams.Count; i++)
                    _detailListBuffer.Add(new int[settings.Resolution, settings.Resolution]);
            }

            // fill alphamap
            var alphaMapElements = CreateElements(settings, _areas,
                settings.Resolution / size.x,
                settings.Resolution / size.z,
                t => t.SplatIndex);

            _areaBuilder.Build(settings, alphaMapElements, _splatMapBuffer, _detailListBuffer);
            
            var gameObject = CreateTerrainGameObject(parent, settings, size, _detailListBuffer);

            ClearBuffers();

            return gameObject;
        }

        /// <inheritdoc />
        public void AddRoadElement(RoadElement roadElement)
        {
            _roadGraphBuilder.Add(roadElement);
        }

        /// <inheritdoc />
        public void AddArea(AreaSettings areaSettings)
        {
            _areas.Add(areaSettings);
        }

        /// <inheritdoc />
        public void AddElevation(AreaSettings areaSettings)
        {
           _elevations.Add(areaSettings);
        }

        /// <inheritdoc />
        public void AddTree(TreeDetail tree)
        {
            _trees.Add(tree);
        }

        #endregion

        private void ProcessTerrainObjects(TerrainSettings settings)
        {
            var heightMap = settings.Tile.HeightMap;
            var roadStyleProvider = settings.RoadStyleProvider;

            var roadGraph = _roadGraphBuilder.Build();
            // build roads
            foreach (var road in roadGraph.Roads)
            {
                var element = road.Elements.First();
                road.GameObject = _gameObjectFactory
                    .CreateNew(String.Format("road [{0}]:{1}", element.Id, element.Type), settings.Tile.GameObject);
                _roadBuilder.Build(heightMap, road, roadStyleProvider.Get(road));               
            }
            // build road junctions
            foreach (var junction in roadGraph.Junctions)
            {
                junction.GameObject = _gameObjectFactory
                    .CreateNew(String.Format("junction: {0}", junction.Center), settings.Tile.GameObject);
                _roadBuilder.Build(heightMap, junction, roadStyleProvider.Get(junction));
            }

            // process elevations
            // NOTE We have to do this in the last order. Otherwise, new height
            // value can affect other models (e.g. water vs road)
            if (_elevations.Any())
            {
                _heightMapProcessor.Recycle(heightMap);

                foreach (var elevationArea in _elevations)
                    _heightMapProcessor.AdjustPolygon(elevationArea.Points, elevationArea.Elevation);
                _heightMapProcessor.Clear();
            }
        }

        /// <summary>
        ///     Creates real game object
        /// </summary>
        protected virtual IGameObject CreateTerrainGameObject(IGameObject parent, TerrainSettings settings,
            Vector3 size, List<int[,]> detailMapList)
        {
            // create TerrainData
            var terrainData = new TerrainData();
            terrainData.heightmapResolution = settings.Tile.HeightMap.Resolution;
            terrainData.SetHeights(0, 0, settings.Tile.HeightMap.Data);
            terrainData.size = size;
            
            // Assume that settings is the same all the time
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

            terrainData.SetAlphamaps(0, 0, _splatMapBuffer);

            SetTrees(terrain, settings, size);

            SetDetails(terrain, settings, detailMapList);

            return new GameObjectWrapper("terrain", gameObject);
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
            terrain.terrainData.treePrototypes = GetTreePrototypes();
            foreach (var treeDetail in _trees)
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
            DetailRenderMode detailMode = DetailRenderMode.GrassBillboard;
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

        private void ClearBuffers()
        {
            // also we set [x,y,0] to 1 in AreaBuilder
            Array.Clear(_splatMapBuffer, 0, _splatMapBuffer.Length);
            _detailListBuffer.ForEach(array => Array.Clear(array, 0, array.Length));

            //Return lists to object pool
            foreach (var area in _areas)
                _objectPool.Store(area.Points);
            foreach (var elevation in _elevations)
                _objectPool.Store(elevation.Points);

            // NOTE do not return road element's points back to store as they will be used in future
            // for unit behavior modeling

            // clear collections to reuse
            _areas.Clear();
            _elevations.Clear();
            _trees.Clear();
        }

        private TerrainElement[] CreateElements(TerrainSettings settings,
            IEnumerable<AreaSettings> areas, float widthRatio, float heightRatio, Func<TerrainElement, float> orderBy)
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