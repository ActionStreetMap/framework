using System;
using ActionStreetMap.Core.Scene.Buildings;
using ActionStreetMap.Core.Unity;
using ActionStreetMap.Explorer.Geometry;
using ActionStreetMap.Explorer.Infrastructure;
using ActionStreetMap.Explorer.Scene.Buildings.Facades;
using ActionStreetMap.Infrastructure.Dependencies;
using ActionStreetMap.Infrastructure.Reactive;
using ActionStreetMap.Infrastructure.Utilities;
using ActionStreetMap.Explorer.Scene.Buildings.Roofs;
using ActionStreetMap.Explorer.Utils;
using UnityEngine;

namespace ActionStreetMap.Explorer.Scene.Buildings
{
    /// <summary> Defines building builder logic. </summary>
    public interface IBuildingBuilder
    {
        /// <summary> Builds building. </summary>
        /// <param name="building">Building.</param>
        /// <param name="facadeBuilder">Facade builder.</param>
        /// <param name="roofBuilder">Roof builder.</param>
        void Build(Building building, IFacadeBuilder facadeBuilder, IRoofBuilder roofBuilder);
    }

    /// <summary> Default building builder. </summary>
    public class BuildingBuilder : IBuildingBuilder
    {
        private readonly IResourceProvider _resourceProvider;
        private readonly IObjectPool _objectPool;

        /// <summary> Creates instance of <see cref="BuildingBuilder"/>. </summary>
        /// <param name="resourceProvider">Resource provider.</param>
        /// <param name="objectPool">Object pool.</param>
        [Dependency]
        public BuildingBuilder(IResourceProvider resourceProvider, IObjectPool objectPool)
        {
            _resourceProvider = resourceProvider;
            _objectPool = objectPool;
        }

        /// <inheritdoc />
        public void Build(Building building, IFacadeBuilder facadeBuilder, IRoofBuilder roofBuilder)
        {
            var facadeMeshData = facadeBuilder.Build(building);
            var roofMeshData = roofBuilder.Build(building);

            Scheduler.MainThread.Schedule(() =>
            {
                // NOTE use different gameObject only to support different materials
                AttachChildGameObject(building.GameObject, "facade", facadeMeshData);
                AttachChildGameObject(building.GameObject, "roof", roofMeshData);
            });
        }

        /// <summary> Process unity's game object. </summary>
        protected virtual void AttachChildGameObject(IGameObject parent, string name, MeshData meshData)
        {
            GameObject gameObject = GetGameObject(meshData);
            gameObject.isStatic = true;
            gameObject.transform.parent = parent.GetComponent<GameObject>().transform;
            gameObject.name = name;
            gameObject.renderer.sharedMaterial = _resourceProvider
              .GetMatertial(meshData.MaterialKey);
        }

        private GameObject GetGameObject(MeshData meshData)
        {
            // GameObject was created directly in builder, so we can use it and ignore other meshData properties.
            // also we expect that all components are defined
            if (meshData.GameObject != null && !meshData.GameObject.IsEmpty)
               return meshData.GameObject.GetComponent<GameObject>();

            var gameObject = new GameObject();
            var mesh = new Mesh();
            mesh.vertices = meshData.Vertices.ToArray();
            mesh.triangles = meshData.Triangles.ToArray();
            mesh.colors = meshData.Colors.ToArray();

            mesh.RecalculateNormals();

            _objectPool.RecycleMeshData(meshData);

            gameObject.AddComponent<MeshFilter>().mesh = mesh;
            gameObject.AddComponent<MeshCollider>();
            gameObject.AddComponent<MeshRenderer>();

            return gameObject;
        }
    }
}
