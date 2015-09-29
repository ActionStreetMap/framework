using ActionStreetMap.Core.Geometry;
using ActionStreetMap.Core.Geometry.Utils;
using ActionStreetMap.Core.MapCss.Domain;
using ActionStreetMap.Core.Tiling.Models;
using ActionStreetMap.Core.Unity;
using ActionStreetMap.Explorer.Customization;
using ActionStreetMap.Explorer.Scene.Generators;
using ActionStreetMap.Explorer.Scene.Indices;
using UnityEngine;

namespace ActionStreetMap.Explorer.Scene.Builders
{
    /// <summary> Provides logic to build spheres. </summary>
    internal class SphereModelBuilder : ModelBuilder
    {
        /// <inheritdoc />
        public override string Name { get { return "sphere"; } }

        /// <inheritdoc />
        public override IGameObject BuildArea(Tile tile, Rule rule, Area area)
        {
            if (tile.Registry.Contains(area.Id))
                return null;
            tile.Registry.RegisterGlobal(area.Id);

            double radius;
            Vector2d center;
            CircleUtils.GetCircle(tile.RelativeNullPoint, area.Points, out radius, out center);

            var elevation = ElevationProvider.GetElevation(center);
            var minHeight = rule.GetMinHeight();

            var materialKey = rule.GetFacadeMaterial();
            var gradient = CustomizationService.GetGradient(rule.GetFacadeColor());
            var texture = CustomizationService
                           .GetAtlas(materialKey)
                           .Get(rule.GetFacadeTexture())
                           .Get((int)area.Id);

            int recursionLevel = rule.EvaluateDefault("recursion_level", 2);

            var center3d = new Vector3((float) center.X, elevation + minHeight, (float) center.Y);          

            var sphereGen = new IcoSphereGenerator()
                .SetCenter(center3d)
                .SetRadius((float)radius)
                .SetRecursionLevel(recursionLevel)
                .SetGradient(gradient)
                .SetTexture(texture);

            var meshData = new MeshData(new SphereMeshIndex((float)radius, center3d),
                sphereGen.CalculateVertexCount());

            meshData.GameObject = GameObjectFactory.CreateNew(GetName(area));
            meshData.MaterialKey = rule.GetMaterialKey(materialKey, false);

            sphereGen.Build(meshData);

            BuildObject(tile.GameObject, meshData, rule, area);

            return meshData.GameObject;
        }
    }
}