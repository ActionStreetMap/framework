using System.Collections.Generic;
using ActionStreetMap.Core.Geometry;
using ActionStreetMap.Core.Scene;
using ActionStreetMap.Explorer;
using ActionStreetMap.Explorer.Infrastructure;
using ActionStreetMap.Explorer.Scene.Roofs;
using ActionStreetMap.Infrastructure.Diagnostic;
using NUnit.Framework;

namespace ActionStreetMap.Tests.Explorer.Scene
{
    [TestFixture]
    class FlatRoofBuilderTests
    {
        [Test]
        public void CanHandleMoreThanVertexLimit()
        {
            // ARRANGE
            FlatRoofBuilder roofBuilder = new FlatRoofBuilder();
            roofBuilder.ObjectPool = TestHelper.GetObjectPool();
            roofBuilder.ResourceProvider = new UnityResourceProvider();
            roofBuilder.Trace = new ConsoleTrace();

            var footprint = new List<Vector2d>()
            {
                new Vector2d(-51.49957, 102.42048),
                new Vector2d(-43.50884, 90.90707),
                new Vector2d(-59.16525, 79.80485),
                new Vector2d(-46.19048, 61.11222),
                new Vector2d(-28.04204, 73.94812),
                new Vector2d(-20.08517, 62.46805),
                new Vector2d(-39.26969, 48.93201),
                new Vector2d(-26.43035, 30.42831),
                new Vector2d(-5.49193, 45.18681),
                new Vector2d(1.94351, 34.47356),
                new Vector2d(-31.90197, 11.04665),
                new Vector2d(-38.49094, 20.55967),
                new Vector2d(-52.14289, 11.11333),
                new Vector2d(-38.74826, -8.17941),
                new Vector2d(-72.30933, -31.39517),
                new Vector2d(-79.02696, -18.85933),
                new Vector2d(-58.49485, -4.65649),
                new Vector2d(-66.30951, 6.61243),
                new Vector2d(-86.73327, -7.51261),
                new Vector2d(-94.79172, 4.10082),
                new Vector2d(-74.53048, 18.11473),
                new Vector2d(-82.81917, 30.05045),
                new Vector2d(-102.96528, 16.11433),
                new Vector2d(-111.01019, 27.71665),
                new Vector2d(-90.83021, 41.675),
                new Vector2d(-98.8074, 53.16619),
                new Vector2d(-118.87225, 39.28563),
                new Vector2d(-126.63274, 50.47676),
                new Vector2d(-94.2906, 72.83679),
                new Vector2d(-92.06268, 69.63615),
                new Vector2d(-78.00441, 79.36031),
                new Vector2d(-80.08336, 82.36091)
            };
            var building = CreateBuilding();
            building.Footprint = footprint;
            building.Levels = 20;

            // ACT
            var meshDataList = roofBuilder.Build(building);

            // ASSERT
            Assert.Greater(meshDataList.Count, 1);
            foreach (var meshData in meshDataList)
                Assert.Less(meshData.Vertices.Length, Consts.MaxMeshSize);
        }

        private Building CreateBuilding()
        {
            return new Building()
            {
                Levels = 0,
                Elevation = 0,
                Height = 10,
                RoofHeight = 2,
                RoofColor = "gradient(#0eff94, #0deb88 50%, #07854d)",
                FloorFrontColor = "gradient(#f3e2c7, #c19e67 50%, #b68d4c 51%, #e9d4b3)",
                FloorBackColor = "gradient(#feffff, #d2ebf9)",
            };
        }
    }
}
