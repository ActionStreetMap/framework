using System.Collections.Generic;
using ActionStreetMap.Core;
using ActionStreetMap.Core.Scene.Buildings;
using ActionStreetMap.Explorer.Infrastructure;
using ActionStreetMap.Explorer.Scene.Buildings;
using ActionStreetMap.Explorer.Scene.Buildings.Roofs;
using NUnit.Framework;
using UnityEngine;
using Rect = ActionStreetMap.Explorer.Geometry.Primitives.Rect;

namespace ActionStreetMap.Tests.Explorer.Buildings
{
    [TestFixture]
    public class RoofBuilderTests
    {
        [Test]
        public void CanBuildMansardWithValidData()
        {
            // ARRANGE
            var builder = new MansardRoofBuilder();
            builder.ObjectPool = new ObjectPool();
            // ACT
            var meshData = builder.Build(new Building()
            {
                Footprint = new List<MapPoint>()
                {
                    new MapPoint(0, 0),
                    new MapPoint(0, 5),
                    new MapPoint(5, 5),
                    new MapPoint(5, 0),
                },
                Elevation = 0,
                Height = 1
            }, new BuildingStyle()
            {
                Roof = new BuildingStyle.RoofStyle()
                {
                    Builders = new IRoofBuilder[] { new FlatRoofBuilder(), },
                    FrontUvMap = new Rect(new Vector2(), new Vector2()),
                    Material = "",
                },
                Facade = new BuildingStyle.FacadeStyle()
            });

            // ASSERT

            Assert.AreEqual(20, meshData.Vertices.Length);
            Assert.AreEqual(30, meshData.Triangles.Length);
            Assert.AreEqual(20, meshData.UV.Length);
        }

        [Test]
        public void CanBuildGabled()
        {
            // ARRANGE
            var roofBuilder = new GabledRoofBuilder(new ObjectPool());

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
                RoofHeight = 2
            }, new BuildingStyle()
            {
                Roof = new BuildingStyle.RoofStyle()
                {
                    FrontUvMap = new Rect(new Vector2(), new Vector2())
                }
            });

            // ASSERT
            Assert.IsNotNull(meshData);
            Assert.AreEqual(14, meshData.Vertices.Length);
            Assert.AreEqual(18, meshData.Triangles.Length);
            Assert.AreEqual(14, meshData.UV.Length);
        }

        [Test]
        public void CanBuildHipped()
        {
            // ARRANGE
            var roofBuilder = new HippedRoofBuilder();

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
                RoofHeight = 2
            }, new BuildingStyle()
            {
                Roof = new BuildingStyle.RoofStyle()
                {
                    FrontUvMap = new Rect(new Vector2(), new Vector2())
                }
            });

            // ASSERT
            Assert.IsNotNull(meshData);
        }
    }
}
