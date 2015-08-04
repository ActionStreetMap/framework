using System;
using ActionStreetMap.Core.Geometry;
using ActionStreetMap.Core.Geometry.Utils;
using ActionStreetMap.Core.Utils;
using NUnit.Framework;

namespace ActionStreetMap.Tests.Core.Geometry.Utils
{
    [TestFixture]
    internal class Vector2dUtilsTests
    {
        [Test]
        public void CanDetectIntersection()
        {
            // ARRANGE
            var s1 = new Vector2d(0, 0);
            var e1 = new Vector2d(10, 10);
            var s2 = new Vector2d(10, 0);
            var e2 = new Vector2d(0, 10);
            double r;

            // ACT
            var result = Vector2dUtils.LineIntersects(s1, e1, s2, e2, out r);
            var currIntersectPoint = Vector2dUtils.GetPointAlongLine(s1, s2, r);

            // ASSERT
            Assert.IsTrue(result);
            Assert.IsTrue(Math.Abs(currIntersectPoint.X - 5) < MathUtils.Epsion);
            Assert.IsTrue(Math.Abs(currIntersectPoint.X - 5) < MathUtils.Epsion);
        }
    }
}
