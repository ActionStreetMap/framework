using System.Collections.Generic;
using ActionStreetMap.Explorer.CommandLine;
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
                new Node() {Id = 1, Tags = new Dictionary<string, string>() {{"amenity", "bar"}}},
                new Way() {Id = 2, Tags = new Dictionary<string, string>() {{"amenity", "bar"}}},
                new Relation() {Id = 3, Tags = new Dictionary<string, string>() {{"amenity", "bar"}}},
            });

            _command = new TagCommand(search.Object);
        }

        [Test]
        public void CanSearchForTagAllElements()
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
        public void CanSearchForTagWithFilter()
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
    }
}
