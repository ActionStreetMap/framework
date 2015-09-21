using System.Collections.Generic;
using UnityEngine;

namespace ActionStreetMap.Explorer.Customization
{
    /// <summary> Represents texture pack. </summary>
    /// <remarks> Not thread safe. </remarks>
    public sealed class TexturePack
    {
        private readonly float _xRatio;
        private readonly float _yRatio;
        private readonly List<TextureRegion> _textures;

        /// <summary> Creates instance of <see cref="TexturePack"/>. </summary>
        /// <param name="width"> Texture width. </param>
        /// <param name="height"> Texture height. </param>
        /// <param name="capacity"> Internal dictionary capacity. </param>
        public TexturePack(int width, int height, int capacity = 1)
        {
            _xRatio = 1 / (float) width;
            _yRatio = 1 / (float) height;
            _textures = new List<TextureRegion>(capacity);
        }

        /// <summary> Stores texture region with given parameters. </summary>
        public TexturePack Add(int x, int y, int width, int height)
        {
            _textures.Add(new TextureRegion(x * _xRatio, y * _yRatio,
                width * _xRatio, height * _yRatio));
            return this;
        }

        /// <summary> Gets texture region using seed provided.  </summary>
        public TextureRegion Get(int seed)
        {
            return _textures[seed % _textures.Count];
        }

        /// <summary> Represents texture region. </summary>
        public sealed class TextureRegion
        {
            public readonly float X;
            public readonly float Y;

            public readonly float Width;
            public readonly float Height;

            internal TextureRegion(float x, float y, float width, float height)
            {
                X = x;
                Y = y;
                Width = width;
                Height = height;
            }

            /// <summary> Maps relative uv coordinate to match texture atlas. </summary>
            public Vector2 Map(Vector2 relative)
            {
                return new Vector2(X + Width * relative.x, Y + Height * relative.y);
            }
        }
    }
}
