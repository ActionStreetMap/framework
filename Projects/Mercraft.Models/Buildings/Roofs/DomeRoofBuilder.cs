﻿using System;
using ActionStreetMap.Core;
using ActionStreetMap.Core.Scene.World.Buildings;
using ActionStreetMap.Core.Unity;
using ActionStreetMap.Infrastructure.Dependencies;
using ActionStreetMap.Models.Geometry;
using UnityEngine;

namespace ActionStreetMap.Models.Buildings.Roofs
{
    /// <summary>
    ///     Builds dome roof.
    /// </summary>
    public class DomeRoofBuilder: IRoofBuilder
    {
        private readonly IGameObjectFactory _gameObjectFactory;

        /// <inheritdoc />
        public string Name { get { return "dome"; } }

        /// <summary>
        ///     Creates DomeRoofBuilder
        /// </summary>
        [Dependency]
        public DomeRoofBuilder(IGameObjectFactory gameObjectFactory)
        {
            _gameObjectFactory = gameObjectFactory;
        }

        /// <inheritdoc />
        public bool CanBuild(Building building)
        {
            // we should use this builder only in case of dome type defined explicitly
            // cause we expect that footprint of building has the coresponding shape (circle)
            return building.RoofType == Name;
        }

        /// <inheritdoc />
        public MeshData Build(Building building, BuildingStyle style)
        {
            IGameObject gameObjectWrapper = _gameObjectFactory.CreatePrimitive(Name, UnityPrimitiveType.Sphere);

            var tuple = CircleUtils.GetCircle(building.Footprint);

            var diameter = tuple.Item1;
            var center = tuple.Item2;

            // if offset is zero, than we will use hemisphere
            float offset = 0;
            if (building.RoofHeight > 0)
                offset = building.RoofHeight - diameter/2;

            center.SetElevation(building.Elevation + building.Height + building.MinHeight + offset);

            ProcessObject(gameObjectWrapper, center, diameter, style);

            return new MeshData()
            {
                MaterialKey = style.Roof.Path,
                GameObject = gameObjectWrapper
            };
        }

        /// <summary>
        ///     Sets Unity specific data.
        /// </summary>
        /// <param name="gameObjectWrapper">GameObject wrapper.</param>
        /// <param name="center">Sphere center.</param>
        /// <param name="diameter">Diameter.</param>
        /// <param name="style">Building style.</param>
        protected virtual void ProcessObject(IGameObject gameObjectWrapper, MapPoint center, float diameter, BuildingStyle style)
        {
            var sphere = gameObjectWrapper.GetComponent<GameObject>();
            sphere.transform.localScale = new Vector3(diameter, diameter, diameter);
            sphere.transform.position = new Vector3(center.X, center.Elevation, center.Y);

            // TODO Is there better way to set uv map?
            Mesh mesh = sphere.renderer.GetComponent<MeshFilter>().mesh;
            var uv = mesh.uv;
            for (int i = 0; i < uv.Length; i++)
                uv[i] = style.Roof.FrontUvMap.RightUpper;
            mesh.uv = uv;
        }
    }
}
