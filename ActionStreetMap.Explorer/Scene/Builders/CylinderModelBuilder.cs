using ActionStreetMap.Core.Geometry;
using ActionStreetMap.Core.MapCss.Domain;
using ActionStreetMap.Core.Tiling.Models;
using ActionStreetMap.Core.Unity;
using ActionStreetMap.Explorer.Geometry;
using ActionStreetMap.Explorer.Geometry.Generators;
using ActionStreetMap.Explorer.Geometry.Utils;
using ActionStreetMap.Explorer.Helpers;
using UnityEngine;

namespace ActionStreetMap.Explorer.Scene.Builders
{
    /// <summary> Provides logic to build cylinders. </summary>
    public class CylinderModelBuilder : ModelBuilder
    {
        /// <inheritdoc />
        public override string Name { get { return "cylinder"; } }

        /// <inheritdoc />
        public override IGameObject BuildArea(Tile tile, Rule rule, Area area)
        {
            base.BuildArea(tile, rule, area);

            if (tile.Registry.Contains(area.Id))
                return null;

            double radius;
            Vector2d center;
            CircleUtils.GetCircle(tile.RelativeNullPoint, area.Points, out radius, out center);

            var elevation = ElevationProvider.GetElevation(center);

            var height = rule.GetHeight();
            var minHeight = rule.GetMinHeight();
            var actualHeight = (height - minHeight);
            var color = rule.GetFillColor();
            var gradient = ResourceProvider.GetGradient(color);

            tile.Registry.RegisterGlobal(area.Id);

            var meshData = new MeshData
            {
                GameObject = GameObjectFactory.CreateNew(GetName(area)),
                MaterialKey = rule.GetMaterialKey()
            };
            new CylinderGenerator(meshData)
                .SetCenter(new Vector3((float)center.X, elevation + minHeight, (float)center.Y))
                .SetHeight(actualHeight)
                .SetMaxSegmentHeight(5f)
                .SetRadialSegments(7)
                .SetRadius((float)radius)
                .SetGradient(gradient)
                .Build();

            BuildObject(tile.GameObject, meshData, rule, area);

            return meshData.GameObject;
        }
    }
}