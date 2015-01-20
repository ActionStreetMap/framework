using ActionStreetMap.Core;
using ActionStreetMap.Core.MapCss.Domain;
using ActionStreetMap.Core.Scene.Models;
using ActionStreetMap.Core.Unity;
using ActionStreetMap.Explorer.Scene.Builders;

namespace ActionStreetMap.Tests.Explorer.Tiles.Stubs.ModelBuilders
{
    class TestSphereModelBuilder: SphereModelBuilder
    {
        protected override void BuildSphere(IGameObject gameObjectWrapper, Rule rule, Model model, MapPoint sphereCenter, float diameter,
            float heightOffset)
        {
            // Do nothing
        }
    }
}
