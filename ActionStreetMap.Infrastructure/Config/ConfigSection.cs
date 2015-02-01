using System;
using System.Collections.Generic;
using System.Linq;
using ActionStreetMap.Infrastructure.Formats.Json;
using ActionStreetMap.Infrastructure.IO;

namespace ActionStreetMap.Infrastructure.Config
{
    /// <summary> Represens a JSON config entry. </summary>
    public class ConfigSection : IConfigSection
    {
        /// <summary> Gets root element of this section. </summary>
        public ConfigElement RootElement { get; private set; }

        /// <summary> Creates ConfigSection. </summary>
        /// <param name="element">Config element.</param>
        public ConfigSection(ConfigElement element)
        {
            RootElement = element;
        }

        /// <summary> Creates ConfigSection. </summary>
        /// <param name="appConfigFileName">Config appConfig.</param>
        /// <param name="fileSystemService">File system service</param>
        public ConfigSection(string appConfigFileName, IFileSystemService fileSystemService)
        {
            var jsonStr = fileSystemService.ReadText(appConfigFileName);
            var json = JSON.Parse(jsonStr);
            RootElement = new ConfigElement(json);
        }

        /// <summary> Creates ConfigSection. </summary>
        /// <param name="content">Json content</param>
        public ConfigSection(string content)
        {
            RootElement = new ConfigElement(JSON.Parse(content));
        }

        /// <inheritdoc />
        public IEnumerable<IConfigSection> GetSections(string xpath)
        {
            return RootElement.GetElements(xpath).Select(e => (new ConfigSection(e)) as IConfigSection);
        }

        /// <inheritdoc />
        public IConfigSection GetSection(string xpath)
        {
            return new ConfigSection(new ConfigElement(RootElement.Node, xpath));
        }

        /// <inheritdoc />
        public bool IsEmpty { get { return RootElement.IsEmpty; } }

        /// <inheritdoc />
        public string GetString(string xpath, string defaultValue)
        {
            return new ConfigElement(RootElement.Node, xpath).GetString(defaultValue);
        }

        /// <inheritdoc />
        public int GetInt(string xpath, int defaultValue)
        {
            return new ConfigElement(RootElement.Node, xpath).GetInt(defaultValue);
        }

        /// <inheritdoc />
        public float GetFloat(string xpath, float defaultValue)
        {
            return new ConfigElement(RootElement.Node, xpath).GetFloat(defaultValue);
        }

        /// <inheritdoc />
        public bool GetBool(string xpath, bool defaultValue)
        {
            return new ConfigElement(RootElement.Node, xpath).GetBool(defaultValue);
        }

        /// <inheritdoc />
        public Type GetType(string xpath)
        {
            return (new ConfigElement(RootElement.Node, xpath)).GetType();
        }

        /// <inheritdoc />
        public T GetInstance<T>(string xpath)
        {
            return (T) Activator.CreateInstance(GetType(xpath));
        }

        /// <inheritdoc />
        public T GetInstance<T>(string xpath, params object[] args)
        {
            return (T) Activator.CreateInstance(GetType(xpath), args);
        }
    }
}