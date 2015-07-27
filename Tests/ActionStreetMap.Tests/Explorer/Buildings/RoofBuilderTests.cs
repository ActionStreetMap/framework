using System.Collections.Generic;
using System.Linq;
using ActionStreetMap.Core;
using ActionStreetMap.Core.Geometry;
using ActionStreetMap.Core.Scene;
using ActionStreetMap.Core.Unity;
using ActionStreetMap.Explorer.Infrastructure;
using ActionStreetMap.Explorer.Scene.Roofs;
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
            var meshData = roofBuilder.Build(new Building()
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
            });

            // ASSERT
            Assert.IsNotNull(meshData);
            Assert.AreEqual(6, meshData.First().Triangles.Length);
        }

        [Test]
        public void CanBuildHipped()
        {
            // ARRANGE
            var roofBuilder = new HippedRoofBuilder();
            roofBuilder.ObjectPool = TestHelper.GetObjectPool();
            roofBuilder.ResourceProvider = new UnityResourceProvider();
            // ACT
            var meshData = roofBuilder.Build(new Building()
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
            });

            // ASSERT
            Assert.IsNotNull(meshData);
        }
    }
}
