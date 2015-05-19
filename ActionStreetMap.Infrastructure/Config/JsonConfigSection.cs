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
        private readonly JSONNode _rootNode;

        /// <summary> Creates <see cref="JsonConfigSection"/>. </summary>
        /// <param name="content">Json content</param>
        public JsonConfigSection(string content)
        {
            _rootNode = JSON.Parse(content);
        }

        /// <summary> Creates <see cref="JsonConfigSection"/>. </summary>
        /// <param name="node">Json node.</param>
        public JsonConfigSection(JSONNode node)
        {
            _rootNode = node;
        }

        /// <inheritdoc />
        public IEnumerable<IConfigSection> GetSections(string xpath)
        {
            return GetElements(_rootNode, xpath).Select(e => (new JsonConfigSection(e)) as IConfigSection);
        }

        /// <inheritdoc />
        public IConfigSection GetSection(string xpath)
        {
            return new JsonConfigSection(GetNode(_rootNode, xpath));
        }

        /// <inheritdoc />
        public string GetString(string xpath, string defaultValue)
        {
            return _rootNode != null ? GetNode(_rootNode, xpath).Value : defaultValue;
        }

        /// <inheritdoc />
        public int GetInt(string xpath, int defaultValue)
        {
            int value;
            return int.TryParse(GetString(xpath, null), out value) ? value : defaultValue;
        }

        /// <inheritdoc />
        public float GetFloat(string xpath, float defaultValue)
        {
            float value;
            return float.TryParse(GetString(xpath, null), out value) ? value : defaultValue;
        }

        /// <inheritdoc />
        public bool GetBool(string xpath, bool defaultValue)
        {
            bool value;
            return bool.TryParse(GetString(xpath, null), out value) ? value : defaultValue;
        }

        /// <inheritdoc />
        public Type GetType(string xpath)
        {
            return Type.GetType(GetString(xpath, null));
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

        private static JSONNode GetNode(JSONNode node, string xpath)
        {
            try
            {
                string[] paths = xpath.Split('/');
                JSONNode current = node;

                if (xpath == "") return node;

                for (int i = 0; i < paths.Length; i++)
                {
                    current = current[(paths[i])];
                    if (current == null) break;
                }

                return current;
            }
            catch (Exception ex)
            {
                throw new ArgumentException(
                    String.Format("Unable to process xml. xpath:{0}\n node:{1}", xpath, node), ex);
            }
        }

        private static IEnumerable<JSONNode> GetElements(JSONNode node, string xpath)
        {
            if (node == null)
                return Enumerable.Empty<JSONNode>();

            string[] paths = xpath.Split('/');
            int last = paths.Length - 1;
            JSONNode current = node;
            for (int i = 0; i < last; i++)
            {
                current = current[paths[i]];
                //xpath isn't valid
                if (current == null)
                    return Enumerable.Empty<JSONNode>();
            }

            return 
                from JSONNode n in current[paths[last]].AsArray
                select n;
        }
    }
}