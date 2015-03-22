using System.Collections.Generic;
using ActionStreetMap.Core;
using ActionStreetMap.Core.MapCss;
using ActionStreetMap.Core.Tiling.Models;
using ActionStreetMap.Explorer.Helpers;
using NUnit.Framework;

namespace ActionStreetMap.Tests.Explorer
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

            var tags = new TagCollection();
            tags.Add("building", "residential");
            var building = new Area()
            {
                Id = 1,
                Points = new List<GeoCoordinate>(),
                Tags = tags.Complete()
            };

            // ACT
            var rule = stylesheet.GetModelRule(building);
            var style = rule.GetFacadeBuilder();

            // ASSERT
            Assert.AreEqual("flat", style);
        }
    }
}
