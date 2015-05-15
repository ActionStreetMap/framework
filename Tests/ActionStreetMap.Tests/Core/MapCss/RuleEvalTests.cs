using System;
using System.Collections.Generic;
using System.Linq;
using ActionStreetMap.Core;
using ActionStreetMap.Core.MapCss;
using ActionStreetMap.Core.Tiling.Models;
using ActionStreetMap.Core.Utils;
using ActionStreetMap.Infrastructure.Dependencies;
using ActionStreetMap.Explorer.Helpers;
using NUnit.Framework;
using UnityEngine;
using ActionStreetMap.Explorer.Infrastructure;
using ActionStreetMap.Maps.Data;
using Canvas = ActionStreetMap.Core.Tiling.Models.Canvas;

namespace ActionStreetMap.Tests.Core.MapCss
{
    [TestFixture]
    internal class RuleEvalTests
    {
        [TestCase(TestHelper.TestBaseMapcssFile, true)]
        [TestCase(TestHelper.TestBaseMapcssFile, false)]
        public void CanUseCanvas(string path, bool canUseExprTree)
        {
            // ARRANGE
            var stylesheet = MapCssHelper.GetStylesheetFromFile(path, canUseExprTree);
            var canvas = new Canvas(new ObjectPool());

            // ACT
            var rule = stylesheet.GetModelRule(canvas, MapConsts.MaxZoomLevel);
            var material = rule.Evaluate<string>("material");

            // ASSERT
            Assert.AreEqual("Terrain", material);
        }

        [TestCase(TestHelper.TestBaseMapcssFile, true)]
        [TestCase(TestHelper.TestBaseMapcssFile, false)]
        public void CanMergeDeclarations(string path, bool canUseExprTree)
        {
            // ARRANGE
            var stylesheet = MapCssHelper.GetStylesheetFromFile(path, canUseExprTree);

            var area = new Area
            {
                Id = 1,
                Points = new List<GeoCoordinate>()
                {
                    new GeoCoordinate(52.5212186, 13.4096926),
                    new GeoCoordinate(52.5210184, 13.4097473),
                    new GeoCoordinate(52.5209891, 13.4097538),
                    new GeoCoordinate(52.5209766, 13.4098037)
                },
                Tags = new Dictionary<string, string>()
                {
                    {"building", "residential"},
                    {"building:shape", "sphere"},
                    {"min_height", "100"},
                    {"building:levels", "5"},
                }.ToTags()
            };

            // ACT
            var rule = stylesheet.GetModelRule(area, MapConsts.MaxZoomLevel);


            // ASSERT
            Assert.IsTrue(rule.IsApplicable, "Unable to get declarations!");

            Assert.AreEqual("sphere", rule.Evaluate<string>("builder"), "Unable to merge declarations!");
            Assert.AreEqual(100, rule.Evaluate<float>("min_height"), "Unable to eval min_height from tag!");
            Assert.AreEqual(new Color32(250, 128, 114, 255), rule.GetFillUnityColor(), "Unable to merge declarations!");
            Assert.AreEqual("solid", rule.Evaluate<string>("behaviour"), "First rule isn't applied!");
            Assert.AreEqual("Concrete_Patterned", rule.Evaluate<string>("material"), "First rule isn't applied!");
            Assert.AreEqual(15, rule.Evaluate<float>("height"), "Unable to eval height from building:levels!");
        }


        [TestCase(TestHelper.DefaultMapcssFile, true)]
        [TestCase(TestHelper.DefaultMapcssFile, false)]
        public void CanProcessSequenceWithApp(string path, bool canUseExprTree)
        {
            // ARRANGE
            var testPoints = new List<GeoCoordinate>()
            {
                new GeoCoordinate(0, 0),
                new GeoCoordinate(0, 0),
                new GeoCoordinate(0, 0)
            };
            var area1 = new Area
            {
                Tags = new Dictionary<string, string>()
                {
                    {"building", "tower"},
                    {"building:material", "metal"},
                    {"building:part", "yes"},
                    {"height", "237"},
                    {"min_height", "205"},
                }.ToTags(),
                Points = testPoints
            };
            var area2 = new Area
            {
                Tags = new Dictionary<string, string>()
                {
                    {"building", "roof"},
                    {"building:part", "yes"},
                    {"level", "1"},
                }.ToTags(),
                Points = testPoints
            };

            using (var container = new Container())
            {
                var componentRoot = TestHelper.GetGameRunner(container);
                componentRoot.RunGame(TestHelper.BerlinTestFilePoint);
                var provider = container.Resolve<IStylesheetProvider>() as StylesheetProvider;
                var stylesheet = provider.Get();

                // ACT
                var rule1 = stylesheet.GetModelRule(area1, MapConsts.MaxZoomLevel);
                var rule2 = stylesheet.GetModelRule(area2, MapConsts.MaxZoomLevel);

                // ASSERT
                Assert.AreEqual(237, rule1.GetHeight());
                Assert.AreEqual(12f, rule2.GetHeight());
            }
        }

