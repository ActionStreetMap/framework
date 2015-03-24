using ActionStreetMap.Core;
using ActionStreetMap.Core.MapCss.Domain;
using ActionStreetMap.Core.Tiling.Models;
using ActionStreetMap.Core.Unity;
using ActionStreetMap.Explorer.Geometry.Generators;
using ActionStreetMap.Explorer.Geometry.Utils;
using ActionStreetMap.Explorer.Helpers;
using ActionStreetMap.Explorer.Utils;
using ActionStreetMap.Infrastructure.Reactive;
using UnityEngine;

namespace ActionStreetMap.Explorer.Scene
{
    /// <summary> Provides logic to build spheres. </summary>
    public class SphereModelBuilder : ModelBuilder
    {
        /// <inheritdoc />
        public override string Name { get { return "sphere"; } }

        /// <inheritdoc />
        public override IGameObject BuildArea(Tile tile, Rule rule, Area area)
        {
            base.BuildArea(tile, rule, area);

            if (tile.Registry.Contains(area.Id))
                return null;

            var circle = CircleUtils.GetCircle(tile.RelativeNullPoint, area.Points);
            var radius = circle.Item1 / 2;
            var center = circle.Item2;
            var minHeight = rule.GetMinHeight();

            var elevation = ElevationProvider.GetElevation(center);

            tile.Registry.RegisterGlobal(area.Id);

            var color = rule.GetFillColor();
            var gradient = ResourceProvider.GetGradient(color);

            var meshData = ObjectPool.CreateMeshData();
            meshData.GameObject = GameObjectFactory.CreateNew(GetName(area));
            meshData.MaterialKey = rule.GetMaterialKey();

            IcoSphereGenerator.Generate(meshData, new Vector3(center.X, elevation + minHeight, center.Y), radius, 2, gradient);

            BuildObject(tile.GameObject, meshData);

            return meshData.GameObject;
        }
    }
}