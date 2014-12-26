using System.IO;
using System.Linq;
using ActionStreetMap.Infrastructure.Formats.Json;
using ActionStreetMap.Osm.Index.Import;
using NUnit.Framework;

namespace ActionStreetMap.Tests.Osm.Index
{
    [TestFixture]
    public class IndexSettingsTests
    {
        private IndexSettings _settings;

        [SetUp]
        public void SetUp()
        {
            _settings = new IndexSettings();
            using (var reader = new StreamReader(File.Open(TestHelper.TestIndexSettingsPath, FileMode.Open)))
            {
                var jsonNode = JSON.Parse(reader.ReadToEnd());
                _settings.ReadFromJson(jsonNode);
            }
        }

        [Test]
        public void CanReadSpatialSettings()
        {
            var spatial = _settings.Spatial;

            // ASSERT
            Assert.AreEqual(65, spatial.MaxEntries);
            Assert.Greater(spatial.RemoveTags.Count(), 0);
            Assert.Greater(spatial.Include.Nodes.Count, 0);
        }

        [Test]
        public void CanReadSearchSettings()
        {
            var search = _settings.Search;

            // ASSERT
            Assert.AreEqual(4, search.PrefixLength);
            Assert.AreEqual(300000, search.KvIndexCapacity);
        }
    }
}
