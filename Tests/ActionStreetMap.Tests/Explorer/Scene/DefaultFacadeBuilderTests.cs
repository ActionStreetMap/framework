using System.Collections.Generic;
using ActionStreetMap.Core.Geometry;
using ActionStreetMap.Core.Scene;
using ActionStreetMap.Explorer.Scene.Facades;
using NUnit.Framework;

namespace ActionStreetMap.Tests.Explorer.Scene
{
    [TestFixture]
    class DefaultFacadeBuilderTests
    {
        [Test]
        public void CanHandleVertexLimit()
        {
            // ARRANGE
            var builder = new FacadeBuilder(TestHelper.GetCustomizationService())
            {
                Trace = new ConsoleTrace()
            };
            var building = new Building()
            {
                Footprint = new List<Vector2d>()
                {
                    new Vector2d(0, 0),
                    new Vector2d(500, 0),
                    new Vector2d(500, 500),
                    new Vector2d(0, 500),
                },
                FacadeColor = "gradient(#0eff94, #0deb88 50%, #07854d)",
                Height = 100
            };

            // ACT
            var result = builder.Build(building);

            // ASSERT
            Assert.IsNotNull(result);
            Assert.Greater(result.Count, 1);
        }
    }
}
