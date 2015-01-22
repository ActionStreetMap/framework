using System.Collections.Generic;
using System.Collections.ObjectModel;
using ActionStreetMap.Core;
using ActionStreetMap.Core.MapCss;
using ActionStreetMap.Core.Tiling.Models;
using ActionStreetMap.Explorer.Helpers;
using NUnit.Framework;

namespace ActionStreetMap.Tests.Models
{
    [TestFixture]
    public class BuildingMapCssTests
    {
        [Test]
        public void CanGetBuildingStyle()
        {
            // ARRANGE
            var provider = new StylesheetProvider(TestHelper.DefaultMapcssFile, TestHelper.GetFileSystemService());
            var stylesheet = provider.Get();

            var building = new Area()
            {
                Id = 1,
                Points = new List<GeoCoordinate>(),
                Tags = new Dictionary<string, string>()
                {
                    {"building","residential"},
                }
            };

            // ACT
            var rule = stylesheet.GetModelRule(building);
            var style = rule.GetBuildingType();

            // ASSERT
            Assert.AreEqual("residential", style);
        }
    }
}
