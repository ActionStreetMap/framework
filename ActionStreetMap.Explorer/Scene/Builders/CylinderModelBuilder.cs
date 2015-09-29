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
    /// <summary> Provides logic to build cylinders. </summary>
    internal class CylinderModelBuilder : ModelBuilder
    {
        /// <inheritdoc />
        public override string Name { get { return "cylinder"; } }

        /// <inheritdoc />
        public override IGameObject BuildArea(Tile tile, Rule rule, Area area)
        {
            if (tile.Registry.Contains(area.Id))
                return null;

            double radius;
            Vector2d center;
            CircleUtils.GetCircle(tile.RelativeNullPoint, area.Points, out radius, out center);

            var elevation = ElevationProvider.GetElevation(center);

            var height = rule.GetHeight();
            var minHeight = rule.GetMinHeight();
            var actualHeight = (height - minHeight);

            var materialKey = rule.GetFacadeMaterial();
            var gradient = CustomizationService.GetGradient(rule.GetFacadeColor());
            var texture = CustomizationService
                            .GetAtlas(materialKey)
                            .Get(rule.GetFacadeTexture())
                            .Get((int) area.Id);

            tile.Registry.RegisterGlobal(area.Id);

            var cylinderGen = new CylinderGenerator()
                .SetCenter(new Vector3((float) center.X, elevation + minHeight, (float) center.Y))
                .SetHeight(actualHeight)
                .SetMaxSegmentHeight(5f)
                .SetRadialSegments(7)
                .SetRadius((float) radius)
                .SetGradient(gradient)
                .SetTexture(texture);

            var meshData = new MeshData(MeshDestroyIndex.Default, cylinderGen.CalculateVertexCount())
            {
                GameObject = GameObjectFactory.CreateNew(GetName(area)),
                MaterialKey = rule.GetMaterialKey(materialKey, false)
            };
            cylinderGen.Build(meshData);

            BuildObject(tile.GameObject, meshData, rule, area);
            return meshData.GameObject;
        }
    }
}