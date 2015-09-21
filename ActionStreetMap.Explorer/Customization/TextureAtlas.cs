using System.Collections.Generic;

namespace ActionStreetMap.Explorer.Customization
{
    /// <summary> Represents texture atlas. </summary>
    public sealed class TextureAtlas
    {
        private readonly Dictionary<string, TexturePack> _texturePackMap;

        /// <summary> Creates instance of <see cref="TextureAtlas"/>. </summary>
        /// <param name="capacity"></param>
        public TextureAtlas(int capacity = 4)
        {
            _texturePackMap = new Dictionary<string, TexturePack>(capacity);
        }

        /// <summary> Registers texture pack by name. </summary>
        public TextureAtlas Register(string name, TexturePack pack)
        {
            _texturePackMap.Add(name, pack);
            return this;
        }

        /// <summary> Gets texture pack by name. </summary>
        public TexturePack Get(string name)
        {
            return _texturePackMap[name];
        }
    }
}
