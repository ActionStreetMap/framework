
using ActionStreetMap.Core;
using ActionStreetMap.Explorer;
using ActionStreetMap.Explorer.Bootstrappers;
using ActionStreetMap.Infrastructure.Bootstrap;
using ActionStreetMap.Infrastructure.Config;
using ActionStreetMap.Infrastructure.Dependencies;
using ActionStreetMap.Infrastructure.IO;
using ActionStreetMap.Tests.Explorer.Tiles.Stubs;

namespace ActionStreetMap.Tests
{
    public static class TestHelper
    {
        public static readonly GeoCoordinate BerlinTestFilePoint = new GeoCoordinate(52.54994964,13.35064315);
        public static readonly GeoCoordinate BerlinInvalidenStr = new GeoCoordinate(52.531036, 13.384866);
        public static readonly GeoCoordinate BerlinHauptBanhoff = new GeoCoordinate(52.5254967, 13.3733636);
        public static readonly GeoCoordinate BerlinTiergarten = new GeoCoordinate(52.516809, 13.367598);
        public static readonly GeoCoordinate BerlinVolksPark = new GeoCoordinate(52.526437, 13.432122);

        public const string ConfigTestRootFile = "test.json";
        public const string ConfigAppRootFile = @"..\..\..\..\Tests\TestAssets\DemoResources\Config\settings.json";

        public const string TestNmeaFilePath = @"..\..\..\..\Tests\TestAssets\Nmea\berlin_seestr_speed_increasing.nme";
        public const string TestIndexSettingsPath = @"..\..\..\..\Tests\TestAssets\DemoResources\Config\themes\default\index.json";

        public const string MapDataPath = @"Maps\osm";

        public const string TestThemeFile = @"..\..\..\..\Tests\TestAssets\Themes\theme.json";
        public const string TestBaseMapcssFile = @"..\..\..\..\Tests\TestAssets\Mapcss\base.mapcss";
        public const string DefaultMapcssFile = @"..\..\..\..\Tests\TestAssets\DemoResources\Config\themes\default\default.mapcss";

        public static GameRunner GetGameRunner()
        {
            return GetGameRunner(new Container());
        }

        public static GameRunner GetGameRunner(IContainer container)
        {
            return GetGameRunner(container, new MessageBus());
        }

        public static GameRunner GetGameRunner(IContainer container, MessageBus messageBus)
        {
            // these items are used during boot process
            var pathResolver = new TestPathResolver();
            container.RegisterInstance<IPathResolver>(pathResolver);
            var fileSystemService = new FileSystemService(pathResolver);
            container.RegisterInstance<IFileSystemService>(GetFileSystemService());

            container.RegisterInstance<IConfigSection>(new ConfigSection(ConfigAppRootFile, fileSystemService));

            // actual boot service
            container.Register(Component.For<IBootstrapperService>().Use<BootstrapperService>());

            // boot plugins
            container.Register(Component.For<IBootstrapperPlugin>().Use<InfrastructureBootstrapper>().Named("infrastructure"));
            container.Register(Component.For<IBootstrapperPlugin>().Use<TileBootstrapper>().Named("tile"));
            container.Register(Component.For<IBootstrapperPlugin>().Use<SceneBootstrapper>().Named("scene"));
            container.Register(Component.For<IBootstrapperPlugin>().Use<TestBootstrapperPlugin>().Named("test"));

            return new GameRunner(container, messageBus);
        }

        public static IFileSystemService GetFileSystemService()
        {
            return new FileSystemService(new TestPathResolver());
        }
    }
}
