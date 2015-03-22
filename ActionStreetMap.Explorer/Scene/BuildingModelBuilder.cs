using System.Collections.Generic;
using System.Linq;
using ActionStreetMap.Core;
using ActionStreetMap.Core.MapCss.Domain;
using ActionStreetMap.Core.Scene.Buildings;
using ActionStreetMap.Core.Tiling.Models;
using ActionStreetMap.Core.Unity;
using ActionStreetMap.Explorer.Geometry.Utils;
using ActionStreetMap.Explorer.Helpers;
using ActionStreetMap.Explorer.Scene.Buildings;
using ActionStreetMap.Explorer.Scene.Buildings.Facades;
using ActionStreetMap.Explorer.Scene.Buildings.Roofs;
using ActionStreetMap.Infrastructure.Dependencies;
using ActionStreetMap.Maps.Helpers;

namespace ActionStreetMap.Explorer.Scene
{
    /// <summary> Provides logic to build buildings. </summary>
    public class BuildingModelBuilder : ModelBuilder
    {
        private readonly IBuildingBuilder _builder;
        private readonly IElevationProvider _elevationProvider;
        private readonly IEnumerable<IFacadeBuilder> _facadeBuilders;
        private readonly IEnumerable<IRoofBuilder> _roofBuilders;

        /// <inheritdoc />
        public override string Name { get { return "building"; } }

        /// <summary> Creates instance of <see cref="BuildingModelBuilder"/>. </summary>
        [Dependency]
        public BuildingModelBuilder(IBuildingBuilder builder, IElevationProvider elevationProvider,
            IEnumerable<IFacadeBuilder> facadeBuilders, IEnumerable<IRoofBuilder> roofBuilders)
        {
            _builder = builder;
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

            // NOTE observed that min_height should be subracted from height for building:part
            // TODO this should be done in mapcss, but stylesheet doesn't support multiply eval operations
            // on the same tag

            var height = rule.GetHeight();
            if (rule.IsPart())
                height -= minHeight;

            // TODO should we save this object in WorldManager?
            var building = new Building
            {
                Id = model.Id,
                Address = AddressExtractor.Extract(model.Tags),
                GameObject = gameObjectWrapper,
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
                Elevation = elevation, // we set equal elevation for every point
                Footprint = points,
            };

            var facadeBuilder = _facadeBuilders.Single(f => f.Name == building.FacadeType);
            var roofBuilder = _roofBuilders.Single(f => f.Name == building.RoofType);

            _builder.Build(building, facadeBuilder, roofBuilder);

            tile.Registry.RegisterGlobal(building.Id);

            return gameObjectWrapper;
        }
    }
}