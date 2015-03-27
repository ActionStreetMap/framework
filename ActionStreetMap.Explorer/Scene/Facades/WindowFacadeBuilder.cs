using ActionStreetMap.Core.Scene;
using ActionStreetMap.Explorer.Geometry;
using ActionStreetMap.Explorer.Infrastructure;
using ActionStreetMap.Explorer.Utils;
using ActionStreetMap.Infrastructure.Dependencies;
using ActionStreetMap.Infrastructure.Utilities;

namespace ActionStreetMap.Explorer.Scene.Facades
{
    internal class WindowFacadeBuilder: IFacadeBuilder
    {
        private readonly IResourceProvider _resourceProvider;
        private readonly IObjectPool _objectPool;

        public string Name { get { return "window"; } }

        [Dependency]
        public WindowFacadeBuilder(IResourceProvider resourceProvider, IObjectPool objectPool)
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
            meshData.MaterialKey = building.FacadeMaterial;

            var simpleBuilder = new WindowSideBuilder(meshData, building.Height, random)
                .SetFacadeGradient(gradient)
                .SetFirstFloorHeight(4)
                .SetElevation(building.MinHeight + building.Elevation)
                .SetFloorHeight(random.NextFloat(2.9f, 4.2f))
                .SetFloorSpan(random.NextFloat(0.7f, 1.2f))
                .CalculateFloors();

            for (int i = 0; i < footprint.Count; i++)
            {
                var nextIndex = i == (footprint.Count - 1) ? 0 : i + 1;
                var start = footprint[i];
                var end = footprint[nextIndex];
                simpleBuilder.Build(start, end);
            }

            return meshData;
        }
    }
}
