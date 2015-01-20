using ActionStreetMap.Core;
using ActionStreetMap.Core.MapCss.Domain;
using ActionStreetMap.Core.Scene.Models;
using ActionStreetMap.Core.Unity;
using ActionStreetMap.Explorer.Scene.Builders;

namespace ActionStreetMap.Tests.Explorer.Tiles.Stubs.ModelBuilders
{
    class TestDetailModelBuilder: DetailModelBuilder
    {
        protected override void BuildObject(IGameObject gameObjectWrapper, Tile tile, Rule rule, Node node, 
            MapPoint mapPoint, float zIndex, string detail)
        {
            // do nothing
        }
    }
}
