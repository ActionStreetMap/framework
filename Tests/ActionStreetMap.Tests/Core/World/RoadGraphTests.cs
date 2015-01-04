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
            var graph = new RoadGraph();
            var junctionPoint = new MapPoint(10, 0);
            var offset = RoadGraph.Offset;
            var roadElement1 = new RoadElement()
            {
                Id = 1,
                Points = new List<MapPoint>() { new MapPoint(0, 0), junctionPoint, new MapPoint(20, 0),}
            };

            var roadElement2 = new RoadElement()
            {
                Id = 2,
                Points = new List<MapPoint>() { new MapPoint(10, 10), junctionPoint, new MapPoint(10, -10)}
            };

            // ACT
            graph.Add(roadElement1);
            graph.Add(roadElement2);

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
            var elements = graph.Elements.ToList();
            Assert.AreEqual(4, elements.Count());

            /*Assert.AreEqual(2, elements[0].Points.Count);
            Assert.AreEqual(roadElement1.Points[0], elements[0].Points[0]);
            Assert.AreEqual(new MapPoint(junctionPoint.X - offset, 0), elements[0].Points[1]);

            Assert.AreEqual(2, elements[1].Points.Count);
            Assert.AreEqual(roadElement2.Points[0], elements[1].Points[0]);
            Assert.AreEqual(new MapPoint(10, junctionPoint.Y + offset), elements[1].Points[1]);

            Assert.AreEqual(2, elements[2].Points.Count);
            Assert.AreEqual(new MapPoint(junctionPoint.X + offset, 0), elements[2].Points[0]);
            Assert.AreEqual(new MapPoint(20, 0), elements[2].Points[1]);

            Assert.AreEqual(2, elements[3].Points.Count);
            Assert.AreEqual(new MapPoint(10, junctionPoint.Y - offset), elements[3].Points[0]);
            Assert.AreEqual(new MapPoint(10, -10), elements[3].Points[1]);*/
        }

        [Test]
        public void CanJoinSimpleRoadCorner()
        {
            var graph = new RoadGraph();
            var junctionPoint = new MapPoint(10, 0);
            var roadElement1 = new RoadElement()
            {
                Id = 1,
                Points = new List<MapPoint>() { new MapPoint(-10, 0), new MapPoint(0, 0), junctionPoint}
            };

            var roadElement2 = new RoadElement()
            {
                Id = 2,
                Points = new List<MapPoint>() { junctionPoint, new MapPoint(10, 10), new MapPoint(10, 20)}
            };

            // ACT
            graph.Add(roadElement1);
            graph.Add(roadElement2);

            // ASSERT
            var elements = graph.Elements.ToList();
            Assert.AreEqual(2, elements.Count());
        }

        [Test]
        public void CanSimpleJoinThreePoints()
        {
            var graph = new RoadGraph();
            var junctionPoint = new MapPoint(10, 0);
            var roadElement1 = new RoadElement()
            {
                Id = 1,
                Points = new List<MapPoint>() { new MapPoint(-10, 0), new MapPoint(0, 0), junctionPoint }
            };

            var roadElement2 = new RoadElement()
            {
                Id = 2,
                Points = new List<MapPoint>() { junctionPoint, new MapPoint(10, 10), new MapPoint(10, 20) }
            };

            var roadElement3 = new RoadElement()
            {
                Id = 3,
                Points = new List<MapPoint>() { new MapPoint(10, -20), new MapPoint(10, -10), junctionPoint }
            };

            // ACT
            graph.Add(roadElement1);
            graph.Add(roadElement2);
            graph.Add(roadElement3);

            // ASSERT
            var elements = graph.Elements.ToList();
            Assert.AreEqual(3, elements.Count());
        }

        [Test]
        public void CanJoinTwoCorners()
        {
            // ARRANGE
            var graph = new RoadGraph();

            // ACT
            graph.Add(new RoadElement { Points = new List<MapPoint>() { new MapPoint(0, 10), new MapPoint(0, 0) }});
            graph.Add(new RoadElement { Points = new List<MapPoint>() { new MapPoint(10, 10), new MapPoint(10, 0) }});
            graph.Add(new RoadElement { Points = new List<MapPoint>() { new MapPoint(0, 0), new MapPoint(10, 0) }});

            // ASSERT
            Assert.AreEqual(2, graph.Junctions.Count());
            Assert.AreEqual(3, graph.Elements.Count());
        }

        [Test]
        public void CanJoinSplitElementBug()
        {
            // ARRANGE
            var graph = new RoadGraph();

            // ACT
            graph.Add(new RoadElement { Points = new List<MapPoint>() 
                { new MapPoint(-10, 0), new MapPoint(0, 0), new MapPoint(10, 0) }});

            graph.Add(new RoadElement { Points = 
                new List<MapPoint>() { new MapPoint(0, 20), new MapPoint(0, 10), new MapPoint(0, 0) }});

            graph.Add(new RoadElement { Points = 
                new List<MapPoint>() { new MapPoint(10, 20), new MapPoint(10, 10), new MapPoint(10, 0) }});
          
            // ASSERT
            var elements = graph.Elements.ToList();
            Assert.AreEqual(4, elements.Count);
        }
    }
}
