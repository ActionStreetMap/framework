using System.Collections.Generic;
using System.Linq;
using ActionStreetMap.Core.Geometry;
using ActionStreetMap.Core.Scene;
using ActionStreetMap.Explorer.Infrastructure;
using ActionStreetMap.Explorer.Scene.Indices;
using ActionStreetMap.Explorer.Scene.Roofs;
using ActionStreetMap.Tests.Explorer.Scene.Indices;
using NUnit.Framework;

namespace ActionStreetMap.Tests.Explorer.Buildings
{
    [TestFixture]
    public class RoofBuilderTests
    {
        [Test]
        public void CanBuildMansardWithValidData()
        {
            // ARRANGE
            var roofBuilder = new MansardRoofBuilder();
            roofBuilder.ObjectPool = TestHelper.GetObjectPool();
            roofBuilder.ResourceProvider = new UnityResourceProvider();

            // ACT
            var meshData = roofBuilder.Build(new Building()
            {
                Footprint = new List<Vector2d>()
                {
                    new Vector2d(0, 0),
                    new Vector2d(0, 5),
                    new Vector2d(5, 5),
                    new Vector2d(5, 0),
                },
                Elevation = 0,
                Height = 1,
                RoofColor = "gradient(#0eff94, #0deb88 50%, #07854d)"
            });

            // ASSERT
            Assert.AreEqual(10, meshData.First().Triangles.Length);
        }

        [Test]
        public void CanBuildGabled()
        {
            // ARRANGE
            var roofBuilder = new GabledRoofBuilder();
            roofBuilder.ResourceProvider = new UnityResourceProvider();
            roofBuilder.ObjectPool = TestHelper.GetObjectPool();
            // ACT
            var meshData = roofBuilder.Build(CreateTestBuilding());

            // ASSERT
            Assert.IsNotNull(meshData);
            Assert.AreEqual(72, meshData.First().Vertices.Length);
        }

        [Test]
        public void CanBuildHipped()
        {
            // ARRANGE
            var roofBuilder = new HippedRoofBuilder();
            roofBuilder.ObjectPool = TestHelper.GetObjectPool();
            roofBuilder.ResourceProvider = new UnityResourceProvider();
            // ACT
            var meshData = roofBuilder.Build(CreateTestBuilding());

            // ASSERT
            Assert.IsNotNull(meshData);
            Assert.AreEqual(72, meshData.First().Vertices.Length);
        }

        [Test]
        public void CanBuildDome()
        {
            // ARRANGE
            var roofBuilder = new DomeRoofBuilder();
            roofBuilder.ObjectPool = TestHelper.GetObjectPool();
            roofBuilder.ResourceProvider = new UnityResourceProvider();
            // ACT
            var meshDataList = roofBuilder.Build(CreateTestBuilding());

            // ASSERT
            Assert.IsNotNull(meshDataList);
            Assert.AreEqual(2, meshDataList.Count);
            Assert.AreEqual(960, meshDataList[0].Vertices.Length);
            Assert.IsAssignableFrom(typeof(SphereMeshIndex), meshDataList[0].Index);
        }

        [Test]
        public void CanBuildPyramidal()
        {
            // ARRANGE
            var roofBuilder = new PyramidalRoofBuilder();
            roofBuilder.ObjectPool = TestHelper.GetObjectPool();
            roofBuilder.ResourceProvider = new UnityResourceProvider();
            // ACT
            var meshDataList = roofBuilder.Build(CreateTestBuilding());

            // ASSERT
            Assert.IsNotNull(meshDataList);
            Assert.AreEqual(2, meshDataList.Count);
            Assert.AreEqual(48, meshDataList[0].Vertices.Length);
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
                Elevation = 0,
                Height = 10,
                RoofHeight = 2,
                RoofColor = "gradient(#0eff94, #0deb88 50%, #07854d)"
            };
        }
    }
}
