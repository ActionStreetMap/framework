using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using ActionStreetMap.Core;
using ActionStreetMap.Core.MapCss;
using ActionStreetMap.Core.MapCss.Domain;
using ActionStreetMap.Core.Tiling.Models;
using ActionStreetMap.Infrastructure.Config;
using Moq;

namespace ActionStreetMap.Tests.Core.MapCss
{
    public static class MapCssHelper
    {
        public static Stylesheet GetStylesheetFromContent(string content, bool canUseExprTree = true)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(content);
            writer.Flush();
            stream.Position = 0;
            var provider = new StylesheetProvider(stream, canUseExprTree);
            return provider.Get();
        }

        public static Stylesheet GetStylesheetFromFile(string file, bool canUseExprTree = true)
        {
            var provider = new StylesheetProvider(file, TestHelper.GetFileSystemService());
            var config = new Mock<IConfigSection>();
            config.Setup(c => c.GetBool(It.IsAny<string>())).Returns(canUseExprTree);
            config.Setup(c => c.GetString(It.IsAny<string>())).Returns(file);
            provider.Configure(config.Object);
            return provider.Get();
        }

        public static IList<Style> GetStyles(Stylesheet stylesheet)
        {
            // NOTE don't want to expose these fieds as public API, so use reflection in tests
            Func<string, IList<Style>> getter = name =>
            {
                var sField = typeof(Stylesheet).GetField("_styles", BindingFlags.NonPublic |
                    BindingFlags.Instance | BindingFlags.GetField);
                var sFieldValue = (sField.GetValue(stylesheet) as StyleCollection);

                var field = typeof(StyleCollection).GetField(name, BindingFlags.NonPublic |
                   BindingFlags.Instance | BindingFlags.GetField);
                return (field.GetValue(sFieldValue) as List<Style>);
            };

            var result = new List<Style>();
            result.AddRange(getter("_canvasStyles"));
            result.AddRange(getter("_areaStyles"));
            result.AddRange(getter("_wayStyles"));
            result.AddRange(getter("_nodeStyles"));

            return result;
        }

        public static Canvas GetCanvas()
        {
            return new Canvas();
        }

        public static Area GetArea(Dictionary<string, string> tags)
        {
            return new Area
            {
                Points = new List<GeoCoordinate>()
                {
                    new GeoCoordinate(0, 0),
                    new GeoCoordinate(1, 0),
                    new GeoCoordinate(1, 0),
                    new GeoCoordinate(0, 0)
                },
                Tags = tags
            };
        }

        public static Way GetWay(Dictionary<string, string> tags)
        {
            return new Way
            {
                Points = new List<GeoCoordinate>()
                {
                    new GeoCoordinate(0, 0),
                    new GeoCoordinate(1, 0),
                    new GeoCoordinate(1, 0),
                    new GeoCoordinate(0, 0)
                },
                Tags = tags
            };
        }

        public static Node GetNode(Dictionary<string, string> tags)
        {
            return new Node
            {
                Point = new GeoCoordinate(0, 0),
                Tags = tags
            };
        }
    }
}