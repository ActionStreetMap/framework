using ActionStreetMap.Infrastructure.Bootstrap;
using ActionStreetMap.Infrastructure.Dependencies;
using ActionStreetMap.Osm.Index;

namespace ActionStreetMap.Explorer.Bootstrappers
{
    /// <summary>
    ///     Register OSM-specific classes.
    /// </summary>
    public class OsmBootstrapper: BootstrapperPlugin
    {
        private const string DataSourceProviderKey = "mapdata";

        /// <inheritdoc />
        public override string Name { get { return "osm"; } }

        /// <inheritdoc />
        public override bool Run()
        {
            Container.Register(Component
               .For<IElementSourceProvider>()
               .Use<ElementSourceProvider>()
               .Singleton()
               .SetConfig(GlobalConfigSection.GetSection(DataSourceProviderKey)));

            return true;
        }
    }
}
