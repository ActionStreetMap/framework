using System.Collections.Generic;
using System.Linq;
using ActionStreetMap.Core;
using ActionStreetMap.Explorer.Customization;
using ActionStreetMap.Explorer.Scene.Builders;
using ActionStreetMap.Infrastructure.Dependencies;
using ActionStreetMap.Maps.Data.Helpers;
using ActionStreetMap.Tests.Core.MapCss;
using Moq;
using NUnit.Framework;

namespace ActionStreetMap.Tests.Explorer.Customization
{
    [TestFixture]
    class CommonRuleExtensionsTests
    {
        [Test]
        public void CanGetMultiplyModelBuilders()
        {
            // ARRANGE
            var stylesheet = MapCssHelper.GetStylesheetFromContent("way[highway] { builders:test1,test2;}\n");
            var way = MapCssHelper.GetWay(new Dictionary<string, string>() { { "highway", "yes" } }.ToTags());
            var testBulder1 = new Mock<IModelBuilder>();
            testBulder1.SetupGet(b => b.Name).Returns("test1");
            var testBulder2 = new Mock<IModelBuilder>();
            testBulder2.SetupGet(b => b.Name).Returns("test2");
            var testBulder3 = new Mock<IModelBuilder>();
            testBulder3.SetupGet(b => b.Name).Returns("test2");
            var provider = new CustomizationService(new Container())
                .RegisterBuilder(testBulder1.Object)
                .RegisterBuilder(testBulder2.Object);
            var rule = stylesheet.GetModelRule(way, ZoomHelper.GetZoomLevel(RenderMode.Scene));

            // ACT
            var builders = rule.GetModelBuilders(provider).ToArray();

            // ASSERT
            Assert.AreEqual(2, builders.Length);
            Assert.AreEqual("test1", builders[0].Name);
            Assert.AreEqual("test2", builders[1].Name);
        }
    }
}
