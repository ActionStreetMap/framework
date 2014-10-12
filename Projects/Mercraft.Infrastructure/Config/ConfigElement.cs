﻿using System;
using System.Collections.Generic;
using System.Linq;
using Mercraft.Infrastructure.Formats.Json;

namespace Mercraft.Infrastructure.Config
{
    /// <summary>
    ///     Represens a single element of xml
    /// </summary>
    public class ConfigElement
    {
        private readonly string _xpath;
        private JSONNode _node;

        public ConfigElement(JSONNode node)
        {
            _node = node;
        }

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

        /// <summary>
        ///     Returns the set of elements
        /// </summary>
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

        /// <summary>
        ///     Returns string
        /// </summary>
        public string GetString()
        {
            return IsNode ? _node.Value : null;
        }

        /// <summary>
        ///     Returns int
        /// </summary>
        public int GetInt()
        {
            return int.Parse(GetString());
        }

        /// <summary>
        ///     Returns float
        /// </summary>
        public float GetFloat()
        {
            return float.Parse(GetString());
        }

        /// <summary>
        ///     Returns boolean
        /// </summary>
        public bool GetBool()
        {
            return bool.Parse(GetString());
        }

        /// <summary>
        ///     Returns type
        /// </summary>
        public new Type GetType()
        {
            string typeName = GetString();
            return Type.GetType(typeName);
        }

        /// <summary>
        ///     Returns current XElement
        /// </summary>
        public JSONNode Node
        {
            get { return _node; }
        }

        /// <summary>
        ///     true if element represents xml node
        /// </summary>
        public bool IsNode
        {
            get { return _node != null; }
        }

        public bool IsEmpty
        {
            get { return !IsNode; }
        }
    }
}