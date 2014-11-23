﻿using System;
using System.Collections.Generic;
using ActionStreetMap.Core;
using ActionStreetMap.Core.Elevation;
using ActionStreetMap.Models.Geometry.ThickLine;
using Moq;
using NUnit.Framework;

namespace ActionStreetMap.Tests.Models.Geometry
{
    [TestFixture]
    public class ThickLineTests
    {
        [Test]
        public void CanGetIntermediatePoints()
        {
            // ARRANGE
            var heightMapMock = new Mock<HeightMap>();
            heightMapMock.Setup(h => h.LookupHeight(It.IsAny<MapPoint>())).Returns(0);
            var points = new List<MapPoint>()
            {
                new MapPoint(0, 0, 1),
                new MapPoint(2, 2, 2),
                new MapPoint(3, 3, 3),
                new MapPoint(5, 5, 4),
                new MapPoint(7, 7, 5),
                new MapPoint(13, 13, 6)
            };

            // ACT
            var result = ThickLineUtils.GetIntermediatePoints(heightMapMock.Object, points, 1f, 1f);

            //ARRANGE
            Assert.IsNotNull(result);
            Assert.IsNotEmpty(result);
            Assert.IsFalse(result.Count == points.Count);
            Assert.AreEqual(16, result.Count); // TODO check corectness
        }

        [Test]
        public void CanGetNextIntermediatePoint()
        {
            // ARRANGE & ACT
            var heightMapMock = new Mock<HeightMap>();
            heightMapMock.Setup(h => h.LookupHeight(It.IsAny<MapPoint>())).Returns(0);
            var result = ThickLineUtils.GetNextIntermediatePoint(heightMapMock.Object, 
                new MapPoint(0, 0, 1), new MapPoint(2, 2, 2), 1);

            // ASSERT
            Assert.IsTrue(Math.Abs(0.7f -  result.X) < 0.01);
            Assert.IsTrue(Math.Abs(0.7f - result.Y) < 0.01);
        }

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
    }
}
