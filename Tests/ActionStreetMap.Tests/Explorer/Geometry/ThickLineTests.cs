using System;
using System.Collections.Generic;
using ActionStreetMap.Core;
using ActionStreetMap.Explorer.Geometry.ThickLine;
using ActionStreetMap.Explorer.Infrastructure;
using Moq;
using NUnit.Framework;

namespace ActionStreetMap.Tests.Explorer.Geometry
{
    [TestFixture]
    public class ThickLineTests
    {
        [Test]
        public void CanDetectDirection()
        {
            // ARRANGE & ACT & ASSERT
            Assert.AreEqual(ThickLineHelper.Direction.Left, ThickLineHelper.GetDirection(
                ThickLineHelper.GetThickSegment(new MapPoint(0, 0, 0), new MapPoint(3, 0, 0), 2),
                ThickLineHelper.GetThickSegment(new MapPoint(3, 0, 0), new MapPoint(6, 2, 0), 2)));

            Assert.AreEqual(ThickLineHelper.Direction.Right, ThickLineHelper.GetDirection(
                ThickLineHelper.GetThickSegment(new MapPoint(0, 0, 0), new MapPoint(3, 0, 0), 2),
                ThickLineHelper.GetThickSegment(new MapPoint(3, 0, 0), new MapPoint(6, -2, 0), 2)));

            Assert.AreEqual(ThickLineHelper.Direction.Straight, ThickLineHelper.GetDirection(
                ThickLineHelper.GetThickSegment(new MapPoint(0, 0, 0), new MapPoint(3, 0, 0), 2),
                ThickLineHelper.GetThickSegment(new MapPoint(3, 0, 0), new MapPoint(6, 0, 0), 2)));
        }

        [Test]
        public void CanGetLineElementsInTile()
        {
            // ARRANGE
            var leftBottomCorner = new MapPoint(0, 0);
            var rightUpperCorner = new MapPoint(100, 100);
            var lineElements = new List<LineElement>()
            {
                new LineElement(new List<MapPoint>()
                {
                    new MapPoint(5, 5), new MapPoint(10, 10),
                    new MapPoint(-10, 10), new MapPoint(-10, 20),
                    new MapPoint(20, 20), new MapPoint(30, 30),
                    new MapPoint(30, 130), new MapPoint(50, 130),
                    new MapPoint(50, 50)
                }, 5)
            };

            // ACT
            var result = new List<LineElement>();
            ThickLineUtils.GetLineElementsInTile(leftBottomCorner, rightUpperCorner, lineElements, result, new ObjectPool());

            // ASSERT
            Assert.AreEqual(3, result.Count);
            Assert.AreEqual(3, result[0].Points.Count);
            Assert.AreEqual(new MapPoint(5, 5), result[0].Points[0]);
            Assert.AreEqual(new MapPoint(10, 10), result[0].Points[1]);
            Assert.AreEqual(new MapPoint(0, 10), result[0].Points[2]);

            Assert.AreEqual(4, result[1].Points.Count);
            Assert.AreEqual(new MapPoint(0, 20), result[1].Points[0]);
            Assert.AreEqual(new MapPoint(20, 20), result[1].Points[1]);
            Assert.AreEqual(new MapPoint(30, 30), result[1].Points[2]);
            Assert.AreEqual(new MapPoint(30, 100), result[1].Points[3]);

            Assert.AreEqual(2, result[2].Points.Count);
            Assert.AreEqual(new MapPoint(50, 100), result[2].Points[0]);
            Assert.AreEqual(new MapPoint(50, 50), result[2].Points[1]);
        }

        [Test]
        public void CanMergeTwoElementsWithSharedPoint()
        {
            // ARRANGE
            var leftBottomCorner = new MapPoint(0, 0);
            var rightUpperCorner = new MapPoint(100, 100);
            var lineElements = new List<LineElement>()
            {
                new LineElement(new List<MapPoint>()
                {
                    new MapPoint(5, 5), new MapPoint(10, 10),
                }, 5),
                new LineElement(new List<MapPoint>()
                {
                    new MapPoint(10, 10), new MapPoint(0, 20),
                }, 5)
            };

            // ACT
            var result = new List<LineElement>();
            ThickLineUtils.GetLineElementsInTile(leftBottomCorner, rightUpperCorner, lineElements, result, new ObjectPool());

            // ASSERT
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(3, result[0].Points.Count);
        }
    }
}
