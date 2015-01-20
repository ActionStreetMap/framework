using ActionStreetMap.Core.Unity;
using ActionStreetMap.Explorer.Infrastructure;

namespace ActionStreetMap.Tests.Explorer.Tiles.Stubs
{
    public class TestGameObjectFactory : GameObjectFactory
    {
        public override IGameObject CreateNew(string name)
        {
            return new TestGameObject();
        }
    }
}