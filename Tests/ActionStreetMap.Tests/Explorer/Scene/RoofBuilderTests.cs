using System.Collections.Generic;
using System.Linq;
using ActionStreetMap.Core.Geometry;
using ActionStreetMap.Core.Scene;
using ActionStreetMap.Explorer.Infrastructure;
using ActionStreetMap.Explorer.Scene.Indices;
using ActionStreetMap.Explorer.Scene.Roofs;
using ActionStreetMap.Infrastructure.Diagnostic;
using NUnit.Framework;

namespace ActionStreetMap.Tests.Explorer.Buildings
{
    [TestFixture]
    public class RoofBuilderTests
    {
        [Test]
        public void CanBuildMansard()
        {
            // ARRANGE
            var roofBuilder = new MansardRoofBuilder();
            InitializeRoofBuilder(roofBuilder);
            var building = CreateTestBuilding();
            building.Levels = 1;
            building.Height = 1;
            building.Footprint = new List<Vector2d>()
            {
                new Vector2d(0, 0),
                new Vector2d(0, 10),
                new Vector2d(10, 10),
                new Vector2d(10, 0),
            };

            // ACT
            var result = roofBuilder.Build(building);

            // ASSERT
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(192 + 120, result[0].Vertices.Length);
        }

        [Test]
        public void CanBuildGabled()
        {
            // ARRANGE
            var roofBuilder = new GabledRoofBuilder();
            InitializeRoofBuilder(roofBuilder);

            // ACT
            var meshData = roofBuilder.Build(CreateTestBuilding());

            // ASSERT
            Assert.IsNotNull(meshData);
            Assert.AreEqual(144, meshData.First().Vertices.Length);
        }

        [Test]
        public void CanBuildHipped()
        {
            // ARRANGE
            var roofBuilder = new HippedRoofBuilder();
            InitializeRoofBuilder(roofBuilder);

            // ACT
            var meshData = roofBuilder.Build(CreateTestBuilding());

            // ASSERT
            Assert.IsNotNull(meshData);
            Assert.AreEqual(144, meshData.First().Vertices.Length);
        }

        [Test]
        public void CanBuildHippedFallback()
        {
            // ARRANGE
            var roofBuilder = new HippedRoofBuilder();
            InitializeRoofBuilder(roofBuilder);
            var building = CreateTestBuilding();
            building.Footprint = new List<Vector2d>()
            {
                new Vector2d(114.68728, -7.44593),
                new Vector2d(114.43672, -6.15679),
                new Vector2d(115.5947, -5.84561),
                new Vector2d(116.62401, -5.31217),
                new Vector2d(117.50435, -4.56758),
                new Vector2d(118.26279, -5.44553),
                new Vector2d(118.34405, -5.38997),
                new Vector2d(119.31919, -4.02303),
                new Vector2d(119.96251, -2.24489),
                new Vector2d(119.90834, 0.54455),
                new Vector2d(118.47272, 3.18953),
                new Vector2d(123.82244, 6.49019),
                new Vector2d(127.97356, 7.40148),
                new Vector2d(134.58961, -0.86684),
                new Vector2d(135.21939, -0.20004),
                new Vector2d(140.47431, -6.61243),
                new Vector2d(136.56698, -15.86984),
                new Vector2d(128.69137, -16.92561),
                new Vector2d(128.50853, -15.86984),
                new Vector2d(122.3665, -16.73668),
                new Vector2d(122.45454, -17.31457),
                new Vector2d(118.52689, -17.73688),
                new Vector2d(118.4524, -17.27012),
                new Vector2d(114.22002, -17.89247),
                new Vector2d(114.34191, -18.48147),
                new Vector2d(110.39395, -19.13716),
                new Vector2d(110.21789, -18.43702),
                new Vector2d(108.33532, -18.77042),
                new Vector2d(105.35573, -8.71285),
                new Vector2d(108.74163, -8.24609),
                new Vector2d(108.59942, -6.96806),
                new Vector2d(112.31037, -6.59021),
                new Vector2d(112.43904, -7.74599),
            };

            // ACT
            var meshData = roofBuilder.Build(building);

            // ASSERT
            Assert.IsNotNull(meshData);
        }

        [Test]
        public void CanBuildDome()
        {
            // ARRANGE
            var roofBuilder = new DomeRoofBuilder();
            InitializeRoofBuilder(roofBuilder);
            var building = CreateTestBuilding();
            building.Levels = 1;

            // ACT
            var meshDataList = roofBuilder.Build(building);

            // ASSERT
            Assert.IsNotNull(meshDataList);
            Assert.AreEqual(1, meshDataList.Count);
            Assert.AreEqual(1152 + 180, meshDataList[0].Vertices.Length);
            Assert.IsAssignableFrom(typeof(CompositeMeshIndex), meshDataList[0].Index);
        }

        [Test]
        public void CanBuildPyramidal()
        {
            // ARRANGE
            var roofBuilder = new PyramidalRoofBuilder();
            InitializeRoofBuilder(roofBuilder);

            // ACT
            var meshDataList = roofBuilder.Build(CreateTestBuilding());

            // ASSERT
            Assert.IsNotNull(meshDataList);
            Assert.AreEqual(1, meshDataList.Count);
            Assert.AreEqual(96, meshDataList[0].Vertices.Length);
            Assert.IsAssignableFrom(typeof(MultiPlaneMeshIndex), meshDataList[0].Index);
        }

        private Building CreateTestBuilding()
        {
            return new Building()
            {
                Footprint = new List<Vector2d>()
                {
                    new Vector2d(0, 0),
                    new Vector2d(0, 10),
                    new Vector2d(20, 10),
                    new Vector2d(20, 0),
                },
                Levels = 0,
                Elevation = 0,
                Height = 10,
                RoofHeight = 2,
                RoofColor = "gradient(#0eff94, #0deb88 50%, #07854d)",
                FloorFrontColor = "gradient(#f3e2c7, #c19e67 50%, #b68d4c 51%, #e9d4b3)",
                FloorBackColor = "gradient(#feffff, #d2ebf9)",
                RoofMaterial = "main",
                FacadeMaterial = "main",
                FacadeTexture = "concrete",
                RoofTexture = "concrete",
                FloorFrontTexture = "concrete",
                FloorBackTexture = "concrete"
            };
        }

        private void InitializeRoofBuilder(RoofBuilder roofBuilder)
        {
            roofBuilder.ObjectPool = TestHelper.GetObjectPool();
            roofBuilder.CustomizationService = TestHelper.GetCustomizationService();
            roofBuilder.Trace = new DefaultTrace();
        }
    }
}
