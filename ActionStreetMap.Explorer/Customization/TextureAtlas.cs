using System.Collections.Generic;

namespace ActionStreetMap.Explorer.Customization
{
    public sealed class TextureAtlas
    {
        private readonly Dictionary<string, TexturePack> _texturePackMap;

        public string Name { get; private set; }

        public TextureAtlas(string name)
        {
            Name = name;
            _texturePackMap = new Dictionary<string, TexturePack>(2);
        }

        public TextureAtlas Register(TexturePack pack)
        {
            _texturePackMap.Add(pack.Name, pack);
            return this;
        }

        public TexturePack Get(string name)
        {
            return _texturePackMap[name];
        }
    }
}
