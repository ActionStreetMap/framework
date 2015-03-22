using System.Collections.Generic;
using ActionStreetMap.Core;
using ActionStreetMap.Core.Scene.Buildings;
using ActionStreetMap.Core.Unity;
using ActionStreetMap.Explorer.Infrastructure;
using ActionStreetMap.Explorer.Scene.Buildings.Roofs;
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
            roofBuilder.ObjectPool = new ObjectPool();
            roofBuilder.ResourceProvider = new UnityResourceProvider();

            // ACT
            var meshData = roofBuilder.Build(new Building()
            {
                Footprint = new List<MapPoint>()
                {
                    new MapPoint(0, 0),
                    new MapPoint(0, 5),
                    new MapPoint(5, 5),
                    new MapPoint(5, 0),
                },
                Elevation = 0,
                Height = 1,
                RoofColor = "gradient(#0eff94, #0deb88 50%, #07854d)"
            });

            // ASSERT
            Assert.AreEqual(20, meshData.Vertices.Count);
            Assert.AreEqual(30, meshData.Triangles.Count);
            Assert.AreEqual(20, meshData.Colors.Count);
        }

        [Test]
        public void CanBuildGabled()
        {
            // ARRANGE
            var roofBuilder = new GabledRoofBuilder();
            roofBuilder.ResourceProvider = new UnityResourceProvider();
            roofBuilder.ObjectPool = new ObjectPool();
            // ACT
            var meshData = roofBuilder.Build(new Building()
            {
                Footprint = new List<MapPoint>()
                {
                    new MapPoint(0, 0),
                    new MapPoint(0, 10),
                    new MapPoint(20, 10),
                    new MapPoint(20, 0),
                },
                Elevation = 0,
                Height = 10,
                RoofHeight = 2,
                RoofColor = "gradient(#0eff94, #0deb88 50%, #07854d)"
            });

            // ASSERT
            Assert.IsNotNull(meshData);
            Assert.AreEqual(14, meshData.Vertices.Count);
            Assert.AreEqual(18, meshData.Triangles.Count);
            Assert.AreEqual(14, meshData.Colors.Count);
        }

        [Test]
        public void CanBuildHipped()
        {
            // ARRANGE
            var roofBuilder = new HippedRoofBuilder();
            roofBuilder.ResourceProvider = new UnityResourceProvider();
            // ACT
            var meshData = roofBuilder.Build(new Building()
            {
                Footprint = new List<MapPoint>()
                {
                    new MapPoint(0, 0),
                    new MapPoint(0, 10),
                    new MapPoint(20, 10),
                    new MapPoint(20, 0),
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
