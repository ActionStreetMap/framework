
using ActionStreetMap.Core;
using ActionStreetMap.Explorer;
using ActionStreetMap.Explorer.Bootstrappers;
using ActionStreetMap.Infrastructure.Bootstrap;
using ActionStreetMap.Infrastructure.Config;
using ActionStreetMap.Infrastructure.Dependencies;
using ActionStreetMap.Infrastructure.Diagnostic;
using ActionStreetMap.Infrastructure.IO;
using ActionStreetMap.Tests.Explorer.Tiles;
using ActionStreetMap.Unity.IO;

namespace ActionStreetMap.Tests
{
    public static class TestHelper
    {
        public static readonly GeoCoordinate BerlinTestFilePoint = new GeoCoordinate(52.54994964,13.35064315);
        public static readonly GeoCoordinate BerlinInvalidenStr = new GeoCoordinate(52.531036, 13.384866);
        public static readonly GeoCoordinate BerlinHauptBanhoff = new GeoCoordinate(52.5254967, 13.3733636);
        public static readonly GeoCoordinate BerlinTiergarten = new GeoCoordinate(52.516809, 13.367598);
        public static readonly GeoCoordinate BerlinVolksPark = new GeoCoordinate(52.526437, 13.432122);

        public static readonly GeoCoordinate MoscowRedSquare = new GeoCoordinate(55.7537315, 37.6198537);

        public const string ConfigTestRootFile = "test.json";
        public const string ConfigAppRootFile = @"..\..\..\..\Tests\TestAssets\DemoResources\Config\settings.json";


        public const string TestNmeaFilePath = @"..\..\..\..\Tests\TestAssets\Nmea\berlin_seestr_speed_increasing.nme";
        public const string TestIndexSettingsPath = @"..\..\..\..\Tests\TestAssets\DemoResources\Config\index.json";

        public const string MapDataPath = @"Maps\osm";

        public const string TestThemeFile = @"..\..\..\..\Tests\TestAssets\Themes\theme.json";
        public const string TestBaseMapcssFile = @"..\..\..\..\Tests\TestAssets\Mapcss\base.mapcss";
        public const string DefaultMapcssFile = @"..\..\..\..\Tests\TestAssets\DemoResources\Config\default.mapcss";

        public const string BerlinXmlData = @"..\..\..\..\Tests\TestAssets\Osm\berlin_xml_api.osm";

        public static GameRunner GetGameRunner()
        {
            return GetGameRunner(new Container());
        }

        public static GameRunner GetGameRunner(IContainer container)
        {
            // these items are used during boot process
            var jsonConfigSection = GetJsonConfig(ConfigAppRootFile);

            container.Register(Component.For<ITrace>().Use<ConsoleTrace>().Singleton());
            container.Register(Component.For<IPathResolver>().Use<TestPathResolver>().Singleton());
            container.Register(Component.For<IMessageBus>().Use<MessageBus>().Singleton());
            return new GameRunner(container, jsonConfigSection)
                .RegisterPlugin<TestBootstrapperPlugin>("test")
                .Bootstrap();
        }

        public static JsonConfigSection GetJsonConfig(string configPath)
        {
            return new JsonConfigSection(new FileSystemService(new TestPathResolver(), new DefaultTrace())
                .ReadText(configPath));
        }

        public static IFileSystemService GetFileSystemService()
        {
            return new FileSystemService(new TestPathResolver(), new ConsoleTrace());
        }
    }
}
