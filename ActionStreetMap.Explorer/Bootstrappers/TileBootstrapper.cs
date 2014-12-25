using ActionStreetMap.Core;
using ActionStreetMap.Core.Elevation;
using ActionStreetMap.Core.Scene;
using ActionStreetMap.Explorer.Scene;
using ActionStreetMap.Infrastructure.Bootstrap;
using ActionStreetMap.Infrastructure.Dependencies;
using ActionStreetMap.Osm;
using ActionStreetMap.Osm.Index;

namespace ActionStreetMap.Explorer.Bootstrappers
{
    /// <summary>
    ///     Register tile processing classes.
    /// </summary>
    public class TileBootstrapper : BootstrapperPlugin
    {
        private const string TileKey = "tile";
        private const string ElevationKey = "elevationdata";
        private const string DataSourceProviderKey = "mapdata";

        /// <inheritdoc />
        public override string Name { get { return "tile"; } }

        /// <inheritdoc />
        public override bool Run()
        {
            Container.Register(Component
               .For<IElementSourceProvider>()
               .Use<ElementSourceProvider>()
               .Singleton()
               .SetConfig(GlobalConfigSection.GetSection(DataSourceProviderKey)));

            Container.Register(Component.For<ITileLoader>().Use<MapTileLoader>().Singleton());

            // activates/deactivates tiles during the game based on distance to player
            Container.Register(Component.For<ITileActivator>().Use<TileActivator>().Singleton());

            Container.Register(Component
                .For<IHeightMapProvider>()
                .Use<HeightMapProvider>()
                .Singleton()
                .SetConfig(GlobalConfigSection.GetSection(TileKey)));

            Container.Register(Component.For<IElevationProvider>().Use<SrtmElevationProvider>().Singleton()
                .SetConfig(GlobalConfigSection.GetSection(ElevationKey)));
            
            Container.Register(Component
                .For<IPositionListener>()
                .Use<TileManager>()
                .Singleton()
                .SetConfig(GlobalConfigSection.GetSection(TileKey)));
            
            return true;
        }
    }
}