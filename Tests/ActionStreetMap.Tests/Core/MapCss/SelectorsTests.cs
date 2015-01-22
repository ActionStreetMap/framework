using System.Collections.Generic;
using System.Collections.ObjectModel;
using ActionStreetMap.Core;
using ActionStreetMap.Core.MapCss;
using ActionStreetMap.Core.Tiling.Models;
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
            var area1 = MapCssHelper.GetArea(new Dictionary<string, string>(){{"landuse", "forest"}});
            var area2 = MapCssHelper.GetArea(new Dictionary<string, string>(){{"building", "residential"}});

            // ASSERT
            Assert.IsTrue(stylesheet.GetModelRule(area1).IsApplicable);
            Assert.IsFalse(stylesheet.GetModelRule(area2).IsApplicable);
        }

        [Test]
        public void CanUseNotExist()
        {
            // ARRANGE
            var stylesheet = MapCssHelper.GetStylesheetFromContent("area[!landuse] { z-index: 0.1}\n");

            // ACT
            var area1 = MapCssHelper.GetArea(new Dictionary<string, string>(){{"landuse", "forest"}});
            var area2 = MapCssHelper.GetArea(new Dictionary<string, string>() { { "building", "residential" } });

            // ASSERT
            Assert.IsFalse(stylesheet.GetModelRule(area1).IsApplicable);
            Assert.IsTrue(stylesheet.GetModelRule(area2).IsApplicable);
        }

        [Test]
        public void CanUseEqual()
        {
            // ARRANGE
            var stylesheet = MapCssHelper.GetStylesheetFromContent("area[landuse=forest] { z-index: 0.1}\n");

            // ACT
            var area1 = MapCssHelper.GetArea(new Dictionary<string, string>(){{"landuse", "forest"}});
            var area2 = MapCssHelper.GetArea(new Dictionary<string, string>() { { "landuse", "grass" } });

            // ASSERT
            Assert.IsTrue(stylesheet.GetModelRule(area1).IsApplicable);
            Assert.IsFalse(stylesheet.GetModelRule(area2).IsApplicable);
        }

        [Test]
        public void CanUseNotEqual()
        {
            // ARRANGE
            var stylesheet = MapCssHelper.GetStylesheetFromContent("area[landuse!=forest] { z-index: 0.1}\n");

            // ACT
            var area1 = MapCssHelper.GetArea(new Dictionary<string, string>() {{ "landuse", "forest" }});
            var area2 = MapCssHelper.GetArea(new Dictionary<string, string>(){{"landuse", "grass"}});

            // ASSERT
            Assert.IsFalse(stylesheet.GetModelRule(area1).IsApplicable);
            Assert.IsTrue(stylesheet.GetModelRule(area2).IsApplicable);
        }

        [Test]
        public void CanUseLess()
        {
            // ARRANGE
            var stylesheet = MapCssHelper.GetStylesheetFromContent("area[level<0] { z-index: 0.1}\n");

            // ACT
            var area1 = MapCssHelper.GetArea(new Dictionary<string, string>() { {"level", "-1"}});
            var area2 = MapCssHelper.GetArea(new Dictionary<string, string>() { { "level", "1" } });

            // ASSERT
            Assert.IsTrue(stylesheet.GetModelRule(area1).IsApplicable);
            Assert.IsFalse(stylesheet.GetModelRule(area2).IsApplicable);
        }

        [Test]
        public void CanUseGreater()
        {
            // ARRANGE
            var stylesheet = MapCssHelper.GetStylesheetFromContent("area[level>0] { z-index: 0.1}\n");

            // ACT
            var area1 = MapCssHelper.GetArea(new Dictionary<string, string>() { {"level", "1"}});
            var area2 = MapCssHelper.GetArea(new Dictionary<string, string>() { {"level", "0"}});

            // ASSERT
            Assert.IsTrue(stylesheet.GetModelRule(area1).IsApplicable);
            Assert.IsFalse(stylesheet.GetModelRule(area2).IsApplicable);
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
                Tags = new Dictionary<string, string>()
                {
                    {"barrier", "yes"}
                }
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
                Tags = new Dictionary<string, string>()
                {
                    {"barrier", "yes"}
                }
            };

            // ACT & ASSERT
            Assert.IsTrue(stylesheet.GetModelRule(closedWay).IsApplicable);
            Assert.IsFalse(stylesheet.GetModelRule(openWay).IsApplicable);
        }
    }
}