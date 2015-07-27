using ActionStreetMap.Core.Geometry;
using ActionStreetMap.Core.Geometry.Utils;
using ActionStreetMap.Core.MapCss.Domain;
using ActionStreetMap.Core.Tiling.Models;
using ActionStreetMap.Core.Unity;
using ActionStreetMap.Explorer.Helpers;
using ActionStreetMap.Explorer.Scene.Generators;
using ActionStreetMap.Explorer.Utils;
using UnityEngine;

namespace ActionStreetMap.Explorer.Scene.Builders
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
            tile.Registry.RegisterGlobal(area.Id);

            double radius;
            Vector2d center;
            CircleUtils.GetCircle(tile.RelativeNullPoint, area.Points, out radius, out center);

            var elevation = ElevationProvider.GetElevation(center);
            var minHeight = rule.GetMinHeight();
            var color = rule.GetFillColor();
            var gradient = ResourceProvider.GetGradient(color);

            int recursionLevel = rule.EvaluateDefault("recursion_level", 2);

            var meshData = new MeshData();
            meshData.GameObject = GameObjectFactory.CreateNew(GetName(area));
            meshData.MaterialKey = rule.GetMaterialKey();

            new IcoSphereGenerator(meshData)
                .SetCenter(new Vector3((float)center.X, elevation + minHeight, (float)center.Y))
                .SetRadius((float)radius)
                .SetRecursionLevel(recursionLevel)
                .SetGradient(gradient)
                .Build();

            BuildObject(tile.GameObject, meshData, rule, area);

            return meshData.GameObject;
        }
    }
}