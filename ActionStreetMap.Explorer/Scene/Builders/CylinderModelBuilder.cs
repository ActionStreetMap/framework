using ActionStreetMap.Core.MapCss.Domain;
using ActionStreetMap.Core.Tiling.Models;
using ActionStreetMap.Core.Unity;
using ActionStreetMap.Explorer.Geometry;
using ActionStreetMap.Explorer.Geometry.Generators;
using ActionStreetMap.Explorer.Geometry.Utils;
using ActionStreetMap.Explorer.Helpers;
using ActionStreetMap.Explorer.Utils;
using UnityEngine;

namespace ActionStreetMap.Explorer.Scene.Builders
{
    /// <summary> Provides logic to build cylinders. </summary>
    public class CylinderModelBuilder : ModelBuilder
    {
        /// <inheritdoc />
        public override string Name
        {
            get { return "cylinder"; }
        }

        /// <inheritdoc />
        public override IGameObject BuildArea(Tile tile, Rule rule, Area area)
        {
            base.BuildArea(tile, rule, area);

            if (tile.Registry.Contains(area.Id))
                return null;

            var circle = CircleUtils.GetCircle(tile.RelativeNullPoint, area.Points);
            var diameter = circle.Item1;
            var cylinderCenter = circle.Item2;

            var elevation = ElevationProvider.GetElevation(cylinderCenter);

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
                .SetCenter(new Vector3((float)cylinderCenter.X, elevation + minHeight, (float)cylinderCenter.Y))
                .SetHeight(actualHeight)
                .SetMaxSegmentHeight(5f)
                .SetRadialSegments(7)
                .SetRadius((float)diameter / 2)
                .SetGradient(gradient)
                .Build();

            BuildObject(tile.GameObject, meshData, rule, area);

            return meshData.GameObject;
        }
    }
}