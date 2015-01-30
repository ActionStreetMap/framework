using ActionStreetMap.Core.Elevation;
using ActionStreetMap.Core.Tiling;
using ActionStreetMap.Explorer.Scene;
using ActionStreetMap.Explorer.Tiling;
using ActionStreetMap.Infrastructure.Bootstrap;
using ActionStreetMap.Infrastructure.Dependencies;
using ActionStreetMap.Explorer.Scene.Utils;
using ActionStreetMap.Maps;
using ActionStreetMap.Maps.Data;
using ActionStreetMap.Maps.Data.Elevation;
using ActionStreetMap.Maps.Data.Search;
using ActionStreetMap.Maps.GeoCoding;
using ActionStreetMap.Maps.Geocoding;

namespace ActionStreetMap.Explorer.Bootstrappers
{
    /// <summary>
    ///     Register tile processing classes.
    /// </summary>
    public class TileBootstrapper : BootstrapperPlugin
    {
        private const string TileKey = "tile";
        private const string ElevationKey = @"data/elevation";
        private const string MapDataKey = @"data/map";

        /// <inheritdoc />
        public override string Name { get { return "tile"; } }

        /// <inheritdoc />
        public override bool Run()
        {
            Container.Register(Component
               .For<IElementSourceProvider>()
               .Use<ElementSourceProvider>()
               .Singleton()
               .SetConfig(GlobalConfigSection.GetSection(MapDataKey)));

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
                .For<ITilePositionObserver>()
                .Use<TileManager>()
                .Singleton()
                .SetConfig(GlobalConfigSection.GetSection(TileKey)));

            // provides text search feature
            Container.Register(Component.For<ISearchEngine>().Use<SearchEngine>().Singleton());

            Container.Register(Component.For<IGeocoder>().Use<NominatimGeocoder>().Singleton());

            Container.Register(Component.For<HeightMapProcessor>().Use<HeightMapProcessor>().Singleton());
            
            return true;
        }
    }
}