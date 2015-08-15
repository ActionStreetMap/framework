using System.Collections.Generic;
using ActionStreetMap.Core.Geometry.Triangle;
using ActionStreetMap.Core.Geometry.Triangle.Geometry;
using NUnit.Framework;

namespace ActionStreetMap.Tests.Core.Geometry.Triangle
{
    [TestFixture]
    class LineGridSplitterTests
    {
        [Test]
        public void CanSplitHorizontal()
        {
            // ARRANGE
            var start = new Point(0, 0);
            var end = new Point(10, 0);
            var splitter = new LineGridSplitter();
            var result = new List<Point>();

            // ACT
            splitter.Split(start, end, TestHelper.GetObjectPool(), result);

            // ASSERT
            for (int i = 0; i <= 10; i++)
                Assert.AreEqual(new Point(i, 0), result[i]);
        }

        [Test]
        public void CanSplitVertical()
        {
            // ARRANGE
            var start = new Point(0, 0);
            var end = new Point(0, 10);
            var splitter = new LineGridSplitter();
            var result = new List<Point>();

            // ACT
            splitter.Split(start, end, TestHelper.GetObjectPool(), result);

            // ASSERT
            for (int i = 0; i <= 10; i++)
                Assert.AreEqual(new Point(0, i), result[i]);
        }

        [Test]
        public void CanSplit45Angle()
        {
            // ARRANGE
            var start = new Point(0, 0);
            var end = new Point(-10, 10);
            var splitter = new LineGridSplitter();
            var result = new List<Point>();

            // ACT
            splitter.Split(start, end, TestHelper.GetObjectPool(), result);

            // ASSERT
            for (int i = 0; i <= 10; i++)
                Assert.AreEqual(new Point(-i, i), result[i]);
        }
    }
}
