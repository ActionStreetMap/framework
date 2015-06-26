using ActionStreetMap.Core;
using ActionStreetMap.Explorer.Geometry.ThickLine;
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
    }
}
