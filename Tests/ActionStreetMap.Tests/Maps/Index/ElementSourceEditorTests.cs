using System;
using System.Collections.Generic;
using System.Linq;
using ActionStreetMap.Core;
using ActionStreetMap.Infrastructure.Reactive;
using ActionStreetMap.Maps.Data;
using ActionStreetMap.Maps.Data.Import;
using ActionStreetMap.Maps.Entities;
using NUnit.Framework;

namespace ActionStreetMap.Tests.Maps.Index
{
    [TestFixture]
    public class ElementSourceEditorTests
    {
        private BoundingBox _boundingBox;
        private IElementSourceEditor _editor;
        private IElementSource _elementSource;

        [SetUp]
        public void Setup()
        {
            var indexSettings = TestHelper.GetIndexSettings();
            var fileSystemService = TestHelper.GetFileSystemService();
            var indexBuilder = new InMemoryIndexBuilder("xml", fileSystemService.ReadStream(TestHelper.BerlinXmlData),
                indexSettings, TestHelper.GetObjectPool(), new ConsoleTrace());
            indexBuilder.Build();

            _boundingBox = indexBuilder.BoundingBox;

            _editor = new ElementSourceEditor();
            _editor.ElementSource = _elementSource = new ElementSource(_boundingBox, indexBuilder.KvUsage,
                indexBuilder.KvIndex, indexBuilder.KvStore, indexBuilder.Store, indexBuilder.Tree);
        }

        [TearDown]
        public void TearDown()
        {
            _editor.Dispose();
        }

        [Test]
        public void CanAddElementIntoElementSource()
        {
            // ARRANGE
            var way = CreateWay();

            // ACT
            _editor.Add(way);

            // ASSERT
            var results = _elementSource.Get(_boundingBox, MapConsts.MaxZoomLevel).ToArray().Wait();
            var result = results.OfType<Way>().Single(w => w.Id == way.Id);
            AssertWays(way, result);
        }

        private void AssertWays(Way expected, Way result)
        {
            Assert.AreEqual(expected.Id, result.Id);

            Assert.AreEqual(expected.Tags.Count, result.Tags.Count);
            for (var i = 0; i < expected.Tags.Count; i++)
                Assert.AreEqual(expected.Tags[i], result.Tags[i]);

            Assert.AreEqual(expected.Coordinates.Count, result.Coordinates.Count);
            for (var i = 0; i < expected.Coordinates.Count; i++)
                AssertGeoCoordinates(expected.Coordinates[i], result.Coordinates[i]);
        }

        private void AssertGeoCoordinates(GeoCoordinate expected, GeoCoordinate result)
        {
            Assert.IsTrue(Math.Abs(expected.Latitude - result.Latitude) < MapConsts.GeoCoordinateAccuracy);
            Assert.IsTrue(Math.Abs(expected.Longitude - result.Longitude) < MapConsts.GeoCoordinateAccuracy);
        }

        private Way CreateWay()
        {
            var minPoint = _boundingBox.MinPoint;
            var maxPoint = _boundingBox.MaxPoint;
            var latStep = (maxPoint.Latitude - minPoint.Latitude)*0.33;
            var lonStep = (maxPoint.Longitude - minPoint.Longitude) * 0.33;
            var coordinates = new List<GeoCoordinate>()
            {
                new GeoCoordinate(minPoint.Latitude + latStep, minPoint.Longitude + lonStep),
                new GeoCoordinate(minPoint.Latitude + latStep * 2, minPoint.Longitude + lonStep * 2),
            };
            return new Way
            {
                Id = 1,
                Coordinates = coordinates,
                Tags = new Dictionary<string, string> {{"key1", "value1"}, {"key2", "value2"}}.ToTags()
            };
        }
    }
}