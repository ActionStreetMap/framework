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
            var offset = 0;
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

        [TestCase(0, 1, 2)]
        [TestCase(0, 2, 1)]
        [TestCase(1, 0, 2)]
        [TestCase(1, 2, 0)]
        [TestCase(2, 1, 0)]
        [TestCase(2, 0, 1)]
        public void CanMergeTwoRoadsWithReversedPointOrder(int first, int second, int third)
        {
            // ARRANGE
            var builder = GetBuilder();
            var elements = new List<RoadElement>() {
                new RoadElement()
                {
                    Id = 0,
                    Type = RoadType.Car,
                    Points = new List<MapPoint>() {new MapPoint(-10, 0), new MapPoint(0, 0),}
                },
                new RoadElement()
                {
                    Id = 1,
                    Type = RoadType.Car,
                    Points = new List<MapPoint>() {new MapPoint(-40, 0), new MapPoint(-50, 0), new MapPoint(-60, 0)}
                },

                 new RoadElement()
                {
                    Id = 2,
                    Type = RoadType.Car,
                    Points = new List<MapPoint>() { new MapPoint(-40, 0), new MapPoint(-30, 0), new MapPoint(-20, 0), new MapPoint(-10, 0) }
                }   
            };

            builder.Add(elements.Single(e => e.Id == first));
            builder.Add(elements.Single(e => e.Id == second));
            builder.Add(elements.Single(e => e.Id == third));

            // ACT
            var graph = builder.Build();

            // ASSERT
            Assert.AreEqual(0, graph.Junctions.Count());
            Assert.AreEqual(1, graph.Roads.Count());
            var road = graph.Roads.First();
            Assert.AreEqual(3, road.Elements.Count);

            // reversed order case
            if (road.Elements[0].Points[0] == new MapPoint(0, 0))
            {
                road.Elements.Reverse();
                road.Elements.ForEach(e => e.Points.Reverse());
            }

            Assert.AreEqual(new MapPoint(-60, 0), road.Elements[0].Points[0]);
            Assert.AreEqual(new MapPoint(-50, 0), road.Elements[0].Points[1]);
            Assert.AreEqual(new MapPoint(-40, 0), road.Elements[0].Points[2]);
            Assert.AreEqual(new MapPoint(-40, 0), road.Elements[1].Points[0]);
            Assert.AreEqual(new MapPoint(-30, 0), road.Elements[1].Points[1]);
            Assert.AreEqual(new MapPoint(-20, 0), road.Elements[1].Points[2]);
            Assert.AreEqual(new MapPoint(-10, 0), road.Elements[1].Points[3]);
            Assert.AreEqual(new MapPoint(-10, 0), road.Elements[2].Points[0]);
            Assert.AreEqual(new MapPoint(0, 0), road.Elements[2].Points[1]);
        }

        [TestCase(0, 1, 2, 3)]
        [TestCase(1, 0, 2, 3)]
        [TestCase(0, 1, 3, 2)]
        [TestCase(2, 1, 0, 3)]
        [TestCase(3, 1, 2, 0)]
        [TestCase(3, 2, 1, 0)]
        public void CanMergeCircularListWithStartJunction(int first, int second, int third, int forth)
        {
            // ARRANGE
            var builder = GetBuilder();
            var elements = new List<RoadElement>()
            {
                new RoadElement()
                {
                    Id = 0,
                    Type = RoadType.Car,
                    Points = new List<MapPoint>() {new MapPoint(0, 0), new MapPoint(-10, 0),}
                },
                new RoadElement()
                {
                    Id = 1,
                    Type = RoadType.Car,
                    Points = new List<MapPoint>() {new MapPoint(-10, 0), new MapPoint(0, 10),}
                },
                new RoadElement()
                {
                    Id = 2,
                    Type = RoadType.Car,
                    Points = new List<MapPoint>() {new MapPoint(0, 0), new MapPoint(10, 0),}
                },
                new RoadElement()
                {
                    Id = 3,
                    Type = RoadType.Car,
                    Points = new List<MapPoint>() {new MapPoint(10, 0), new MapPoint(0, 10),}
                },
            };

            builder.Add(elements.Single(e => e.Id == first));
            builder.Add(elements.Single(e => e.Id == second));
            builder.Add(elements.Single(e => e.Id == third));
            builder.Add(elements.Single(e => e.Id == forth));

            // ACT
            var graph = builder.Build();

            // ASSERT
            Assert.AreEqual(0, graph.Junctions.Count());
            Assert.AreEqual(1, graph.Roads.Count());
        }

        [TestCase(0, 1, 2, 3)]
        [TestCase(0, 1, 3, 2)]
        [TestCase(0, 3, 1, 2)]
        [TestCase(0, 2, 1, 3)]
        [TestCase(1, 0, 2, 3)]
        [TestCase(1, 3, 2, 0)]
        [TestCase(1, 2, 0, 3)]
        [TestCase(1, 0, 3, 2)]
        [TestCase(2, 1, 0, 3)]
        [TestCase(2, 3, 0, 1)]
        [TestCase(2, 0, 1, 3)]
        [TestCase(2, 1, 3, 0)]
        [TestCase(3, 1, 2, 0)]
        [TestCase(3, 2, 1, 0)]
        [TestCase(3, 0, 2, 1)]
        [TestCase(3, 2, 0, 1)]
        public void CanSplitElementIntersectSelfAndOthers(int first, int second, int third, int forth)
        {
            // ARRANGE
            var builder = GetBuilder();
            var elements = new List<RoadElement>()
            {
                new RoadElement(){
                Id = 0,
                Type = RoadType.Car,
                Points = new List<MapPoint>()
                    {
                        new MapPoint(0, 0), new MapPoint(10, 0), new MapPoint(10, 10), new MapPoint(0, 10),
                        new MapPoint(0, 0), new MapPoint(0, -10), new MapPoint(0, -20),
                        new MapPoint(0, -30), new MapPoint(0, -40)
                    }
                },
                new RoadElement()
                {
                    Id = 1,
                    Type = RoadType.Car,
                    Points = new List<MapPoint>() { new MapPoint(10, -10), new MapPoint(0, -10) }
                },
                new RoadElement()
                {
                    Id = 2,
                    Type = RoadType.Car,
                    Points = new List<MapPoint>() { new MapPoint(10, -20), new MapPoint(0, -20) }
                },
                new RoadElement()
                {
                    Id = 3,
                    Type = RoadType.Car,
                    Points = new List<MapPoint>() { new MapPoint(10, -30), new MapPoint(0, -30) }
                }
            };

            builder.Add(elements.Single(e => e.Id == first));
            builder.Add(elements.Single(e => e.Id == second));
            builder.Add(elements.Single(e => e.Id == third));
            builder.Add(elements.Single(e => e.Id == forth));

            // ACT
            var graph = builder.Build();

            // ASSERT
            Assert.AreEqual(4, graph.Roads.Count());
            Assert.AreEqual(3, graph.Junctions.Count());

            var roads = graph.Roads.ToList();
            var testRoad = roads.Single(r => r.Elements[0].Id == 0);
            AssertElementSequence(testRoad.Elements);
        }

        private void AssertElementSequence(List<RoadElement> elements)
        {
            for (int i = 0; i < elements.Count; i++)
            {
                if (i == elements.Count - 1)
                    break;
                Assert.AreEqual(elements[i].Points[elements[i].Points.Count - 1], elements[i + 1].Points[0]);
            }
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
