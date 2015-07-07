using System;
using System.Collections.Generic;
using System.Linq;
using ActionStreetMap.Core;
using ActionStreetMap.Infrastructure.Reactive;
using ActionStreetMap.Maps.Data;
using ActionStreetMap.Maps.Data.Import;
using NUnit.Framework;
using Way = ActionStreetMap.Maps.Entities.Way;

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
            _boundingBox = BoundingBox.Create(TestHelper.BerlinTestFilePoint, 500);
            var indexBuilder = new InMemoryIndexBuilder(_boundingBox,
                TestHelper.GetIndexSettings(), TestHelper.GetObjectPool(), new ConsoleTrace());
            indexBuilder.Build();
            
            _editor = new ElementSourceEditor();
            _editor.ElementSource = _elementSource = new ElementSource(indexBuilder);
        }

        [TearDown]
        public void TearDown()
        {
            _editor.Dispose();
        }

        [Test]
        public void CanAddElement()
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


        [Test (Description = "We're expecting two elements with the same id but one with special tag. ")]
        public void CanDeleteElement()
        {
            // ARRANGE
            var way = CreateWay();
            _editor.Add(way);

            // ACT
            _editor.Delete<Way>(way.Id, new BoundingBox(way.Coordinates[0], way.Coordinates[1]));

            // ASSERT
            var result = GetWayById(way.Id);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(1, result[0].Tags.Count(t => t.Key == ActionStreetMap.Maps.Strings.DeletedElementTagKey));
        }

        #region Assertion helpers

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

        #endregion

        #region Helpers

        private List<Way> GetWayById(long id)
        {
            var results = _elementSource.Get(_boundingBox, MapConsts.MaxZoomLevel).ToArray().Wait();
            return results.OfType<Way>().Where(w => w.Id == id).ToList();
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

        #endregion
    }
}