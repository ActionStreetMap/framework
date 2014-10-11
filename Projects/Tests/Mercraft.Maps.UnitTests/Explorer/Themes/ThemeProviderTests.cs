﻿using System.Collections.Generic;
using Mercraft.Core.Utilities;
using Mercraft.Core.World.Buildings;
using Mercraft.Core.World.Infos;
using Mercraft.Core.World.Roads;
using Mercraft.Explorer.Themes;
using Mercraft.Infrastructure.Config;
using Mercraft.Models.Buildings.Facades;
using Mercraft.Models.Buildings.Roofs;
using NUnit.Framework;

namespace Mercraft.Maps.UnitTests.Explorer.Themes
{
    [TestFixture]
    public class ThemeProviderTests
    {
        [Test]
        public void CanGetBuildingStyle()
        {
            // ARRANGE
            var provider = GetThemeProvider();
            provider.Configure(GetTestThemeConfig());

            // ACT
            var theme = provider.Get();
            Assert.IsNotNull(theme);

            var style = theme.GetBuildingStyle(new Building()
            {
                Type = "residential"
            });

            // ASSERT
            Assert.IsNotNull(style);
            Assert.IsNotNull(style.Facade);

            Assert.AreEqual(9, style.Facade.Height);
            Assert.AreEqual(6, style.Facade.Width);

            Assert.AreEqual("Materials/Buildings/facades/residential_1", style.Facade.Path);
            Assert.AreEqual("brick", style.Facade.Material);
            Assert.AreEqual(ColorUtility.FromName("red"), style.Facade.Color);
            Assert.IsNotNull(style.Facade.FrontUvMap);
            Assert.IsNull(style.Facade.BackUvMap);
            Assert.IsNull(style.Facade.SideUvMap);
            Assert.IsNotNull(style.Facade.Builders);

            Assert.IsNotNull(style.Roof);
            Assert.AreEqual("Materials/Buildings/roofs/residential_1", style.Roof.Path);
            Assert.AreEqual("brick", style.Roof.Material);
            Assert.AreEqual(ColorUtility.FromName("red"), style.Roof.Color);
            Assert.IsNotNull(style.Roof.FrontUvMap);
            Assert.IsNull(style.Roof.SideUvMap);
            Assert.IsNotNull(style.Roof.Builders);
        }

        [Test]
        public void CanGetRoadStyle()
        {
            // ARRANGE
            var provider = GetThemeProvider();
            provider.Configure(GetTestThemeConfig());

            // ACT
            var theme = provider.Get();
            Assert.IsNotNull(theme);

            var style = theme.GetRoadStyle(new Road()
            {
                Elements = new List<RoadElement>()
                {
                    new RoadElement()
                    {
                        Type = "residental"
                    }
                }
            });

            // ASSERT
            Assert.AreEqual("Materials/Roads/default_1", style.Path);
            Assert.AreEqual("asphalt", style.Material);
            Assert.AreEqual(ColorUtility.FromName("gray"), style.Color);
            Assert.IsNotNull(style.MainUvMap);
            Assert.IsNull(style.TurnUvMap);

        }

        [Test]
        public void CanGetInfo()
        {
            // ARRANGE
            var provider = GetThemeProvider();
            provider.Configure(GetTestThemeConfig());

            // ACT
            var theme = provider.Get();
            Assert.IsNotNull(theme);

            var style = theme.GetInfoStyle(new Info()
            {
                Key = "accommodation_camping"
            });

            // ASSERT
            Assert.AreEqual("Materials/Infos/default_1", style.Path);
            Assert.IsNotNull(style.UvMap);
        }

        private ThemeProvider GetThemeProvider()
        {
            return new ThemeProvider(
                new TestPathResolver(), 
                new List<IFacadeBuilder>()
                {
                   new FlatFacadeBuilder()
                }, 
                new List<IRoofBuilder>()
                {
                    new FlatRoofBuilder()
                });
        }

        private IConfigSection GetTestThemeConfig()
        {
            return new ConfigSection(TestHelper.TestThemeFile, TestHelper.GetPathResolver());
        }
    }
}
