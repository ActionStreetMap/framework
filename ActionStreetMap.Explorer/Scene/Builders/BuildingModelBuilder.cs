using System;
using System.Collections.Generic;
using System.Linq;
using ActionStreetMap.Core;
using ActionStreetMap.Core.Elevation;
using ActionStreetMap.Core.MapCss.Domain;
using ActionStreetMap.Core.Tiling.Models;
using ActionStreetMap.Core.Scene.Buildings;
using ActionStreetMap.Core.Unity;
using ActionStreetMap.Infrastructure.Dependencies;
using ActionStreetMap.Maps.Helpers;
using ActionStreetMap.Explorer.Scene.Buildings;
using ActionStreetMap.Explorer.Scene.Geometry;
using ActionStreetMap.Explorer.Scene.Utils;
using ActionStreetMap.Explorer.Helpers;

namespace ActionStreetMap.Explorer.Scene.Builders
{
    /// <summary>
    ///     Provides logic to build buildings.
    /// </summary>
    public class BuildingModelBuilder : ModelBuilder
    {
        private readonly IBuildingBuilder _builder;

        private readonly HeightMapProcessor _heightMapProcessor;

        /// <inheritdoc />
        public override string Name { get { return "building"; } }

        /// <summary>
        ///     Creates BuildingModelBuilder.
        /// </summary>
        [Dependency]
        public BuildingModelBuilder(IBuildingBuilder builder, HeightMapProcessor heightMapProcessor)
        {
            _builder = builder;
            _heightMapProcessor = heightMapProcessor;
        }

        private const int NoValue = 0;

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

            PointUtils.GetClockwisePolygonPoints(tile.HeightMap, tile.RelativeNullPoint, footPrint, points);
            var minHeight = rule.GetMinHeight();

            // NOTE simplification is important to build hipped/gabled roofs
            //PolygonUtils.Simplify(points, simplified);

            var elevation = AdjustHeightMap(tile.HeightMap, points, minHeight);

            if (tile.Registry.Contains(model.Id))
                return null;

            var gameObject = BuildGameObject(tile, rule, model, points, elevation, minHeight);

            ObjectPool.StoreList(points);
            //ObjectPool.StoreList(simplified);

            return gameObject;
        }

        private float AdjustHeightMap(HeightMap heightMap, List<MapPoint> footPrint, float minHeight)
        {
            // TODO if we have added building to WorldManager then
            // we should use elevation from existing building

            var elevation = footPrint.Average(p => p.Elevation);

            for (int i = 0; i < footPrint.Count; i++)
                footPrint[i].SetElevation(elevation);
            // NOTE do not adjust height map in case of positive minHeight
            if (!heightMap.IsFlat && Math.Abs(minHeight) < 0.5f)
                _heightMapProcessor.AdjustPolygon(footPrint, elevation);

            return elevation;
        }

        private IGameObject BuildGameObject(Tile tile, Rule rule, Model model, List<MapPoint> points,
            float elevation, float minHeight)
        {
            var gameObjectWrapper = GameObjectFactory
                .CreateNew(GetName(model), tile.GameObject);

            // NOTE observed that min_height should be subracted from height for building:part
            // TODO this should be done in mapcss, but stylesheet doesn't support multiply eval operations
            // on the same tag

            var height = rule.GetHeight(NoValue);
            if (rule.IsPart())
                height -= minHeight;

            // TODO should we save this object in WorldManager?
            var building = new Building
            {
                Id = model.Id,
                Address = AddressExtractor.Extract(model.Tags),
                GameObject = gameObjectWrapper,
                Height = height,
                Levels = rule.GetLevels(NoValue),
                MinHeight = minHeight,
                Type = rule.GetBuildingType(),
                FacadeColor = rule.GetFillColor(),
                FacadeMaterial = rule.GetFacadeMaterial(),
                RoofType = rule.GetRoofType(),
                RoofColor = rule.GetRoofColor(),
                RoofMaterial = rule.GetRoofMaterial(),
                RoofHeight = rule.GetRoofHeight(),
                Elevation = elevation, // we set equal elevation for every point
                Footprint = points,
            };

            var theme = ThemeProvider.Get();
            BuildingStyle style = theme.GetBuildingStyle(building);

            _builder.Build(tile.HeightMap, building, style);

            tile.Registry.Register(building);
            tile.Registry.RegisterGlobal(building.Id);

            return gameObjectWrapper;
        }
    }
}