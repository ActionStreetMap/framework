using System;
using ActionStreetMap.Core;
using NUnit.Framework;

namespace ActionStreetMap.Tests.Core
{
    [TestFixture]
    public class BoundingBoxTests
    {
        [Test]
        public void CanAddBbox()
        {
            // ARRANGE
            var bbox1 = new BoundingBox(new GeoCoordinate(0, 0), new GeoCoordinate(2, 2));
            var bbox2 = new BoundingBox(new GeoCoordinate(1, 1), new GeoCoordinate(3, 3));

            // ACT
            var result = bbox1 + bbox2;

            // ASSERT
            Assert.AreEqual(bbox1.MinPoint, result.MinPoint);
            Assert.AreEqual(bbox2.MaxPoint, result.MaxPoint);
        }

        [Test]
        public void CanGetSize()
        {
            // ARRANGE
            const int expectedSize = 2000;
            const int delta = 5;

            // ACT
            var bbox = BoundingBox.CreateBoundingBox(new GeoCoordinate(52, 13), expectedSize);
            var calculatedSize = bbox.Size;

            // ASSERT
            Assert.LessOrEqual(Math.Abs(expectedSize - calculatedSize), delta);
        }
    }
}