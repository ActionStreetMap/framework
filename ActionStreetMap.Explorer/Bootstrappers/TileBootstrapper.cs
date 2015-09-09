using ActionStreetMap.Core;
using ActionStreetMap.Core.Tiling;
using ActionStreetMap.Explorer.Infrastructure;
using ActionStreetMap.Explorer.Scene.Terrain;
using ActionStreetMap.Explorer.Tiling;
using ActionStreetMap.Infrastructure.Dependencies;
using ActionStreetMap.Maps;
using ActionStreetMap.Maps.Data;
using ActionStreetMap.Maps.Data.Elevation;
using ActionStreetMap.Maps.Data.Search;
using ActionStreetMap.Maps.GeoCoding;
using ActionStreetMap.Maps.Geocoding;

namespace ActionStreetMap.Explorer.Bootstrappers
{
    /// <summary> Register tile processing classes. </summary>
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
            var mapDataConfig = GlobalConfigSection.GetSection(MapDataKey);
            var tileConfig = GlobalConfigSection.GetSection(TileKey);

            // responsible for choosing of OSM data provider
            Container.Register(Component.For<IElementSourceProvider>().Use<ElementSourceProvider>()
                .Singleton().SetConfig(mapDataConfig));

            // responsible for map index maintanence
            Container.Register(Component.For<MapIndexUtility>().Use<MapIndexUtility>().Singleton()
                .SetConfig(mapDataConfig));

            // loads map data for given tile
            Container.Register(Component.For<ITileLoader>().Use<MapTileLoader>().Singleton());

            // activates/deactivates tiles during the game based on distance to player
            Container.Register(Component.For<ITileActivator>().Use<TileActivator>().Singleton());

            // provides elevation data.
            Container.Register(Component.For<IElevationProvider>().Use<SrtmElevationProvider>()
                .Singleton().SetConfig(GlobalConfigSection.GetSection(ElevationKey)));
            
            // responsible for listening position changes and loading tiles.
            Container.Register(Component.For<ITileController>().Use<TileController>().Singleton()
                .SetConfig(tileConfig));

            // provides text search feature.
            Container.Register(Component.For<ISearchEngine>().Use<SearchEngine>().Singleton());
            
            // provides geocoding features.
            Container.Register(Component.For<IGeocoder>().Use<NominatimGeocoder>().Singleton());

            // terrain
            Container.Register(Component.For<ITerrainBuilder>().Use<TerrainBuilder>().Singleton()
                .SetConfig(tileConfig));

            // editor
            Container.Register(Component.For<IElementSourceEditor>().Use<ElementSourceEditor>().Singleton());
            Container.Register(Component.For<ITileModelEditor>().Use<TileModelEditor>().Singleton());

            return true;
        }
    }
}