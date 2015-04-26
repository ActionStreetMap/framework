using System.Collections.Generic;
using System.Linq;
using ActionStreetMap.Core;
using ActionStreetMap.Core.Tiling;
using ActionStreetMap.Explorer.Commands;
using ActionStreetMap.Infrastructure.Reactive;
using ActionStreetMap.Maps.Data.Search;
using ActionStreetMap.Maps.Entities;
using Moq;
using NUnit.Framework;

namespace ActionStreetMap.Tests.Explorer.CommandLine
{
    [TestFixture]
    public class TagCommandTests
    {
        private Mock<ISearchEngine> _searchMock;
        private SearchCommand _command;

        [SetUp]
        public void SetUp()
        {
            _searchMock = new Mock<ISearchEngine>();

            _searchMock.Setup(s => s.SearchByTag("amenity", "bar")).Returns(Observable.Create<Element>(o =>
            {
                o.OnNext(new Node() {Id = 1, Tags = new Dictionary<string, string>() {{"amenity", "bar"}}.ToTags(), Coordinate = new GeoCoordinate(52.001, 13)});
                o.OnNext(new Way() { Id = 2, Tags = new Dictionary<string, string>() {{"amenity", "bar"}}.ToTags(), Coordinates = new List<GeoCoordinate> { new GeoCoordinate(52.0008, 13)}});
                o.OnNext(new Relation() { Id = 3, Tags = new Dictionary<string, string>() { { "amenity", "bar" } }.ToTags(), Members = new List<RelationMember>() { new RelationMember() { Member = new Node() { Coordinate = new GeoCoordinate(52.002, 13) } } }});
                o.OnCompleted();
                return Disposable.Empty;
            }));

            var geoPositionListener = new Mock<IPositionObserver<GeoCoordinate>>();
            geoPositionListener.Setup(p => p.Current).Returns(new GeoCoordinate(52, 13));
            _command = new SearchCommand(geoPositionListener.As<ITilePositionObserver>().Object, _searchMock.Object);
        }

        [Test]
        public void CanSearchForTag()
        {
            // ACT
            var result = _command.Execute(new[]
            {
                "/q:amenity=bar",
            }).Wait();

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
            }).Wait();

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
            }).Wait();

            // ASSERT
            Assert.IsNotNullOrEmpty(result);
            Assert.IsFalse(result.Contains("Node"));
            Assert.IsTrue(result.Contains("Way"));
            Assert.IsFalse(result.Contains("Relation"));
        }
    }
}
