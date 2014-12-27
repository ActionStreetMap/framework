using System.Collections.Generic;
using ActionStreetMap.Core;
using ActionStreetMap.Explorer.Commands;
using ActionStreetMap.Osm.Entities;
using ActionStreetMap.Osm.Index.Search;
using Moq;
using NUnit.Framework;

namespace ActionStreetMap.Tests.Explorer.CommandLine
{
    [TestFixture]
    public class TagCommandTests
    {
        private TagCommand _command;
        [SetUp]
        public void SetUp()
        {
            var search = new Mock<ISearchEngine>();
            search.Setup(s => s.SearchByTag("amenity", "bar")).Returns(new List<Element>()
            {
                new Node() {Id = 1, Tags = new Dictionary<string, string>() {{"amenity", "bar"}}, Coordinate = new GeoCoordinate(52.001, 13)},
                new Way() {Id = 2, Tags = new Dictionary<string, string>() {{"amenity", "bar"}}, Coordinates = new List<GeoCoordinate>() {new GeoCoordinate(52.0008, 13)}},
                new Relation() {Id = 3, Tags = new Dictionary<string, string>() {{"amenity", "bar"}}, Members = new List<RelationMember>() { new RelationMember() { Member = new Node() { Coordinate = new GeoCoordinate(52.002, 13)}}}},
            });

            var positionListener = new Mock<IPositionListener>();
            positionListener.Setup(p => p.CurrentPosition).Returns(new GeoCoordinate(52, 13));
            _command = new TagCommand(positionListener.Object, search.Object);
        }

        [Test]
        public void CanSearchForTag()
        {
            // ACT
            var result = _command.Execute(new[]
            {
                "/q:amenity=bar",
            });

            // ASSERT
            Assert.IsNotNullOrEmpty(result);
            Assert.IsTrue(result.Contains("Node"));
            Assert.IsTrue(result.Contains("Way"));
            Assert.IsTrue(result.Contains("Relation"));
        }

        [Test]
        public void CanSearchForTagWithTypeFilter()
        {
            // ACT
            var result = _command.Execute(new[]
            {
                "/q:amenity=bar",
                "/f:w"
            });

            // ASSERT
            Assert.IsNotNullOrEmpty(result);
            Assert.IsFalse(result.Contains("Node"));
            Assert.IsTrue(result.Contains("Way"));
            Assert.IsFalse(result.Contains("Relation"));
        }

        [Test]
        public void CanSearchForTagWithDistanceFilter()
        {
            // ACT
            var result = _command.Execute(new[]
            {
                "/q:amenity=bar",
                "/r:100"
            });

            // ASSERT
            Assert.IsNotNullOrEmpty(result);
            Assert.IsFalse(result.Contains("Node"));
            Assert.IsTrue(result.Contains("Way"));
            Assert.IsFalse(result.Contains("Relation"));
        }
    }
}
