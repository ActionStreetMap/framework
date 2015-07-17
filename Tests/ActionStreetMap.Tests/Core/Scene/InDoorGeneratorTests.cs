using System.Collections.Generic;
using ActionStreetMap.Core;
using ActionStreetMap.Core.Geometry;
using ActionStreetMap.Core.Geometry.Clipping;
using ActionStreetMap.Core.Geometry.StraightSkeleton;
using ActionStreetMap.Core.Scene.InDoor;
using ActionStreetMap.Explorer.Infrastructure;
using ActionStreetMap.Infrastructure.Utilities;
using NUnit.Framework;

namespace ActionStreetMap.Tests.Core.Scene
{
    [TestFixture]
    public class InDoorGeneratorTests
    {
        private InDoorGenerator _generator;
        private InDoorGeneratorSettings _settings;

        [SetUp]
        public void SetUp()
        {
            var objectPool = new ObjectPool()
              .RegisterListType<Vector2d>(16)
              .RegisterListType<MapLine>(16)
              .RegisterListType<int>(16)
              .RegisterListType<Apartment>(16);

            _generator = new InDoorGenerator();
            _settings = new InDoorGeneratorSettings(objectPool, new Clipper(), 10)
            {
                VaaSizeHeight = 20,
                VaaSizeWidth = 40,
                PreferedWidthStep = 100,
                MinimalWidthStep = 10,
            };
        }

        [Test]
        public void CanGenerateFloorFromFootprint1()
        {
            // ARRANGE
            var footprint = new List<Vector2d>()
            {
                new Vector2d(200, 200),
                new Vector2d(200, 100),
                new Vector2d(600, 100),
                new Vector2d(600, 400),
                new Vector2d(500, 400),
                new Vector2d(500, 200),
            };
            var doors = new List<KeyValuePair<int, float>>() {new KeyValuePair<int, float>(1, 30)};
            var skeleton = SkeletonBuilder.Build(footprint);

            // ACT
            var floor = _generator.Build(_settings, skeleton, footprint, 
                new List<List<Vector2d>>(), doors);

            // ASSERT
            AssertFloor(floor, 14, 13, 20, 25);
        }

        [Test]
        public void CanGenerateFloorFromFootprint2()
        {
            // ARRANGE
            var footprint = new List<Vector2d>()
            {
                new Vector2d(100, 80),
                new Vector2d(100, 400),
                new Vector2d(500, 400),
                new Vector2d(500, 250),
                new Vector2d(500, 100),
            };
            var doors = new List<KeyValuePair<int, float>>() { new KeyValuePair<int, float>(1, 30) };
            var skeleton = SkeletonBuilder.Build(footprint);

            // ACT
            var floor = _generator.Build(_settings, skeleton, footprint,
                new List<List<Vector2d>>(), doors);

            // ASSERT
            AssertFloor(floor, 5, 4, 9, 15);
        }

        [Test]
        public void CanGenerateFloorFromFootprint3()
        {
            var footprint = new List<Vector2d>()
            {
                new Vector2d(80, 72),
                new Vector2d(83, 327),
                new Vector2d(643, 320),
                new Vector2d(633, 60),
                new Vector2d(507, 56),
                new Vector2d(509, 203),
                new Vector2d(394, 203),
                new Vector2d(391, 120),
                new Vector2d(288, 115),
                new Vector2d(290, 170),
                new Vector2d(151, 171),
                new Vector2d(154, 60),
            };
            var doors = new List<KeyValuePair<int, float>>() { new KeyValuePair<int, float>(2, 30) };
            var skeleton = SkeletonBuilder.Build(footprint);

            // ACT
            var floor = _generator.Build(_settings, skeleton, footprint,
                new List<List<Vector2d>>(), doors);

            // ASSERT
            AssertFloor(floor);
        }

        [Test]
        public void CanGenerateFloorFromFootprint4()
        {
            var footprint = new List<Vector2d>()
            {
                new Vector2d(99, 79),
                new Vector2d(94, 341),
                new Vector2d(712, 337),
                new Vector2d(700, 67),
                new Vector2d(603, 66),
                new Vector2d(600, 246),
                new Vector2d(472, 241),
                new Vector2d(463, 133),
                new Vector2d(361, 119),
                new Vector2d(358, 230),
                new Vector2d(175, 229),
                new Vector2d(180, 78),
            };
            var doors = new List<KeyValuePair<int, float>>() { new KeyValuePair<int, float>(2, 30) };
            var skeleton = SkeletonBuilder.Build(footprint);

            // ACT
            var floor = _generator.Build(_settings, skeleton, footprint,
                new List<List<Vector2d>>(), doors);

            // ASSERT
            AssertFloor(floor);
        }

