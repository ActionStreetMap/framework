using System.Collections.Generic;
using UnityEngine;

namespace ActionStreetMap.Explorer.Customization
{
    /// <summary> Represents texture group. </summary>
    /// <remarks> Not thread safe. </remarks>
    public sealed class TextureGroup
    {
        private readonly float _xRatio;
        private readonly float _yRatio;
        private readonly List<Texture> _textures;

        /// <summary> Creates instance of <see cref="TextureGroup"/>. </summary>
        /// <param name="width"> Texture width. </param>
        /// <param name="height"> Texture height. </param>
        /// <param name="capacity"> Internal dictionary capacity. </param>
        public TextureGroup(int width, int height, int capacity = 1)
        {
            _xRatio = 1 / (float) width;
            _yRatio = 1 / (float) height;
            _textures = new List<Texture>(capacity);
        }

        /// <summary> Stores texture in atlas. </summary>
        public TextureGroup Add(int x, int y, int width, int height)
        {
            _textures.Add(new Texture(x * _xRatio, y * _yRatio,
                width * _xRatio, height * _yRatio));
            return this;
        }

        /// <summary> Gets texture region using seed provided.  </summary>
        public Texture Get(int seed)
        {
            return _textures[seed % _textures.Count];
        }

        /// <summary> Represents texture in atlas. </summary>
        public sealed class Texture
        {
            private readonly float _x;
            private readonly float _y;

            private readonly float _width;
            private readonly float _height;

            internal Texture(float x, float y, float width, float height)
            {
                _x = x;
                _y = y;
                _width = width;
                _height = height;
            }

            /// <summary> Maps relative uv coordinate to absolute in atlas. </summary>
            public Vector2 Map(Vector2 relative)
            {
                return new Vector2(_x + _width * relative.x, _y + _height * relative.y);
            }
        }
    }
}
