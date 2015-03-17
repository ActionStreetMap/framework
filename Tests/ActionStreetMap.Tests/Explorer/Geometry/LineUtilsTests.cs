using ActionStreetMap.Core;
using ActionStreetMap.Explorer.Scene.Geometry;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace ActionStreetMap.Tests.Explorer.Geometry
{
    [TestFixture]
    internal class LineUtilsTests
    {
        [Test]
        public void CanGetIntermediatePoints()
        {
            // ARRANGE 
            var heightMapMock = new Mock<IElevationProvider>();
            heightMapMock.Setup(h => h.GetElevation(It.IsAny<MapPoint>())).Returns(0);
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
            var result = LineUtils.GetIntermediatePoints(heightMapMock.Object, points, 1f, 1f);

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
            var heightMapMock = new Mock<IElevationProvider>();
            heightMapMock.Setup(h => h.GetElevation(It.IsAny<MapPoint>())).Returns(0);
            var result = LineUtils.GetNextIntermediatePoint(heightMapMock.Object,
                new MapPoint(0, 0, 1), new MapPoint(2, 2, 2), 1);

            // ASSERT 
            Assert.IsTrue(Math.Abs(0.7f - result.X) < 0.01);
            Assert.IsTrue(Math.Abs(0.7f - result.Y) < 0.01);
        }
    }
}