        [Test]
        public void CanGenerateFloorFromFootprint5()
        {
            var footprint = new List<Vector2d>()
            {
                new Vector2d(100, 66),
                new Vector2d(93, 270),
                new Vector2d(338, 238),
                new Vector2d(331, 420),
                new Vector2d(521, 440),
                new Vector2d(690, 199),
                new Vector2d(633, 32),
                new Vector2d(435, 40),
                new Vector2d(290, 7),
            };
            var doors = new List<KeyValuePair<int, float>>() { new KeyValuePair<int, float>(3, 30) };
            var skeleton = SkeletonBuilder.Build(footprint);

            // ACT
            var floor = _generator.Build(_settings, skeleton, footprint,
                new List<List<Vector2d>>(), doors);

            // ASSERT
            AssertFloor(floor);
        }

        [Test]
        public void CanGenerateFloorFromFootprint6()
        {
            var footprint = new List<Vector2d>()
            {
                new Vector2d(223, 107),
                new Vector2d(79, 220),
                new Vector2d(242, 308),
                new Vector2d(391, 218),
                new Vector2d(530, 346),
                new Vector2d(587, 186),
                new Vector2d(543, 0),
                new Vector2d(373, 21),
            };
            var doors = new List<KeyValuePair<int, float>>() { new KeyValuePair<int, float>(1, 30) };
            var skeleton = SkeletonBuilder.Build(footprint);

            // ACT
            var floor = _generator.Build(_settings, skeleton, footprint,
                new List<List<Vector2d>>(), doors);

            // ASSERT
            AssertFloor(floor);
        }

        [Test]
        public void CanGenerateFloorFromFootprint7()
        {
            var footprint = new List<Vector2d>()
            {
                new Vector2d(299, 133),
                new Vector2d(99, 283),
                new Vector2d(227, 554),
                new Vector2d(832, 575),
                new Vector2d(971, 250),
                new Vector2d(792, 61),
            };
            var doors = new List<KeyValuePair<int, float>>() { new KeyValuePair<int, float>(1, 30) };
            var skeleton = SkeletonBuilder.Build(footprint);

            // ACT
            var floor = _generator.Build(_settings, skeleton, footprint,
                new List<List<Vector2d>>(), doors);

            // ASSERT
            AssertFloor(floor);
        }

        [Test]
        public void CanGenerateFloorFromFootprint8()
        {
            var footprint = new List<Vector2d>()
            {
                new Vector2d(107, 62),
                new Vector2d(284, 62),
                new Vector2d(291, 322),
                new Vector2d(464, 320),
                new Vector2d(464, 168),
                new Vector2d(616, 162),
                new Vector2d(618, 309),
                new Vector2d(814, 308),
                new Vector2d(808, 40),
                new Vector2d(1003, 36),
                new Vector2d(1003, 495),
                new Vector2d(833, 503),
                new Vector2d(837, 599),
                new Vector2d(616, 604),
                new Vector2d(618, 511),
                new Vector2d(483, 509),
                new Vector2d(480, 600),
                new Vector2d(103, 601),
            };
            var doors = new List<KeyValuePair<int, float>>() { new KeyValuePair<int, float>(1, 30) };
            var skeleton = SkeletonBuilder.Build(footprint);

            // ACT
            var floor = _generator.Build(_settings, skeleton, footprint,
                new List<List<Vector2d>>(), doors);

            // ASSERT
            AssertFloor(floor);
        }

        private void AssertFloor(Floor floor, int apartments, int partitionWalls,
            int outerWalls, int transitWalls)
        {
            CheckThatPolyLineIsConnected(floor.OuterWalls);
            CheckThatPolyLineIsConnected(floor.TransitWalls);

            Assert.AreEqual(apartments, floor.Apartments.Count);
            Assert.AreEqual(partitionWalls, floor.PartitionWalls.Count);
            Assert.AreEqual(outerWalls, floor.OuterWalls.Count);
            Assert.AreEqual(transitWalls, floor.TransitWalls.Count);
        }

        private void AssertFloor(Floor floor)
        {
            CheckThatPolyLineIsConnected(floor.OuterWalls);
            CheckThatPolyLineIsConnected(floor.TransitWalls);

            Assert.AreNotEqual(0, floor.Apartments.Count);
            Assert.AreNotEqual(0,floor.PartitionWalls.Count);
            Assert.AreNotEqual(0, floor.OuterWalls.Count);
            Assert.AreNotEqual(0, floor.TransitWalls.Count);
        }

        private void CheckThatPolyLineIsConnected(List<MapLine> polyLine)
        {
            var lastPoint = Vector2d.Empty;
            for (int i = 0; i < polyLine.Count - 1; i++)
            {
                var line = polyLine[i];

                Assert.AreNotEqual(Vector2d.Empty, line.Start);
                Assert.AreNotEqual(Vector2d.Empty, line.End);

                if (lastPoint != Vector2d.Empty)
                    Assert.AreEqual(lastPoint, line.Start);
                
                lastPoint = line.End;
            }
        }
    }
}
