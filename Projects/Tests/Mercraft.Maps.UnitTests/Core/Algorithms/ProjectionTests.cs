﻿using System;
using Mercraft.Core;
using Mercraft.Core.Utilities;
using NUnit.Framework;

namespace Mercraft.Maps.UnitTests.Core.Algorithms
{
    [TestFixture]
    public class ProjectionTests
    {
        private const double Percision = 0.000001;

        /// <summary>
        ///     Berlin, Mitte (Eichendorffstraße)
        /// </summary>
        private readonly GeoCoordinate _center = new GeoCoordinate(52.529814, 13.388015);

        /// <summary>
        ///     Berlin, Tegel
        /// </summary>
        private readonly GeoCoordinate _target = new GeoCoordinate(52.582922, 13.282957);

        [Test]
        public void CanConvertToMap()
        {
            // ACT
            var mapCoordinate = GeoProjection.ToMapCoordinate(_center, _target);

            // ASSERT
            Assert.AreEqual(-7114, Math.Truncate(mapCoordinate.X));
            Assert.AreEqual(5902, Math.Truncate(mapCoordinate.Y));
            Assert.AreEqual(9244, Math.Truncate(Distance(new MapPoint(0, 0), mapCoordinate)));
        }

        [Test]
        public void CanConvertToGeo()
        {
            // ARRANGE
            var mapCoordinate = GeoProjection.ToMapCoordinate(_center, _target);

            // ACT
            var geoCoordinate = GeoProjection.ToGeoCoordinate(_center, mapCoordinate);

            // ASSERT
            Assert.True(Math.Abs(52.582922 - geoCoordinate.Latitude) < Percision);
            Assert.True(Math.Abs(13.282957 - geoCoordinate.Longitude) < Percision);
        }

        private static double Distance(MapPoint p1, MapPoint p2)
        {
            var diffX = p1.X - p2.X;
            var diffY = p1.Y - p2.Y;

            return Math.Sqrt(diffX*diffX + diffY*diffY);
        }
    }
}