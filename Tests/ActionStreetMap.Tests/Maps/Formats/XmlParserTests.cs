
using System.IO;
using ActionStreetMap.Maps.Formats.Xml;
using NUnit.Framework;

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
            _parser = new XmlResponseParser(_xmlContent);
        }

        [Test]
        public void CanParseOsmXml()
        {
            _parser.Parse();
        }
    }
}
