using System;
using System.Collections.Generic;
using ActionStreetMap.Core.Geometry;
using ActionStreetMap.Core.Scene;
using ActionStreetMap.Explorer.Infrastructure;
using ActionStreetMap.Explorer.Scene.Facades;
using NUnit.Framework;

namespace ActionStreetMap.Tests.Explorer.Scene
{
    [TestFixture]
    class EmptyFacadeBuilderTests
    {
        [Test]
        public void CanHandleVertexLimit()
        {
            // ARRANGE
            var builder = new EmptyFacadeBuilder(new UnityResourceProvider())
            {
                Trace = new ConsoleTrace()
            };
            var building = new Building()
            {
                Footprint = new List<Vector2d>()
                {
                    new Vector2d(0, 0),
                    new Vector2d(100, 0),
                    new Vector2d(100, 100),
                    new Vector2d(0, 100),
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
