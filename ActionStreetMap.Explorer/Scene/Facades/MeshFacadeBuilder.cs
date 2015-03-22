using System.Collections.Generic;
using ActionStreetMap.Core.Scene.Buildings;
using ActionStreetMap.Explorer.Geometry;
using ActionStreetMap.Explorer.Infrastructure;
using ActionStreetMap.Infrastructure.Dependencies;
using UnityEngine;

namespace ActionStreetMap.Explorer.Scene.Facades
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

        public MeshData Build(Building building)
        {
            var footprint = building.Footprint;
            var gradient = _resourceProvider.GetGradient(building.FacadeColor);
            var meshData = new MeshData
            {
                Vertices = new List<Vector3>(1024),
                Triangles = new List<int>(2048),
                Colors = new List<Color>(1024),
                MaterialKey = building.FacadeMaterial
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
