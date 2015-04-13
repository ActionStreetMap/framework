using ActionStreetMap.Core.Scene;
using ActionStreetMap.Explorer.Geometry;
using ActionStreetMap.Explorer.Infrastructure;
using ActionStreetMap.Explorer.Utils;
using ActionStreetMap.Infrastructure.Dependencies;
using ActionStreetMap.Infrastructure.Utilities;

namespace ActionStreetMap.Explorer.Scene.Facades
{
    internal class EmptyFacadeBuilder: IFacadeBuilder
    {
        private readonly IResourceProvider _resourceProvider;
        private readonly IObjectPool _objectPool;

        public string Name { get { return "empty"; } }

        [Dependency]
        public EmptyFacadeBuilder(IResourceProvider resourceProvider, IObjectPool objectPool)
        {
            _resourceProvider = resourceProvider;
            _objectPool = objectPool;
        }

        public MeshData Build(Building building)
        {
            var random = new System.Random((int)building.Id);

            var footprint = building.Footprint;
            var gradient = _resourceProvider.GetGradient(building.FacadeColor);
            var meshData = _objectPool.CreateMeshData();
            var index = new FacadeMeshIndex(building.Footprint.Count, meshData.Triangles);
            meshData.MaterialKey = building.FacadeMaterial;
            meshData.Index = index;

            var firstFloorHeight = random.NextFloat(2.2f, 3.2f);
            firstFloorHeight = building.Height > firstFloorHeight ? firstFloorHeight : building.Height;

            var simpleBuilder = new EmptySideBuilder(meshData, building.Height, random)
                .SetFacadeGradient(gradient)
                .SetFirstFloorHeight(firstFloorHeight)
                .SetElevation(building.MinHeight + building.Elevation)
                .SetPositionNoise(building.IsPart ? 0.01f : 0.15f)
                .SetFloorHeight(random.NextFloat(2.9f, 4.2f))
                .SetFloorSpan(random.NextFloat(0.7f, 1.2f))
                .SetFloors(building.Levels)
                .CalculateFloors();

            for (int i = 0; i < footprint.Count; i++)
            {
                var nextIndex = i == (footprint.Count - 1) ? 0 : i + 1;
                var start = footprint[i];
                var end = footprint[nextIndex];
                index.SetSide(start, end);
                simpleBuilder.Build(start, end);
            }

            index.Build();

            return meshData;
        }
    }
}
