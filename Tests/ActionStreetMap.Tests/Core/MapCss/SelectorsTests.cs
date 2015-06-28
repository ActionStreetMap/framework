using System;
using System.Collections.Generic;
using ActionStreetMap.Core;
using ActionStreetMap.Core.MapCss;
using ActionStreetMap.Core.Tiling.Models;
using ActionStreetMap.Maps.Data;
using NUnit.Framework;

namespace ActionStreetMap.Tests.Core.MapCss
{
    [TestFixture]
    public class SelectorsTests
    {
        [Test]
        public void CanUseExist()
        {
            // ARRANGE
            var stylesheet = MapCssHelper.GetStylesheetFromContent("area[landuse] { z-index: 0.1}\n");

            // ACT
            var area1 = MapCssHelper.GetArea(new Dictionary<string, string>() { { "landuse", "forest" } }.ToTags());
            var area2 = MapCssHelper.GetArea(new Dictionary<string, string>() { { "building", "residential" } }.ToTags());

            // ASSERT
            Assert.IsTrue(stylesheet.GetModelRule(area1, MapConsts.MaxZoomLevel).IsApplicable);
            Assert.IsFalse(stylesheet.GetModelRule(area2, MapConsts.MaxZoomLevel).IsApplicable);
        }

        [Test]
        public void CanUseNotExist()
        {
            // ARRANGE
            var stylesheet = MapCssHelper.GetStylesheetFromContent("area[!landuse] { z-index: 0.1}\n");

            // ACT
            var area1 = MapCssHelper.GetArea(new Dictionary<string, string>() { { "landuse", "forest" } }.ToTags());
            var area2 = MapCssHelper.GetArea(new Dictionary<string, string>() { { "building", "residential" } }.ToTags());

            // ASSERT
            Assert.IsFalse(stylesheet.GetModelRule(area1, MapConsts.MaxZoomLevel).IsApplicable);
            Assert.IsTrue(stylesheet.GetModelRule(area2, MapConsts.MaxZoomLevel).IsApplicable);
        }

        [Test]
        public void CanUseEqual()
        {
            // ARRANGE
            var stylesheet = MapCssHelper.GetStylesheetFromContent("area[landuse=forest] { z-index: 0.1}\n");

            // ACT
            var area1 = MapCssHelper.GetArea(new Dictionary<string, string>() { { "landuse", "forest" } }.ToTags());
            var area2 = MapCssHelper.GetArea(new Dictionary<string, string>() { { "landuse", "grass" } }.ToTags());

            // ASSERT
            Assert.IsTrue(stylesheet.GetModelRule(area1, MapConsts.MaxZoomLevel).IsApplicable);
            Assert.IsFalse(stylesheet.GetModelRule(area2, MapConsts.MaxZoomLevel).IsApplicable);
        }

        [Test]
        public void CanUseNotEqual()
        {
            // ARRANGE
            var stylesheet = MapCssHelper.GetStylesheetFromContent("area[landuse!=forest] { z-index: 0.1}\n");

            // ACT
            var area1 = MapCssHelper.GetArea(new Dictionary<string, string>() { { "landuse", "forest" } }.ToTags());
            var area2 = MapCssHelper.GetArea(new Dictionary<string, string>() { { "landuse", "grass" } }.ToTags());

            // ASSERT
            Assert.IsFalse(stylesheet.GetModelRule(area1, MapConsts.MaxZoomLevel).IsApplicable);
            Assert.IsTrue(stylesheet.GetModelRule(area2, MapConsts.MaxZoomLevel).IsApplicable);
        }

        [Test]
        public void CanUseLess()
        {
            // ARRANGE
            var stylesheet = MapCssHelper.GetStylesheetFromContent("area[level<0] { z-index: 0.1}\n");

            // ACT
            var area1 = MapCssHelper.GetArea(new Dictionary<string, string>() { { "level", "-1" } }.ToTags());
            var area2 = MapCssHelper.GetArea(new Dictionary<string, string>() { { "level", "1" } }.ToTags());

            // ASSERT
            Assert.IsTrue(stylesheet.GetModelRule(area1, MapConsts.MaxZoomLevel).IsApplicable);
            Assert.IsFalse(stylesheet.GetModelRule(area2, MapConsts.MaxZoomLevel).IsApplicable);
        }

        [Test]
        public void CanUseGreater()
        {
            // ARRANGE
            var stylesheet = MapCssHelper.GetStylesheetFromContent("area[level>0] { z-index: 0.1}\n");

            // ACT
            var area1 = MapCssHelper.GetArea(new Dictionary<string, string>() { { "level", "1" } }.ToTags());
            var area2 = MapCssHelper.GetArea(new Dictionary<string, string>() { { "level", "0" } }.ToTags());

            // ASSERT
            Assert.IsTrue(stylesheet.GetModelRule(area1, MapConsts.MaxZoomLevel).IsApplicable);
            Assert.IsFalse(stylesheet.GetModelRule(area2, MapConsts.MaxZoomLevel).IsApplicable);
        }

        [Test]
        public void CanUseClosed()
        {
            // ARRANGE
            var provider = new StylesheetProvider(TestHelper.TestBaseMapcssFile, TestHelper.GetFileSystemService());
            var stylesheet = provider.Get();

            var closedWay = new Way
            {
                Points = new List<GeoCoordinate>()
                {
                    new GeoCoordinate(0, 0),
                    new GeoCoordinate(1, 0),
                    new GeoCoordinate(1, 0),
                    new GeoCoordinate(0, 0)
                },
                Tags = new Dictionary<string, string>() { { "barrier", "yes" } }.ToTags()
            };

            var openWay = new Way
            {
                Points = new List<GeoCoordinate>()
                {
                    new GeoCoordinate(0, 0),
                    new GeoCoordinate(1, 0),
                    new GeoCoordinate(1, 0),
                    new GeoCoordinate(0, 1)
                },
                Tags = new Dictionary<string, string>() { { "barrier", "yes" } }.ToTags()
            };

            // ACT & ASSERT
            Assert.IsTrue(stylesheet.GetModelRule(closedWay, MapConsts.MaxZoomLevel).IsApplicable);
            Assert.IsFalse(stylesheet.GetModelRule(openWay, MapConsts.MaxZoomLevel).IsApplicable);
        }

        [TestCase("z12", 12, 12, 12, true)]
        [TestCase("z12", 12, 12, 11, false)]
        [TestCase("z1-12", 1, 12, 5, true)]
        public void CanUseZoomLevel(string zoomStr, int start, int end, int currentZoomLevel, bool expectedResult)
        {
            // ARRANGE
            var stylesheet = MapCssHelper.GetStylesheetFromContent(
                String.Format("area|{0}[level>0] {{ z-index: 0.1}}\n", zoomStr));

            var area = MapCssHelper.GetArea(new Dictionary<string, string>() { { "level", "1" } }.ToTags());

            // ACT
            var rule = stylesheet.GetModelRule(area, currentZoomLevel);

            // ASSERT
            Assert.IsTrue(rule.IsApplicable == expectedResult);
        }
    }
}