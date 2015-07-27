using System.Collections.Generic;
using System.Linq;
using ActionStreetMap.Core;
using ActionStreetMap.Core.Geometry;
using ActionStreetMap.Core.Geometry.Utils;
using ActionStreetMap.Core.Utils;
using NUnit.Framework;

namespace ActionStreetMap.Tests.Core.Geometry.Utils
{
    [TestFixture]
    public class PointHelperTests
    {
        [Test(Description = "Tests correctness of traversal order of sorting logic.")]
        public void CanReverseVertices()
        {
            // ARRANGE
            var center = new GeoCoordinate(52.529814, 13.388015);
            var geoCoordinates = new List<GeoCoordinate>
            {
                new GeoCoordinate(52.5295083, 13.3889532),
                new GeoCoordinate(52.5291505, 13.3891865),
                new GeoCoordinate(52.5291244, 13.3891088),
                new GeoCoordinate(52.5291819, 13.389071),
                new GeoCoordinate(52.5291502, 13.3889361),
                new GeoCoordinate(52.529244, 13.3888741),
                new GeoCoordinate(52.5292772, 13.3890143),
                new GeoCoordinate(52.529354, 13.3889638),
                new GeoCoordinate(52.5293253, 13.3888356),
                new GeoCoordinate(52.5294599, 13.3887466),
            };

            // ACT & ASSERT
            var originalOrder = geoCoordinates.Select(g => GeoProjection.ToMapCoordinate(center, g)).ToArray();

            // direct order
            var points = new List<Vector2d>();
            PointUtils.GetClockwisePolygonPoints(center, geoCoordinates, points);
            Assert.IsTrue(points.SequenceEqual(originalOrder));

            // reversed
            geoCoordinates.Reverse();
            points.Clear();
            PointUtils.GetClockwisePolygonPoints(center, geoCoordinates, points);

            Assert.IsTrue(points.SequenceEqual(originalOrder));
        }      
    }
}
