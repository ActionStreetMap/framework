using System.Collections.Generic;
using System.Linq;
using ActionStreetMap.Core;
using ActionStreetMap.Core.Scene.World.Roads;

using NUnit.Framework;

namespace ActionStreetMap.Tests.Core.World
{
    [TestFixture]
    public class RoadGraphTests
    {
        [Test]
        public void CanJoinSimpleRoadCross()
        {
            // ARRANGE
            var builder = GetBuilder();
            var junctionPoint = new MapPoint(10, 0);
            var offset = 2;
            var roadElement1 = new RoadElement()
            {
                Id = 1,
                Width = offset,
                Type = RoadType.Car,
                Points = new List<MapPoint>() { new MapPoint(0, 0), junctionPoint, new MapPoint(20, 0), }
            };

            var roadElement2 = new RoadElement()
            {
                Id = 2,
                Width = offset,
                Type = RoadType.Car,
                Points = new List<MapPoint>() { new MapPoint(10, 10), junctionPoint, new MapPoint(10, -10) }
            };

            // ACT
            builder.Add(roadElement1);
            builder.Add(roadElement2);
            var graph = builder.Build();

            // ASSERT

            // check junction
            Assert.AreEqual(1, graph.Junctions.Count());
            Assert.AreEqual(junctionPoint, graph.Junctions.First().Center);
            var connections = graph.Junctions.First().Connections.ToList();
            Assert.AreEqual(4, connections.Count);
            Assert.AreEqual(new MapPoint(junctionPoint.X - offset, 0), connections[0].Point);
            Assert.AreEqual(new MapPoint(junctionPoint.X + offset, 0), connections[1].Point);
            Assert.AreEqual(new MapPoint(10, junctionPoint.Y + offset), connections[2].Point);
            Assert.AreEqual(new MapPoint(10, junctionPoint.Y - offset), connections[3].Point);

            // check elements
            var elements = GetElements(graph).ToList();
            Assert.AreEqual(4, elements.Count());
        }

        [Test]
        public void CanJoinSimpleRoadCorner()
        {
            // ARRANGE
            var builder = GetBuilder();
            var junctionPoint = new MapPoint(10, 0);
            var roadElement1 = new RoadElement()
            {
                Id = 1,
                Type = RoadType.Car,
                Points = new List<MapPoint>() { new MapPoint(-10, 0), new MapPoint(0, 0), junctionPoint }
            };

            var roadElement2 = new RoadElement()
            {
                Id = 2,
                Type = RoadType.Car,
                Points = new List<MapPoint>() { junctionPoint, new MapPoint(10, 10), new MapPoint(10, 20) }
            };

            // ACT
            builder.Add(roadElement1);
            builder.Add(roadElement2);
            var graph = builder.Build();

            // ASSERT
            var elements = GetElements(graph).ToList();
            Assert.AreEqual(2, elements.Count());
        }

        [Test]
        public void CanSimpleJoinThreePoints()
        {
            // ARRANGE
            var builder = GetBuilder();
            var junctionPoint = new MapPoint(10, 0);
            var roadElement1 = new RoadElement()
            {
                Id = 1,
                Type = RoadType.Car,
                Points = new List<MapPoint>() { new MapPoint(-10, 0), new MapPoint(0, 0), junctionPoint }
            };

            var roadElement2 = new RoadElement()
            {
                Id = 2,
                Type = RoadType.Car,
                Points = new List<MapPoint>() { junctionPoint, new MapPoint(10, 10), new MapPoint(10, 20) }
            };

            var roadElement3 = new RoadElement()
            {
                Id = 3,
                Type = RoadType.Car,
                Points = new List<MapPoint>() { new MapPoint(10, -20), new MapPoint(10, -10), junctionPoint }
            };

            // ACT
            builder.Add(roadElement1);
            builder.Add(roadElement2);
            builder.Add(roadElement3);
            var graph = builder.Build();

            // ASSERT
            var elements = GetElements(graph).ToList();
            Assert.AreEqual(3, elements.Count());
        }

