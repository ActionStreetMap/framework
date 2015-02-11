using System;
using System.Collections.Generic;
using System.Linq;
using ActionStreetMap.Core;
using ActionStreetMap.Core.Scene.Roads;
using ActionStreetMap.Explorer.Infrastructure;
using NUnit.Framework;

namespace ActionStreetMap.Tests.Core.Scene
{
    [TestFixture]
    public class RoadGraphBugFixTests
    {
        [Test]
        public void CanCorrectDetectJunctions()
        {
            // ARRANGE
            var builder = new RoadGraphBuilder();
            builder.Add(new RoadElement()
            {
                Id = 241886108,
                Width = 4,
                Type = RoadType.Car,
                Points = new List<MapPoint>() 
                { 
                    new MapPoint(-271.7f, -13.5f,42.9f),
                    new MapPoint(-242.2f, -52.4f,43.9f),
                    new MapPoint(-192.5f, -118.2f,43.9f),
                    new MapPoint(-186.9f, -125.1f,43.9f),
                    new MapPoint(-154.5f, -167.7f,43.9f),
                    new MapPoint(-152.4f, -170.2f,43.9f),
                    new MapPoint(-138.2f, -188.9f,43.9f),
                    new MapPoint(-124.5f, -206.1f,43.9f),
                    new MapPoint(-72.6f, -278.2f,43.9f),
                    new MapPoint(-47.2f, -312.8f,43.9f),
                }
            });

            builder.Add(new RoadElement()
            {
                Id = 172078684,
                Width = 6,
                Type = RoadType.Car,
                Points = new List<MapPoint>() 
                { 
                    new MapPoint(224.3f, -44.5f,42.9f),
                    new MapPoint(144.7f, -74.3f,42.9f),
                    new MapPoint(38.7f, -116.9f,42.9f),
                    new MapPoint(-138.2f, -188.9f,43.9f),
                }
            });

            builder.Add(new RoadElement()
            {
                Id = 53257010,
                Width = 6,
                Type = RoadType.Car,
                Points = new List<MapPoint>() 
                { 
                    new MapPoint(-39.2f, 112.4f,40.2f),
                    new MapPoint(-36.8f, 104.8f,40.0f),
                    new MapPoint(-17.6f, 43.6f,39.3f),
                    new MapPoint(-2.0f, 0.0f,40.8f),
                    new MapPoint(38.7f, -116.9f,42.9f),
                }
            });

            builder.Add(new RoadElement()
            {
                Id = 304447794,
                Width = 1,
                Type = RoadType.Pedestrian,
                Points = new List<MapPoint>() 
                { 
                    new MapPoint(30.9f, 119.5f,38.4f),
                    new MapPoint(34.0f, 102.7f,38.4f),
                    new MapPoint(67.9f, 24.0f,39.8f),
                }
            });

            builder.Add(new RoadElement()
            {
                Id = 4597202,
                Width = 6,
                Type = RoadType.Car,
                Points = new List<MapPoint>() 
                { 
                    new MapPoint(-186.9f, -125.1f,43.9f),
                    new MapPoint(-86.5f, -57.1f,43.9f),
                    new MapPoint(-2.0f, 0.0f,40.8f),
                    new MapPoint(67.9f, 24.0f,39.8f),
                    new MapPoint(93.1f, 32.6f,39.4f),
                }
            });

            // ACT
            var graph = builder.Build(new ObjectPool());

            // ASSERT
            var roads = graph.Roads;
            var junctions = graph.Junctions;

            Assert.AreEqual(4, junctions.Count());

            // check connections count
            var junction1 = junctions.Single(j => Math.Abs(j.Center.X - (-186)) < 1);
            var junction2 = junctions.Single(j => Math.Abs(j.Center.X - (-138)) < 1);
            var junction3 = junctions.Single(j => Math.Abs(j.Center.X - 38) < 1);
            var junction4 = junctions.Single(j => Math.Abs(j.Center.X - (-2)) < 1);

            Assert.AreEqual(3, junction1.Connections.Count);
            Assert.IsNull(junction1.Connections[0].Start);
            Assert.IsTrue(junction1.Connections[0].End == junction1);
            Assert.IsTrue(junction1.Connections[1].Start == junction1);
            Assert.IsTrue(junction1.Connections[1].End == junction2);
            Assert.IsTrue(junction1.Connections[2].Start == junction1);
            Assert.IsTrue(junction1.Connections[2].End == junction4);

            Assert.AreEqual(3, junction2.Connections.Count);
            Assert.IsTrue(junction2.Connections[0] == junction1.Connections[1]);
            Assert.IsTrue(junction2.Connections[1] != junction1.Connections[1]);

            Assert.AreEqual(3, junction3.Connections.Count);
            Assert.AreEqual(4, junction4.Connections.Count);
        }
    }
}
