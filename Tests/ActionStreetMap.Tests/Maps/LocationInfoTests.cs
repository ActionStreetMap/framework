using System.Collections.Generic;
using ActionStreetMap.Maps.Helpers;
using NUnit.Framework;

namespace ActionStreetMap.Tests.Maps
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
            }.ToTags();

            // ACT
            var locationInfo = AddressExtractor.Extract(tags);

            // ASSERT
            Assert.AreEqual("26", locationInfo.Name);
            Assert.AreEqual("Zacharova", locationInfo.Street);
            Assert.AreEqual("220088", locationInfo.Code);
        }
    }
}