        [Test]
        public void CanJoinTwoCorners()
        {
            // ARRANGE
            var builder = GetBuilder();

            // ACT
            builder.Add(new RoadElement
            {
                Id = 0,
                Type = RoadType.Car,
                Points = new List<MapPoint>() { new MapPoint(0, 10), new MapPoint(0, 0) }
            });
            builder.Add(new RoadElement
            {
                Id = 1,
                Type = RoadType.Car,
                Points = new List<MapPoint>() { new MapPoint(10, 10), new MapPoint(10, 0) }
            });
            builder.Add(new RoadElement
            {
                Id = 2,
                Type = RoadType.Car,
                Points = new List<MapPoint>() { new MapPoint(0, 0), new MapPoint(10, 0) }
            });
            var graph = builder.Build();

            // ASSERT
            Assert.AreEqual(0, graph.Junctions.Count());
            Assert.AreEqual(1, graph.Roads.Count());
            Assert.AreEqual(3, GetElements(graph).Count());
        }

        [Test]
        public void CanJoinSplitElementBug()
        {
            // ARRANGE
            var builder = GetBuilder();

            // ACT
            builder.Add(new RoadElement
            {
                Id = 0,
                Type = RoadType.Car,
                Points = new List<MapPoint>() { new MapPoint(-10, 0), new MapPoint(0, 0), new MapPoint(10, 0) }
            });

            builder.Add(new RoadElement
            {
                Id = 1,
                Type = RoadType.Car,
                Points =
                    new List<MapPoint>() { new MapPoint(0, 20), new MapPoint(0, 10), new MapPoint(0, 0) }
            });

            builder.Add(new RoadElement
            {
                Id = 2,
                Type = RoadType.Car,
                Points =
                    new List<MapPoint>() { new MapPoint(10, 20), new MapPoint(10, 10), new MapPoint(10, 0) }
            });
            var graph = builder.Build();

            // ASSERT
            var elements = GetElements(graph).ToList();
            Assert.AreEqual(4, elements.Count);
        }

        [Test]
        public void CanHandleRoadWithTheSamePoints()
        {
            // ARRANGE
            var builder = GetBuilder();

            // ACT
            builder.Add(new RoadElement
            {
                Id = 0,
                Type = RoadType.Car,
                Points = new List<MapPoint>() { new MapPoint(-10, 0), new MapPoint(0, 0), new MapPoint(10, 0), new MapPoint(0, 0) }
            });
            var graph = builder.Build();

            // ASSERT
            Assert.AreEqual(0, graph.Junctions.Count());
            Assert.AreEqual(1, GetElements(graph).Count());
        }

        [Test]
        public void CanSkipJunctionOfDifferentTypes()
        {
            // ARRANGE
            var builder = GetBuilder();

            // ACT
            builder.Add(new RoadElement()
            {
                Id = 0,
                Type = RoadType.Car,
                Points = new List<MapPoint>() { new MapPoint(-10, 0), new MapPoint(0, 0), new MapPoint(10, 0) }
            });

            builder.Add(new RoadElement()
            {
                Id = 1,
                Type = RoadType.Pedestrian,
                Points = new List<MapPoint>() { new MapPoint(0, 10), new MapPoint(0, 0), new MapPoint(0, -10) }
            });
            var graph = builder.Build();

            // ASSERT
            Assert.AreEqual(0, graph.Junctions.Count());
            Assert.AreEqual(2, graph.Roads.Count());
        }

        [Test]
        public void CanMergeTwoSameRoadsJunction()
        {
            // ARRANGE
            var builder = GetBuilder();
            builder.Add(new RoadElement()
            {
                Id = 0,
                Type = RoadType.Car,
                Points = new List<MapPoint>() { new MapPoint(-10, 0), new MapPoint(0, 0), new MapPoint(10, 0) }
            });
            builder.Add(new RoadElement()
            {
                Id = 1,
                Type = RoadType.Car,
                Points = new List<MapPoint>() { new MapPoint(10, 0), new MapPoint(20, 0), new MapPoint(30, 0) }
            });

            // ACT
            var graph = builder.Build();

            // ASSERT
            Assert.AreEqual(0, graph.Junctions.Count());
            Assert.AreEqual(1, graph.Roads.Count());
        }

        private static IEnumerable<RoadElement> GetElements(RoadGraph graph)
        {
            return graph.Roads.Select(r => r.Elements).SelectMany(e => e);
        }

        private static RoadGraphBuilder GetBuilder()
        {
            return new RoadGraphBuilder { Trace = new ConsoleTrace() };
        }
    }
}
