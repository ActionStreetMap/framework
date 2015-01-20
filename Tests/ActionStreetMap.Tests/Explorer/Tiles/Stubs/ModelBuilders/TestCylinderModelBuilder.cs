using ActionStreetMap.Core;
using ActionStreetMap.Core.MapCss.Domain;
using ActionStreetMap.Core.Scene.Models;
using ActionStreetMap.Core.Unity;
using ActionStreetMap.Explorer.Scene.Builders;

namespace ActionStreetMap.Tests.Explorer.Tiles.Stubs.ModelBuilders
{
    public class TestCylinderModelBuilder : CylinderModelBuilder
    {
        protected override void BuildCylinder(IGameObject gameObjectWrapper, Rule rule, Model model, MapPoint cylinderCenter, float diameter,
            float actualHeight, float heightOffset)
        {
            // do nothing
        }
    }
}