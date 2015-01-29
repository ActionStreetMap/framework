
using System.IO;
using ActionStreetMap.Maps.Formats.Xml;
using NUnit.Framework;
using ActionStreetMap.Maps.Formats;
using System.Text;

namespace ActionStreetMap.Tests.Maps.Formats
{
    [TestFixture]
    public class XmlParserTests
    {
        private string _xmlContent;
        private XmlResponseParser _parser;

        [TestFixtureSetUp]
        public void SetUpFixture()
        {
            _xmlContent = File.ReadAllText(TestHelper.BerlinXmlData);
        }

        [SetUp]
        public void Setup()
        {
            var readerContext = new ReaderContext
            {
                SourceStream = new MemoryStream(Encoding.Default.GetBytes(_xmlContent)),
                Builder = new ActionStreetMap.Maps.Index.Import.IndexBuilder(new ConsoleTrace()),
                ReuseEntities = false,
                SkipTags = false,
            };
            _parser = new XmlResponseParser(readerContext);
        }

        [Test]
        public void CanParseOsmXml()
        {
            _parser.Parse();
        }
    }
}
