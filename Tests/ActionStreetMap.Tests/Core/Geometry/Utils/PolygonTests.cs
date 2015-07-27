using System.Collections.Generic;
using ActionStreetMap.Core.Geometry;
using ActionStreetMap.Core.Geometry.Utils;
using NUnit.Framework;

namespace ActionStreetMap.Tests.Core.Geometry.Utils
{
    [TestFixture]
    public class PolygonTests
    {
        [Test]
        public void CanGetCentroid()
        {
            // ARRANGE
            var polygon = new List<Vector2d>()
            {
                new Vector2d(0, 0),
                new Vector2d(0, 10),
                new Vector2d(10, 10),
                new Vector2d(10, 0),
            };

            // ACT
            var center = PolygonUtils.GetCentroid(polygon);

            // ASSERT
            Assert.AreEqual(new Vector2d(5, 5), center);
        }
    }
}
