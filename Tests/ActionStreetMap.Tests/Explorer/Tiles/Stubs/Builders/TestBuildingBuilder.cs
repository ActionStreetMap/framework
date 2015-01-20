using ActionStreetMap.Core.Unity;
using ActionStreetMap.Infrastructure.Dependencies;
using ActionStreetMap.Infrastructure.Utilities;
using ActionStreetMap.Models.Buildings;
using ActionStreetMap.Models.Utils;

namespace ActionStreetMap.Tests.Explorer.Tiles.Stubs.Builders
{
    public class TestBuildingBuilder: BuildingBuilder
    {
        [Dependency]
        public TestBuildingBuilder(IResourceProvider resourceProvider, IObjectPool objectPool)
            : base(resourceProvider, objectPool)
        {
        }

        protected override void AttachChildGameObject(IGameObject parent, string name, MeshData meshData)
        {
            // Do nothing
        }
    }
}
