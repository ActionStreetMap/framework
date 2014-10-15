﻿using System.Collections.Generic;
using System.Linq;
using Mercraft.Core;
using Mercraft.Core.MapCss;
using Mercraft.Core.Scene.Models;
using Mercraft.Core.Utilities;
using Mercraft.Explorer.Helpers;
using Mercraft.Infrastructure.Dependencies;
using NUnit.Framework;
using UnityEngine;

namespace Mercraft.Maps.UnitTests.Core.MapCss
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
            var canvas = new Canvas();

            // ACT
            var rule = stylesheet.GetModelRule(canvas);
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
                }
            };

            // ACT
            var rule = stylesheet.GetModelRule(area);


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
                },
                Points = testPoints
            };
            var area2 = new Area
            {
                Tags = new Dictionary<string, string>()
                {
                    {"building", "roof"},
                    {"building:part", "yes"},
                    {"level", "1"},
                },
                Points = testPoints
            };

            var container = new Container();
            var componentRoot = TestHelper.GetGameRunner(container);
            var provider = container.Resolve<IStylesheetProvider>() as StylesheetProvider;
            var stylesheet = provider.Get();

            // ACT
            var rule1 = stylesheet.GetModelRule(area1);
            var rule2 = stylesheet.GetModelRule(area2);

            // ASSERT
            Assert.AreEqual(237, rule1.GetHeight());
            Assert.AreEqual(12f, rule2.GetHeight());
            componentRoot = null;
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
                }
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
                }
            };

            var stylesheet = MapCssHelper.GetStylesheetFromContent("area[building:height][roof:height] { height: eval(num(tag('building:height')) - num(tag('roof:height')));}\n", canUseExprTree);
            var rule = stylesheet.GetModelRule(model);

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
                }
            };

            // ACT
            var rule = stylesheet.GetModelRule(area);

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
                });
            var way2 = MapCssHelper.GetWay(new Dictionary<string, string>()
            {
                {"name", "some name"}
            });

            // ASSERT
            Assert.IsTrue(stylesheet.GetModelRule(way1).IsApplicable);
            Assert.IsFalse(stylesheet.GetModelRule(way2).IsApplicable);
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
                }
            };

            // ACT
            var rule = stylesheet.GetModelRule(buildingWithColorCode);

            // ASSERT
            Assert.AreEqual(ColorUtility.FromName("red"),
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
                }
            };

            // ACT
            var rule = stylesheet.GetModelRule(buildingWithColorName);

            // ASSERT
            Assert.AreEqual(ColorUtility.FromName("salmon"),
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
                }
            };

            // ACT
            var rule = stylesheet.GetModelRule(buildingWithColorCode);

            // ASSERT
            Assert.AreEqual(ColorUtility.FromUnknown("#cfc6b5"),
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
                });

            // ASSERT
            Assert.IsTrue(stylesheet.GetModelRule(node).IsApplicable);
        }

        private Mercraft.Core.Unity.Color32 GetOriginalColorTypeObject(Color32 color)
        {
            return new Mercraft.Core.Unity.Color32(color.r, color.g, color.b, color.a);
        }
    }
}