        [TestCase(TestHelper.TestBaseMapcssFile, true)]
        [TestCase(TestHelper.TestBaseMapcssFile, false)]
        public void CanUseSimpleEvaluate(string path, bool canUseExprTree)
        {
            // ARRANGE
            var model = new Area
            {
                Id = 1,
                Tags = new Dictionary<string, string>()
                {
                    {"building:levels", "5"}
                }.ToTags()
            };

            var stylesheet = MapCssHelper.GetStylesheetFromFile(path, canUseExprTree);
            var evalDeclaration = MapCssHelper.GetStyles(stylesheet)[3].Declarations.First();

            // ACT
            var evalResult = evalDeclaration.Value.Evaluator.Walk<float>(model);

            // ASSERT
            Assert.AreEqual(15, evalResult);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void CanPerformSimpleOperationWithTags(bool canUseExprTree)
        {
            // ARRANGE
            var model = new Area
            {
                Id = 1,
                Tags = new Dictionary<string, string>()
                {
                    {"building:height", "20"},
                    {"roof:height", "5"},
                }.ToTags()
            };

            var stylesheet = MapCssHelper.GetStylesheetFromContent("area[building:height][roof:height] { height: eval(num(tag('building:height')) - num(tag('roof:height')));}\n", canUseExprTree);
            var rule = stylesheet.GetModelRule(model, MapConsts.MaxZoomLevel);

            // ACT
            var evalResult = rule.GetHeight();

            // ASSERT
            Assert.AreEqual(15, evalResult);
        }


       /* [Test]
        public void CanPerformTwoEvalOperationSequence()
        {
            // ARRANGE
            var model = new Area
            {
                Id = 1,
                Tags = new Dictionary<string, string>()
                {
                    {"building:part", "yes"},
                    {"building:height", "20"},
                    {"building:min_height", "3"},
                    {"roof:height", "5"},
                }
            };

            var stylesheet = MapCssHelper.GetStylesheetFromContent("area[building:height][roof:height] { height: eval(num(tag('building:height')) - num(tag('roof:height')));}\n"+
                                                        "area[building:part][building:height][building:min_height] { height: eval(num(tag('building:height')) - num(tag('building:min_height')));}");
            var rule = stylesheet.GetModelRule(model);


            foreach (var declaration in rule.Declarations)
            {
                var height = declaration.Value.Evaluator.Walk<float>(model);
            }

            // ACT
            //var evalResult = rule.GetHeight();

            // ASSERT
           // Assert.AreEqual(12, evalResult);
        }*/

        [TestCase(TestHelper.TestBaseMapcssFile, true)]
        [TestCase(TestHelper.TestBaseMapcssFile, false)]
        public void CanGetMissing(string path, bool canUseExprTree)
        {
            // ARRANGE
            var stylesheet = MapCssHelper.GetStylesheetFromFile(path, canUseExprTree);

            var area = new Area
            {
                Id = 1,
                Points = new List<GeoCoordinate>(),
                Tags = new Dictionary<string, string>()
                {
                    {"building", "residential"},
                }.ToTags()
            };

            // ACT
            var rule = stylesheet.GetModelRule(area, MapConsts.MaxZoomLevel);

            // ASSERT
            Assert.AreEqual(0, rule.GetLevels());
        }

        [TestCase(true)]
        [TestCase(false)]
        public void CanUseAndSelectors(bool canUseExprTree)
        {
            // ARRANGE
            var stylesheet = MapCssHelper.GetStylesheetFromContent("way[waterway][name],way[waterway] { z-index: 0.1}\n", canUseExprTree);

            // ACT
            var way1 = MapCssHelper.GetWay(
                new Dictionary<string, string>()
                {
                    {"waterway", "river"},
                    {"name", "spree"}
                }.ToTags());
            var way2 = MapCssHelper.GetWay(new Dictionary<string, string>()
            {
                {"name", "some name"}
            }.ToTags());

            // ASSERT
            Assert.IsTrue(stylesheet.GetModelRule(way1, MapConsts.MaxZoomLevel).IsApplicable);
            Assert.IsFalse(stylesheet.GetModelRule(way2, MapConsts.MaxZoomLevel).IsApplicable);
        }

        [TestCase(TestHelper.TestBaseMapcssFile, true)]
        [TestCase(TestHelper.TestBaseMapcssFile, false)]
        public void CanGetColorByRGB(string path, bool canUseExprTree)
        {
            // ARRANGE
            var stylesheet = MapCssHelper.GetStylesheetFromFile(path, canUseExprTree);

            var buildingWithColorCode = new Area
            {
                Id = 1,
                Points = new List<GeoCoordinate>(),
                Tags = new Dictionary<string, string>()
                {
                    {"building", "commercial"},
                }.ToTags()
            };

            // ACT
            var rule = stylesheet.GetModelRule(buildingWithColorCode, MapConsts.MaxZoomLevel);

            // ASSERT
            Assert.AreEqual(ColorUtils.FromName("red"),
                GetOriginalColorTypeObject(rule.GetFillUnityColor()));
        }

        [TestCase(TestHelper.TestBaseMapcssFile, true)]
        [TestCase(TestHelper.TestBaseMapcssFile, false)]
        public void CanGetColorByName(string path, bool canUseExprTree)
        {
            // ARRANGE
            var stylesheet = MapCssHelper.GetStylesheetFromFile(path, canUseExprTree);

            var buildingWithColorName = new Area
            {
                Id = 1,
                Points = new List<GeoCoordinate>(),
                Tags = new Dictionary<string, string>()
                {
                    {"building", "yes"},
                }.ToTags()
            };

            // ACT
            var rule = stylesheet.GetModelRule(buildingWithColorName, MapConsts.MaxZoomLevel);

            // ASSERT
            Assert.AreEqual(ColorUtils.FromName("salmon"),
                GetOriginalColorTypeObject(rule.GetFillUnityColor()));
        }

        [TestCase(TestHelper.TestBaseMapcssFile, true)]
        [TestCase(TestHelper.TestBaseMapcssFile, false)]
        public void CanApplyColorByRGB(string path, bool canUseExprTree)
        {
            // ARRANGE
            var stylesheet = MapCssHelper.GetStylesheetFromContent("area[building:color] { fill-color:eval(color(tag('building:color')));}", canUseExprTree);

            var buildingWithColorCode = new Area
            {
                Id = 1,
                Points = new List<GeoCoordinate>(),
                Tags = new Dictionary<string, string>()
                {
                    {"building", "commercial"},
                    {"building:color", "#cfc6b5"}
                }.ToTags()
            };

            // ACT
            var rule = stylesheet.GetModelRule(buildingWithColorCode, MapConsts.MaxZoomLevel);

            // ASSERT
            Assert.AreEqual(ColorUtils.FromUnknown("#cfc6b5"),
                GetOriginalColorTypeObject(rule.GetFillUnityColor()));
        }

        [TestCase(true)]
        [TestCase(false)]
        public void CanUseNode(bool canUseExprTree)
        {
            // ARRANGE
            var stylesheet = MapCssHelper.GetStylesheetFromContent("node[test] { z-index: 0.1}\n", canUseExprTree);

            // ACT
            var node = MapCssHelper.GetNode(new Dictionary<string, string>()
                {
                    {"test", "yes"},
                }.ToTags());

            // ASSERT
            Assert.IsTrue(stylesheet.GetModelRule(node, MapConsts.MaxZoomLevel).IsApplicable);
        }

        [Test]
        public void CanEvaluateGradient()
        {
            // ARRANGE
            var stylesheet = MapCssHelper
                .GetStylesheetFromContent(String.Format("node[test] {{ facade: eval(gradient(tag('colour')));}}\n"));

            // ACT
            var node = MapCssHelper.GetNode(new Dictionary<string, string>()
                {
                    {"test", "yes"},
                    {"colour", "#0fff8f"},
                }.ToTags());

            // ASSERT
            var rule = stylesheet.GetModelRule(node, MapConsts.MaxZoomLevel);
            var facadeGradient = rule.Evaluate<string>("facade");
            Assert.IsNotEmpty(facadeGradient);
            Assert.AreEqual("gradient(#0fff8f, #0fff8f 50%, #099956)", facadeGradient);
        }

        [Test]
        public void CanUseGradient()
        {
            // ARRANGE
            const string gradientString = "gradient(#f4f4f4, yellow 10%, green 20%)";
            var stylesheet = MapCssHelper
                .GetStylesheetFromContent(String.Format("node[test] {{ facade: {0};}}\n", gradientString));

            // ACT
            var node = MapCssHelper.GetNode(new Dictionary<string, string>() { { "test", "yes" }, }.ToTags());

            // ASSERT
            var rule = stylesheet.GetModelRule(node, MapConsts.MaxZoomLevel);
            var facadeGradient = rule.Evaluate<string>("facade");
            Assert.IsNotEmpty(facadeGradient);
            Assert.AreEqual(gradientString, facadeGradient);
        }

        private ActionStreetMap.Core.Unity.Color32 GetOriginalColorTypeObject(Color32 color)
        {
            return new ActionStreetMap.Core.Unity.Color32(color.r, color.g, color.b, color.a);
        }      
    }
}