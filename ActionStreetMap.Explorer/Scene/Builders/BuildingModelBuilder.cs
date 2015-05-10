using System.Collections.Generic;
using System.Linq;
using ActionStreetMap.Core;
using ActionStreetMap.Core.MapCss.Domain;
using ActionStreetMap.Core.Scene;
using ActionStreetMap.Core.Tiling.Models;
using ActionStreetMap.Core.Unity;
using ActionStreetMap.Explorer.Geometry.Utils;
using ActionStreetMap.Explorer.Helpers;
using ActionStreetMap.Explorer.Scene.Facades;
using ActionStreetMap.Explorer.Scene.Roofs;
using ActionStreetMap.Infrastructure.Dependencies;

namespace ActionStreetMap.Explorer.Scene.Builders
{
    /// <summary> Provides logic to build buildings. </summary>
    public class BuildingModelBuilder : ModelBuilder
    {
        private readonly IElevationProvider _elevationProvider;
        private readonly IEnumerable<IFacadeBuilder> _facadeBuilders;
        private readonly IEnumerable<IRoofBuilder> _roofBuilders;

        /// <inheritdoc />
        public override string Name { get { return "building"; } }

        /// <summary> Creates instance of <see cref="BuildingModelBuilder"/>. </summary>
        [Dependency]
        public BuildingModelBuilder(IElevationProvider elevationProvider,
                                    IEnumerable<IFacadeBuilder> facadeBuilders, 
                                    IEnumerable<IRoofBuilder> roofBuilders)
        {
            _elevationProvider = elevationProvider;

            _facadeBuilders = facadeBuilders.ToArray();
            _roofBuilders = roofBuilders.ToArray();
        }

        /// <inheritdoc />
        public override IGameObject BuildArea(Tile tile, Rule rule, Area area)
        {
            base.BuildArea(tile, rule, area);
            return BuildBuilding(tile, rule, area, area.Points);
        }

        private IGameObject BuildBuilding(Tile tile, Rule rule, Model model, List<GeoCoordinate> footPrint)
        {
            var points = ObjectPool.NewList<MapPoint>();
            
            //var simplified = ObjectPool.NewList<MapPoint>();

            PointUtils.GetClockwisePolygonPoints(_elevationProvider, tile.RelativeNullPoint, footPrint, points);
            var minHeight = BuildingRuleExtensions.GetMinHeight(rule);

            // NOTE simplification is important to build hipped/gabled roofs
            //PolygonUtils.Simplify(points, simplified);

            var elevation = points.Average(p => p.Elevation);

            if (tile.Registry.Contains(model.Id))
                return null;

            var gameObject = BuildGameObject(tile, rule, model, points, elevation, minHeight);

            ObjectPool.StoreList(points);
            //ObjectPool.StoreList(simplified);

            return gameObject;
        }

        private IGameObject BuildGameObject(Tile tile, Rule rule, Model model, List<MapPoint> points,
            float elevation, float minHeight)
        {
            var gameObjectWrapper = GameObjectFactory
                .CreateNew(GetName(model), tile.GameObject);

            var isPart = rule.IsPart();
            var height = rule.GetHeight();

            // NOTE: this is not clear
            //if (isPart)
            height -= minHeight;

            // TODO should we save this object in WorldManager?
            var building = new Building
            {
                Id = model.Id,
                GameObject = gameObjectWrapper,
                IsPart = isPart,
                Height = height,
                Levels = rule.GetLevels(),
                MinHeight = minHeight,
                Type = rule.GetFacadeBuilder(),
                FacadeType = rule.GetFacadeBuilder(),
                FacadeColor = rule.GetFacadeColor(),
                FacadeMaterial = rule.GetFacadeMaterial(),
                RoofType = rule.GetRoofBuilder(),
                RoofColor = rule.GetRoofColor(),
                RoofMaterial = rule.GetRoofMaterial(),
                RoofHeight = rule.GetRoofHeight(),
                Elevation = elevation,
                Footprint = points,
            };

            tile.Registry.RegisterGlobal(building.Id);

            // facade
            var facadeBuilder = _facadeBuilders.Single(f => f.Name == building.FacadeType);
            var facadeMeshData = facadeBuilder.Build(building);
            facadeMeshData.GameObject = GameObjectFactory.CreateNew("facade");
            facadeMeshData.MaterialKey = building.FacadeMaterial;
            BuildObject(gameObjectWrapper, facadeMeshData);

            // roof
            var roofBuilder = _roofBuilders.Single(f => f.Name == building.RoofType);
            var roofMeshData = roofBuilder.Build(building);
            roofMeshData.GameObject = GameObjectFactory.CreateNew("roof");
            roofMeshData.MaterialKey = building.RoofMaterial;
            BuildObject(gameObjectWrapper, roofMeshData);

            return gameObjectWrapper;
        }
    }
}