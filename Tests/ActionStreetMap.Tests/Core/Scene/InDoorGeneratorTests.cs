using System.Collections.Generic;
using ActionStreetMap.Core;
using ActionStreetMap.Core.Geometry;
using ActionStreetMap.Core.Scene.InDoor;
using NUnit.Framework;

namespace ActionStreetMap.Tests.Core.Scene
{
    [TestFixture]
    public class InDoorGeneratorTests
    {
        private readonly InDoorGeneratorSettings _settings = new InDoorGeneratorSettings(10)
        {
            VaaSizeHeight = 20,
            VaaSizeWidth = 40,
            PreferedWidthStep = 100,
            MinimalWidthStep = 10,
        };

        [Test]
        public void CanGenerateFloorFromFootprint1()
        {
            // ARRANGE
            var footprint = new List<MapPoint>()
            {
                new MapPoint(200, 200),
                new MapPoint(200, 100),
                new MapPoint(600, 100),
                new MapPoint(600, 400),
                new MapPoint(500, 400),
                new MapPoint(500, 200),
                new MapPoint(200, 200),
            };
            var doors = new List<KeyValuePair<int, float>>() {new KeyValuePair<int, float>(1, 30)};

            // ACT
            var floor = InDoorGenerator.Build(_settings, footprint, doors, new List<List<MapPoint>>());

            // ASSERT
            AssertFloor(floor, 14, 13, 20, 25);
        }

        [Test]
        public void CanGenerateFloorFromFootprint2()
        {
            // ARRANGE
            var footprint = new List<MapPoint>()
            {
                new MapPoint(100, 80),
                new MapPoint(100, 400),
                new MapPoint(500, 400),
                new MapPoint(500, 250),
                new MapPoint(500, 100),
                new MapPoint(100, 80),
            };
            var doors = new List<KeyValuePair<int, float>>() { new KeyValuePair<int, float>(1, 30) };

            // ACT
            var floor = InDoorGenerator.Build(_settings, footprint, doors, new List<List<MapPoint>>());

            // ASSERT
            AssertFloor(floor, 5, 4, 9, 15);
        }

        [Test]
        public void CanGenerateFloorFromFootprint3()
        {
            var footprint = new List<MapPoint>()
            {
                new MapPoint(80, 72),
                new MapPoint(83, 327),
                new MapPoint(643, 320),
                new MapPoint(633, 60),
                new MapPoint(507, 56),
                new MapPoint(509, 203),
                new MapPoint(394, 203),
                new MapPoint(391, 120),
                new MapPoint(288, 115),
                new MapPoint(290, 170),
                new MapPoint(151, 171),
                new MapPoint(154, 60),
                new MapPoint(80, 72),
            };
            var doors = new List<KeyValuePair<int, float>>() { new KeyValuePair<int, float>(2, 30) };

            // ACT
            var floor = InDoorGenerator.Build(_settings, footprint, doors, new List<List<MapPoint>>());

            // ASSERT
            AssertFloor(floor);
        }

        [Test]
        public void CanGenerateFloorFromFootprint4()
        {
            var footprint = new List<MapPoint>()
            {
                new MapPoint(99, 79),
                new MapPoint(94, 341),
                new MapPoint(712, 337),
                new MapPoint(700, 67),
                new MapPoint(603, 66),
                new MapPoint(600, 246),
                new MapPoint(472, 241),
                new MapPoint(463, 133),
                new MapPoint(361, 119),
                new MapPoint(358, 230),
                new MapPoint(175, 229),
                new MapPoint(180, 78),
                new MapPoint(99, 79)
            };
            var doors = new List<KeyValuePair<int, float>>() { new KeyValuePair<int, float>(2, 30) };

            // ACT
            var floor = InDoorGenerator.Build(_settings, footprint, doors, new List<List<MapPoint>>());

            // ASSERT
            AssertFloor(floor);
        }

        [Test]
        public void CanGenerateFloorFromFootprint5()
        {
            var footprint = new List<MapPoint>()
            {
                new MapPoint(100, 66),
                new MapPoint(93, 270),
                new MapPoint(338, 238),
                new MapPoint(331, 420),
                new MapPoint(521, 440),
                new MapPoint(690, 199),
                new MapPoint(633, 32),
                new MapPoint(435, 40),
                new MapPoint(290, 7),
                new MapPoint(100, 66),
            };
            var doors = new List<KeyValuePair<int, float>>() { new KeyValuePair<int, float>(3, 30) };

            // ACT
            var floor = InDoorGenerator.Build(_settings, footprint, doors, new List<List<MapPoint>>());

            // ASSERT
            AssertFloor(floor);
        }

        [Test]
        public void CanGenerateFloorFromFootprint6()
        {
            var footprint = new List<MapPoint>()
            {
                new MapPoint(223, 107),
                new MapPoint(79, 220),
                new MapPoint(242, 308),
                new MapPoint(391, 218),
                new MapPoint(530, 346),
                new MapPoint(587, 186),
                new MapPoint(543, 0),
                new MapPoint(373, 21),
                new MapPoint(223, 107),
            };
            var doors = new List<KeyValuePair<int, float>>() { new KeyValuePair<int, float>(1, 30) };

            // ACT
            var floor = InDoorGenerator.Build(_settings, footprint, doors, new List<List<MapPoint>>());

            // ASSERT
            AssertFloor(floor);
        }

        [Test]
        public void CanGenerateFloorFromFootprint7()
        {
            var footprint = new List<MapPoint>()
            {
                new MapPoint(299, 133),
                new MapPoint(99, 283),
                new MapPoint(227, 554),
                new MapPoint(832, 575),
                new MapPoint(971, 250),
                new MapPoint(792, 61),
                new MapPoint(299, 133),
            };
            var doors = new List<KeyValuePair<int, float>>() { new KeyValuePair<int, float>(1, 30) };

            // ACT
            var floor = InDoorGenerator.Build(_settings, footprint, doors, new List<List<MapPoint>>());

            // ASSERT
            AssertFloor(floor);
        }

        [Test]
        public void CanGenerateFloorFromFootprint8()
        {
            var footprint = new List<MapPoint>()
            {
                new MapPoint(107, 62),
                new MapPoint(284, 62),
                new MapPoint(291, 322),
                new MapPoint(464, 320),
                new MapPoint(464, 168),
                new MapPoint(616, 162),
                new MapPoint(618, 309),
                new MapPoint(814, 308),
                new MapPoint(808, 40),
                new MapPoint(1003, 36),
                new MapPoint(1003, 495),
                new MapPoint(833, 503),
                new MapPoint(837, 599),
                new MapPoint(616, 604),
                new MapPoint(618, 511),
                new MapPoint(483, 509),
                new MapPoint(480, 600),
                new MapPoint(103, 601),
                new MapPoint(107, 62),
            };
            var doors = new List<KeyValuePair<int, float>>() { new KeyValuePair<int, float>(1, 30) };

            // ACT
            var floor = InDoorGenerator.Build(_settings, footprint, doors, new List<List<MapPoint>>());

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
