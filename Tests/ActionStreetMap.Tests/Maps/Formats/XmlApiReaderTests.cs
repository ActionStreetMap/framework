using System.IO;
using System.Text;
using ActionStreetMap.Core;
using ActionStreetMap.Infrastructure.Diagnostic;
using ActionStreetMap.Infrastructure.Formats.Json;
using ActionStreetMap.Infrastructure.Reactive;
using ActionStreetMap.Maps.Formats;
using ActionStreetMap.Maps.Formats.Xml;
using ActionStreetMap.Maps.Index.Import;
using ActionStreetMap.Maps.Index.Spatial;
using ActionStreetMap.Maps.Index.Storage;
using NUnit.Framework;

namespace ActionStreetMap.Tests.Maps.Formats
{
    [TestFixture]
    public class XmlApiReaderTests
    {
        private string _xmlContent;
        private XmlApiReader _reader;
        private TestableIndexBuilder _indexBuilder;

        [TestFixtureSetUp]
        public void SetUpFixture()
        {
            _xmlContent = File.ReadAllText(TestHelper.BerlinXmlData);
            _indexBuilder = new TestableIndexBuilder(new ConsoleTrace());
        }

        [SetUp]
        public void Setup()
        {
            _reader = new XmlApiReader(new ReaderContext
            {
                SourceStream = new MemoryStream(Encoding.UTF8.GetBytes(_xmlContent)),
                Builder = _indexBuilder,
                ReuseEntities = false,
                SkipTags = false,
            });
        }

        [Test]
        public void CanParseOsmXml()
        {
            // ACT
            _reader.Read();

            // ASSERT
            // TODO check xml processing logic, not builder!
            var element = _indexBuilder.GetTree().Search(new BoundingBox(new GeoCoordinate(52.533, 13.386),
                new GeoCoordinate(52.534, 13.387))).Wait();
            Assert.AreNotEqual(0, element);
        }

        #region Nested classes

        // NOTE This class is workaround due to limitation of mock framework (cannot mock sealed, internal, etc.)
        private class TestableIndexBuilder : IndexBuilder
        {
            public TestableIndexBuilder(ITrace trace) : base(trace)
            {
                Settings = new IndexSettings();
                Settings.ReadFromJson(JSON.Parse(File.ReadAllText(TestHelper.TestIndexSettingsPath)));

                Store = new ElementStore(new KeyValueStore(new KeyValueIndex(1000, 3),
                    new KeyValueUsage(new MemoryStream()), new MemoryStream()), new MemoryStream());

                Tree = new RTree<uint>();
            }

            public override void Build() {}

            public ISpatialIndex<uint> GetTree()
            {
                return Tree;
            }
        }

        #endregion
    }
}