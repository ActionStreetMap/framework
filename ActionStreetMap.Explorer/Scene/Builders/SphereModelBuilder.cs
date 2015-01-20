using System;
using ActionStreetMap.Core;
using ActionStreetMap.Core.MapCss.Domain;
using ActionStreetMap.Core.Scene.Models;
using ActionStreetMap.Core.Unity;
using ActionStreetMap.Models.Geometry;
using ActionStreetMap.Explorer.Helpers;
using UnityEngine;

namespace ActionStreetMap.Explorer.Scene.Builders
{
    /// <summary>
    ///     Provides logic to build spheres.
    /// </summary>
    public class SphereModelBuilder : ModelBuilder
    {
        /// <inheritdoc />
        public override string Name
        {
            get { return "sphere"; }
        }

        /// <inheritdoc />
        public override IGameObject BuildArea(Tile tile, Rule rule, Area area)
        {
            base.BuildArea(tile, rule, area);

            if (tile.Registry.Contains(area.Id))
                return null;

            var circle = CircleUtils.GetCircle(tile.RelativeNullPoint, area.Points);
            var diameter = circle.Item1;
            var sphereCenter = circle.Item2;
            var minHeight = rule.GetMinHeight();

            var elevation = tile.HeightMap.LookupHeight(sphereCenter);

            IGameObject gameObjectWrapper = GameObjectFactory.CreateNew(String.Format("Sphere {0}", area));

            tile.Registry.RegisterGlobal(area.Id);

            return BuildSphere(gameObjectWrapper, rule, area, sphereCenter, diameter, elevation + minHeight);
        }

        /// <summary>
        ///     Process unity specific data.
        /// </summary>
        protected virtual IGameObject BuildSphere(IGameObject gameObjectWrapper, Rule rule, Model model, 
            MapPoint sphereCenter, float diameter, float heightOffset)
        {
            var sphere = gameObjectWrapper.AddComponent(GameObject.CreatePrimitive(PrimitiveType.Sphere));
            sphere.isStatic = true;
            sphere.renderer.sharedMaterial = rule.GetMaterial(ResourceProvider);

            // TODO use defined color
            Mesh mesh = sphere.renderer.GetComponent<MeshFilter>().mesh;
            var uv = mesh.uv;
            for (int i = 0; i < uv.Length; i++)
                uv[i] = new Vector2(0, 0);
            mesh.uv = uv;

            sphere.transform.localScale = new Vector3(diameter, diameter, diameter);
            sphere.transform.position = new Vector3(sphereCenter.X, heightOffset + diameter/2, sphereCenter.Y);

            return gameObjectWrapper;
        }
    }
}