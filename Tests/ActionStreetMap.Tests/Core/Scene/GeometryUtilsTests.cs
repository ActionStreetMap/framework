using System.Collections.Generic;
using ActionStreetMap.Core;
using ActionStreetMap.Core.Geometry;
using ActionStreetMap.Core.Geometry.Utils;
using NUnit.Framework;

namespace ActionStreetMap.Tests.Core.Scene
{
    [TestFixture]
    public class GeometryUtilsTests
    {
        [TestCase(-10, 0, 90)]
        [TestCase(-5, -5, 45)]
        [TestCase(-5, 5, 135)]
        [TestCase(0, 10, 180)]
        [TestCase(5, 5, 225)]
        [TestCase(10, 0, 270)]
        [TestCase(5, -5, 315)]
        public void CanGetAngle(float x, float y, float expectedAngle)
        {
            // ARRANGE
            var pivot = new Vector2d(0, 0);
            var bottom = new Vector2d(0, -10);

            // ACT
            var angle = GeometryUtils.GetTurnAngle(bottom, pivot, new Vector2d(x, y));

            // ASSERT
            Assert.AreEqual(expectedAngle, angle);
        }

        [Test]
        public void CanSortByTurnAngle()
        {
            // ARRANGE
            var pivot = new Vector2d(0, 0);
            var bottom = new Vector2d(0, -10);
            var left = new Vector2d(-10, 0);
            var right = new Vector2d(10, 0);
            var top = new Vector2d(0, 10);
            var sorted1 = new List<Vector2d>() { left, right, top };
            var sorted2 = new List<Vector2d>() { top, right, left };
            var sorted3 = new List<Vector2d>() { right, left, top };

            // ACT
            GeometryUtils.SortByAngle(bottom, pivot, sorted1);
            GeometryUtils.SortByAngle(bottom, pivot, sorted2);
            GeometryUtils.SortByAngle(bottom, pivot, sorted3);

            // ASSERT
            CollectionAssert.AreEqual(new List<Vector2d>() { left, top, right }, sorted2);
            CollectionAssert.AreEqual(sorted1, sorted2);
            CollectionAssert.AreEqual(sorted1, sorted3);
        }
    }
}
