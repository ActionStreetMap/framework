using System;
using ActionStreetMap.Core;
using ActionStreetMap.Core.MapCss.Domain;
using ActionStreetMap.Core.Tiling.Models;
using ActionStreetMap.Core.Unity;
using ActionStreetMap.Infrastructure.Reactive;
using ActionStreetMap.Explorer.Scene.Geometry;
using ActionStreetMap.Explorer.Helpers;
using UnityEngine;

namespace ActionStreetMap.Explorer.Scene.Builders
{
    /// <summary>
    ///     Provides logic to build cylinders.
    /// </summary>
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

            var elevation = tile.HeightMap.LookupHeight(cylinderCenter);

            var height = rule.GetHeight();
            var minHeight = rule.GetMinHeight();

            var actualHeight = (height - minHeight) / 2;

            var gameObjectWrapper = GameObjectFactory.CreateNew(GetName(area));

            tile.Registry.RegisterGlobal(area.Id);

            Scheduler.MainThread.Schedule(() =>
                BuildCylinder(gameObjectWrapper, rule, area, cylinderCenter, diameter, actualHeight, elevation+ minHeight));

            return gameObjectWrapper;
        }

        /// <summary>
        ///     Process unity specific data.
        /// </summary>
        protected virtual void BuildCylinder(IGameObject gameObjectWrapper, Rule rule, Model model,
            MapPoint cylinderCenter, float diameter, float actualHeight, float heightOffset)
        {
            var cylinder = gameObjectWrapper.AddComponent(GameObject.CreatePrimitive(PrimitiveType.Cylinder));

            cylinder.transform.localScale = new Vector3(diameter, actualHeight, diameter);
            cylinder.transform.position = new Vector3(cylinderCenter.X, heightOffset + actualHeight, cylinderCenter.Y);

            cylinder.AddComponent<MeshRenderer>();
            cylinder.renderer.sharedMaterial = rule.GetMaterial(ResourceProvider);

            // TODO use defined color
            Mesh mesh = cylinder.renderer.GetComponent<MeshFilter>().mesh;
            var uv = mesh.uv;
            for (int i = 0; i < uv.Length; i++)
                uv[i] = new Vector2(0, 0);
            mesh.uv = uv;
        }
    }
}