using System;
using System.Collections.Generic;

namespace ActionStreetMap.Explorer.Customization
{
    /// <summary> Represents texture atlas. </summary>
    public sealed class TextureAtlas
    {
        private readonly Dictionary<string, TextureGroup> _textureGroupMap;

        /// <summary> Creates instance of <see cref="TextureAtlas"/>. </summary>
        /// <param name="capacity"></param>
        public TextureAtlas(int capacity = 4)
        {
            _textureGroupMap = new Dictionary<string, TextureGroup>(capacity);
        }

        /// <summary> Adds texture group by name. </summary>
        public TextureAtlas Add(string name, TextureGroup @group)
        {
            _textureGroupMap.Add(name, @group);
            return this;
        }

        /// <summary> Gets texture group by name. </summary>
        public TextureGroup Get(string name)
        {
            if (!_textureGroupMap.ContainsKey(name))
                throw new ArgumentException(String.Format(Strings.TextureGroupIsNotRegistered, name), "name");

            return _textureGroupMap[name];
        }
    }
}
