using System;
using System.Collections.Generic;
using ActionStreetMap.Core.Scene.Buildings;
using ActionStreetMap.Explorer.Geometry;
using ActionStreetMap.Infrastructure.Dependencies;
using UnityEngine;

namespace ActionStreetMap.Explorer.Scene.Buildings.Facades
{
    internal class MeshFacadeBuilder: IFacadeBuilder
    {
        private readonly IResourceProvider _resourceProvider;

        public string Name { get { return "flat"; } }

        [Dependency]
        public MeshFacadeBuilder(IResourceProvider resourceProvider)
        {
            _resourceProvider = resourceProvider;
        }

        public MeshData Build(Building building, BuildingStyle style)
        {
            var footprint = building.Footprint;
            var gradient = _resourceProvider.GetGradient("building.default");
            var meshData = new MeshData
            {
                Vertices = new List<Vector3>(1024),
                Triangles = new List<int>(2048),
                Colors = new List<Color>(1024),
                MaterialKey = style.Facade.Path
            };

            var simpleBuilder = new EmptySideBuilder(meshData, building.Height)
                .SetFacadeGradient(gradient)
                .SetFirstFloorHeight(4)
                .SetElevation(building.Elevation)
                .Optimize();

            for (int i = 0; i < footprint.Count; i++)
            {
                var nextIndex = i == (footprint.Count - 1) ? 0 : i + 1;
                simpleBuilder.Build(footprint[i], footprint[nextIndex]);
            }

            return meshData;
        }
    }
}
