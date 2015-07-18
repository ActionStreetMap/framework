using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ActionStreetMap.Core;
using ActionStreetMap.Core.Geometry;
using ActionStreetMap.Core.Geometry.Clipping;
using ActionStreetMap.Core.Geometry.StraightSkeleton;
using ActionStreetMap.Core.MapCss.Domain;
using ActionStreetMap.Core.Scene;
using ActionStreetMap.Core.Scene.InDoor;
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
            PointUtils.GetClockwisePolygonPoints(_elevationProvider, tile.RelativeNullPoint, footPrint, points);

            var minHeight = BuildingRuleExtensions.GetMinHeight(rule);

            var elevation = points.Average(p => p.Elevation);

            if (tile.Registry.Contains(model.Id))
                return null;

            var gameObject = BuildGameObject(tile, rule, model, points, elevation, minHeight);

            ObjectPool.StoreList(points);

            return gameObject;
        }

        private IGameObject BuildGameObject(Tile tile, Rule rule, Model model, List<MapPoint> points,
            float elevation, float minHeight)
        {
            tile.Registry.RegisterGlobal(model.Id);

            var gameObjectWrapper = GameObjectFactory
                .CreateNew(GetName(model), tile.GameObject);

            var isPart = rule.IsPart();
            var height = rule.GetHeight();

            // NOTE: this is not clear
            //if (isPart)
            height -= minHeight;

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

            // create floor plans
            building.FloorPlans = CreateFloorPlans(model.Id, points);

            // facade
            var facadeBuilder = _facadeBuilders.Single(f => f.Name == building.FacadeType);
            var facadeMeshData = facadeBuilder.Build(building);
            facadeMeshData.GameObject = GameObjectFactory.CreateNew("facade");
            facadeMeshData.MaterialKey = building.FacadeMaterial;
            BuildObject(gameObjectWrapper, facadeMeshData, rule, model);

            // roof
            var roofBuilder = _roofBuilders.Single(f => f.Name == building.RoofType);
            var roofMeshData = roofBuilder.Build(building);
            roofMeshData.GameObject = GameObjectFactory.CreateNew("roof");
            roofMeshData.MaterialKey = building.RoofMaterial;
            BuildObject(gameObjectWrapper, roofMeshData, rule, model);

            return gameObjectWrapper;
        }

        private List<Floor> CreateFloorPlans(long id, List<MapPoint> footprint)
        {
            const double shift = 0;
            const double translate = 1;
            InDoorGeneratorSettings settings = null;
            var ggg = footprint
                .Select(p => new Vector2d(translate * p.X + shift, translate * p.Y + shift))
                .ToList();
            try
            {
                var skeleton = SkeletonBuilder.Build(ggg);
                var indoorGenerator = new InDoorGenerator();
                settings = CreateSettings(skeleton, ggg);
                return new List<Floor>(1)
                {
                    indoorGenerator.Build(settings)
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                Trace.Error("floor.generator", ex, "Unable to generate floor plan for {0}", 
                    id.ToString());
                lock (this)
                {
                    // NOTE test code is there
                    using (var sw = File.AppendText(@"points.txt"))
                    {
                        sw.WriteLine("id:{0}", id);
                        if (settings != null)
                        {
                            var door = settings.Doors[0];
                            sw.WriteLine("door: {0} {1}", door.Key, door.Value);
                        }
                        foreach (var point in footprint)
                            sw.WriteLine("new PointF({0}f, {1}f),", point.X, point.Y);

                        sw.WriteLine();
                        sw.WriteLine();
                        sw.WriteLine();
                        sw.WriteLine();
                    }
                }
            }
            return null;
        }


        private InDoorGeneratorSettings CreateSettings(Skeleton skeleton, List<Vector2d> footprint)
        {
            var index = 0;
            var maxDistance = 0d;
            for (int i = 0; i < footprint.Count; i++)
            {
                var start = footprint[i];
                var end = footprint[i + 1 == footprint.Count ? 0 : i + 1];
                var distance = start.DistanceTo(end);
                if (distance > maxDistance)
                {
                    index = i;
                    maxDistance = distance;
                }
            }

            var doors = new List<KeyValuePair<int, double>>
            {
                new KeyValuePair<int, double>(index, maxDistance/2d)
            };

            return new InDoorGeneratorSettings(ObjectPool, new Clipper(), skeleton, 
                footprint, null, doors, 2);
        }
    }
}