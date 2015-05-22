using ActionStreetMap.Core;
using ActionStreetMap.Infrastructure.Config;

namespace ActionStreetMap.Explorer.Infrastructure
{
    /// <summary> Represents a builder responsible for application configuration creation. </summary>
    public class ConfigBuilder
    {
        private readonly CodeConfigSection _configSection = new CodeConfigSection();

        /// <summary> Adds value to configuration with path provided. </summary>
        /// <remarks> Use this method to extend application with your custom settings. </remarks>
        /// <typeparam name="T">Type of value.</typeparam>
        /// <param name="path">Path.</param>
        /// <param name="value">Value.</param>
        protected void Add<T>(string path, T value)
        {
            _configSection.Add(path, value);
        }

        /// <summary> Builds application specific configuration. </summary>
        public IConfigSection Build()
        {
            return _configSection;
        }

        #region Map data settings

        /// <summary> Sets map data path. </summary>
        public ConfigBuilder SetLocalMapData(string path)
        {
            Add<string>("data/map/local", path);
            return this;
        }

        /// <summary> Sets index settings path. </summary>
        public ConfigBuilder SetIndexSettings(string path)
        {
            Add<string>("data/map/index.settings", path);
            return this;
        }

        /// <summary> Sets remote server parameters. </summary>
        public ConfigBuilder SetRemoteMapData(string url, string schema, string format)
        {
            Add<string>("data/map/remote.server", url);
            Add<string>("data/map/remote.query", schema);
            Add<string>("data/map/remote.format", format);
            return this;
        }

        /// <summary> Sets local path to elevation data. </summary>
        public ConfigBuilder SetLocalElevationData(string path)
        {
            Add<string>("data/elevation/local", path);
            return this;
        }

        /// <summary> Sets settings to get elevation data from remote server. </summary>
        public ConfigBuilder SetRemoteElevationData(string url, string schema)
        {
            Add<string>("data/elevation/remote.server", url);
            Add<string>("data/elevation/remote.schema", schema);
            return this;
        }

        /// <summary> Sets mapcss path. </summary>
        public ConfigBuilder SetMapCss(string path)
        {
            Add<string>("mapcss", path);
            return this;
        }

        /// <summary> Sets geocoding server's url. </summary>
        public ConfigBuilder SetGeocodingServer(string url)
        {
            Add<string>("geocoding", url);
            return this;
        }

        /// <summary> Sets sandbox mode: on/off. </summary>
        public ConfigBuilder SetSandbox(bool isOn)
        {
            Add<bool>("sandbox", isOn);
            return this;
        }

        /// <summary> Sets tile settings. </summary>
        /// <param name="size">Size of tile.</param>
        /// <param name="offset">Sensivity offset.</param>
        /// <returns></returns>
        public ConfigBuilder SetTileSettings(float size, float offset)
        {
            Add<float>(@"tile/size", size);
            Add<float>(@"tile/offset", offset);

            return this;
        }
        /// <summary> Sets rendering mode. </summary>
        /// <param name="renderMode">Render mode.</param>
        /// <param name="overviewBuffer">Overview buffer count.</param>
        /// <returns></returns>
        public ConfigBuilder SetRenderMode(RenderMode renderMode, int overviewBuffer)
        {
            Add<string>(@"tile/render_mode", renderMode.ToString().ToLower());
            Add<int>(@"tile/overview_buffer", overviewBuffer);
            return this;
        }

        #endregion

        #region Default instance

        /// <summary> Gets ConfigBuilder with default settings. </summary>
        /// <remarks> You can call methods to override settings with custom ones. </remarks>
        public static ConfigBuilder GetDefault()
        {
            return new ConfigBuilder()
                .SetLocalMapData("Maps/osm")
                .SetIndexSettings("Config/index.json")
                .SetRemoteMapData("http://api.openstreetmap.org/api/0.6/map?bbox=", "{0},{1},{2},{3}", "xml")
                .SetLocalElevationData("Maps/elevation")
                .SetRemoteElevationData("http://dds.cr.usgs.gov/srtm/version2_1/SRTM3", "Config/srtm.schema.txt")
                .SetMapCss("Config/default.mapcss")
                .SetGeocodingServer("http://nominatim.openstreetmap.org/search")
                .SetSandbox(false)
                .SetTileSettings(400, 80)
                .SetRenderMode(RenderMode.Scene, 1);
        }

        #endregion
    }
}
