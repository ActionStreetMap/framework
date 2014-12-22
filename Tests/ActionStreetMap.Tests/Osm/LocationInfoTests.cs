using System.Collections.Generic;
using ActionStreetMap.Osm.Helpers;
using NUnit.Framework;

namespace ActionStreetMap.Tests.Osm
{
    [TestFixture]
    public class LocationInfoTests
    {
        [Test]
        public void CanExtractLocationInfo()
        {
            // ARRANGE
            var tags = new Dictionary<string, string>()
            {
                {"addr:housenumber", "26"},
                {"addr:postcode", "220088"},
                {"addr:street", "Zacharova"},
            };

            // ACT
            var locationInfo = AddressExtractor.Extract(tags);

            // ASSERT
            Assert.AreEqual("26", locationInfo.Name);
            Assert.AreEqual("Zacharova", locationInfo.Street);
            Assert.AreEqual("220088", locationInfo.Code);
        }
    }
}