using System;
using System.Collections.Generic;
using System.Linq;
using ActionStreetMap.Infrastructure.Formats.Json;
using ActionStreetMap.Infrastructure.IO;

namespace ActionStreetMap.Infrastructure.Config
{
    /// <summary> Represens a JSON config entry. </summary>
    public class JsonConfigSection : IConfigSection
    {
        /// <summary> Gets root element of this section. </summary>
        private ConfigElement _rootElement;

        /// <summary> Creates <see cref="JsonConfigSection"/>. </summary>
        /// <param name="element">Config element.</param>
        private JsonConfigSection(ConfigElement element)
        {
            _rootElement = element;
        }

        /// <summary> Creates <see cref="JsonConfigSection"/>. </summary>
        /// <param name="appConfigFileName">Config appConfig.</param>
        /// <param name="fileSystemService">File system service</param>
        public JsonConfigSection(string appConfigFileName, IFileSystemService fileSystemService)
        {
            var jsonStr = fileSystemService.ReadText(appConfigFileName);
            var json = JSON.Parse(jsonStr);
            _rootElement = new ConfigElement(json);
        }

        /// <summary> Creates <see cref="JsonConfigSection"/>. </summary>
        /// <param name="content">Json content</param>
        public JsonConfigSection(string content)
        {
            _rootElement = new ConfigElement(JSON.Parse(content));
        }

        /// <inheritdoc />
        public IEnumerable<IConfigSection> GetSections(string xpath)
        {
            return _rootElement.GetElements(xpath).Select(e => (new JsonConfigSection(e)) as IConfigSection);
        }

        /// <inheritdoc />
        public IConfigSection GetSection(string xpath)
        {
            return new JsonConfigSection(new ConfigElement(_rootElement.Node, xpath));
        }

        /// <inheritdoc />
        public bool IsEmpty { get { return _rootElement.IsEmpty; } }

        /// <inheritdoc />
        public string GetString(string xpath, string defaultValue)
        {
            return new ConfigElement(_rootElement.Node, xpath).GetString(defaultValue);
        }

        /// <inheritdoc />
        public int GetInt(string xpath, int defaultValue)
        {
            return new ConfigElement(_rootElement.Node, xpath).GetInt(defaultValue);
        }

        /// <inheritdoc />
        public float GetFloat(string xpath, float defaultValue)
        {
            return new ConfigElement(_rootElement.Node, xpath).GetFloat(defaultValue);
        }

        /// <inheritdoc />
        public bool GetBool(string xpath, bool defaultValue)
        {
            return new ConfigElement(_rootElement.Node, xpath).GetBool(defaultValue);
        }

        /// <inheritdoc />
        public Type GetType(string xpath)
        {
            return (new ConfigElement(_rootElement.Node, xpath)).GetType();
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

        private class ConfigElement
        {
            private readonly string _xpath;
            private JSONNode _node;

            /// <summary> Returns current JSON node. </summary>
            public JSONNode Node { get { return _node; } }

            /// <summary> True if element represents json node. </summary>
            public bool IsNode { get { return _node != null; } }

            /// <summary> Trues if is empty. </summary>
            public bool IsEmpty { get { return !IsNode; } }

            /// <summary> Creates ConfigElement. </summary>
            /// <param name="node">Node.</param>
            public ConfigElement(JSONNode node)
            {
                _node = node;
            }

            /// <summary> Creates ConfigElement.</summary>
            /// <param name="node">Node.</param>
            /// <param name="xpath">XPath</param>
            public ConfigElement(JSONNode node, string xpath)
            {
                _node = node;
                _xpath = xpath;

                Initialize();
            }

            private void Initialize()
            {
                try
                {
                    string[] paths = _xpath.Split('/');

                    JSONNode current = _node;

                    if (_xpath == "")
                        return;

                    for (int i = 0; i < paths.Length; i++)
                    {
                        current = current[(paths[i])];
                        if (current == null)
                            break;
                    }

                    _node = current;
                }
                catch (Exception ex)
                {
                    throw new ArgumentException(
                        String.Format("Unable to process xml. xpath:{0}\n node:{1}", _xpath, _node), ex);
                }
            }

            /// <summary> Returns the set of elements. </summary>
            public IEnumerable<ConfigElement> GetElements(string xpath)
            {
                if (Node == null)
                    return Enumerable.Empty<ConfigElement>();

                string[] paths = xpath.Split('/');
                int last = paths.Length - 1;
                JSONNode current = Node;
                for (int i = 0; i < last; i++)
                {
                    current = current[paths[i]];
                    //xpath isn't valid
                    if (current == null)
                        return Enumerable.Empty<ConfigElement>();
                }

                return
                    from JSONNode node in current[paths[last]].AsArray
                    select new ConfigElement(node);
            }

            /// <summary> Returns string. </summary>
            public string GetString(string defaultValue)
            {
                return IsNode ? _node.Value : defaultValue;
            }

            /// <summary> Returns int. </summary>
            public int GetInt(int defaultValue)
            {
                int value;
                return int.TryParse(GetString(null), out value) ? value : defaultValue;
            }

            /// <summary> Returns float. </summary>
            public float GetFloat(float defaultValue)
            {
                float value;
                return float.TryParse(GetString(null), out value) ? value : defaultValue;
            }

            /// <summary> Returns boolean. </summary>
            public bool GetBool(bool defaultValue)
            {
                bool value;
                return bool.TryParse(GetString(null), out value) ? value : defaultValue;
            }

            /// <summary> Returns type. </summary>
            public new Type GetType()
            {
                return Type.GetType(GetString(null));
            }
        }
    }
}