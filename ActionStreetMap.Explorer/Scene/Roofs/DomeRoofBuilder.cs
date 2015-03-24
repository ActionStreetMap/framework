using ActionStreetMap.Core;
using ActionStreetMap.Core.Scene;
using ActionStreetMap.Core.Unity;
using ActionStreetMap.Explorer.Geometry;
using ActionStreetMap.Explorer.Geometry.Utils;
using ActionStreetMap.Infrastructure.Reactive;
using UnityEngine;

namespace ActionStreetMap.Explorer.Scene.Roofs
{
    /// <summary> Builds dome roof. </summary>
    public class DomeRoofBuilder : RoofBuilder
    {
        /// <inheritdoc />
        public override string Name { get { return "dome"; } }

        /// <inheritdoc />
        public override bool CanBuild(Building building)
        {
            // we should use this builder only in case of dome type defined explicitly
            // cause we expect that footprint of building has the coresponding shape (circle)
            return building.RoofType == Name;
        }

        /// <inheritdoc />
        public override MeshData Build(Building building)
        {
            IGameObject gameObjectWrapper = GameObjectFactory.CreateNew(Name);

            var tuple = CircleUtils.GetCircle(building.Footprint);

            var diameter = tuple.Item1;
            var center = tuple.Item2;

            // if offset is zero, than we will use hemisphere
            float offset = 0;
            if (building.RoofHeight > 0)
                offset = building.RoofHeight - diameter/2;

            center.SetElevation(building.Elevation + building.Height + building.MinHeight + offset);

            Scheduler.MainThread.Schedule(() => ProcessObject(gameObjectWrapper, center, diameter));

            return new MeshData()
            {
                MaterialKey = building.RoofMaterial,
                GameObject = gameObjectWrapper
            };
        }

        /// <summary> Sets Unity specific data. </summary>
        /// <param name="gameObjectWrapper">GameObject wrapper.</param>
        /// <param name="center">Sphere center.</param>
        /// <param name="diameter">Diameter.</param>
        protected virtual void ProcessObject(IGameObject gameObjectWrapper, MapPoint center, float diameter)
        {
            var sphere = gameObjectWrapper.AddComponent(GameObject.CreatePrimitive(PrimitiveType.Sphere));
            sphere.transform.localScale = new Vector3(diameter, diameter, diameter);
            sphere.transform.position = new Vector3(center.X, center.Elevation, center.Y);

            // TODO set colors
            Mesh mesh = sphere.renderer.GetComponent<MeshFilter>().mesh;
        }
    }
}
