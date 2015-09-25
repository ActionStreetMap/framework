using System;
using System.Collections.Generic;
using UnityEngine;

namespace ActionStreetMap.Explorer.Customization
{
    /// <summary> Represents texture group. </summary>
    /// <remarks> Not thread safe. </remarks>
    public sealed class TextureGroup
    {
        private static readonly NullTexture EmptyTexture = new NullTexture();
        private readonly List<Texture> _textures;

        private readonly float _xRatio;
        private readonly float _yRatio;
        private readonly bool _isEmpty;

        /// <summary> Creates instance of <see cref="TextureGroup" />. </summary>
        /// <param name="width"> Texture width. </param>
        /// <param name="height"> Texture height. </param>
        /// <param name="capacity"> Internal dictionary capacity. </param>
        public TextureGroup(int width, int height, int capacity = 1)
        {
            _xRatio = 1/(float) width;
            _yRatio = 1/(float) height;
            _textures = new List<Texture>(capacity);
        }

        /// <summary>
        ///     Creates instance of <see cref="TextureGroup" />. which will
        ///     return null texture
        /// </summary>
        internal TextureGroup() : this(1, 1, 1)
        {
            _isEmpty = true;
        }

        /// <summary> Adds texture with given parameters to group. </summary>
        public TextureGroup Add(int x, int y, int width, int height)
        {
            if (_isEmpty)
                throw new InvalidOperationException(Strings.CannotAddTexture);

            _textures.Add(new Texture(x*_xRatio, y*_yRatio,
                width*_xRatio, height*_yRatio));
            return this;
        }

        /// <summary> Gets texture using seed provided.  </summary>
        public Texture Get(int seed)
        {
            return !_isEmpty ? _textures[seed%_textures.Count] : EmptyTexture;
        }

        /// <summary> Represents texture in atlas. </summary>
        public class Texture
        {
            private readonly float _height;

            private readonly float _width;
            private readonly float _x;
            private readonly float _y;

            internal Texture(float x, float y, float width, float height)
            {
                _x = x;
                _y = y;
                _width = width;
                _height = height;
            }

            /// <summary> Maps relative uv coordinate to absolute in atlas. </summary>
            public virtual Vector2 Map(Vector2 relative)
            {
                return new Vector2(_x + _width*relative.x, _y + _height*relative.y);
            }
        }

        /// <summary> Represents empty texture in atlas. </summary>
        internal class NullTexture : Texture
        {
            internal NullTexture()
                : base(0, 0, 0, 0)
            {
            }

            public override Vector2 Map(Vector2 relative)
            {
                return new Vector2(0, 0);
            }
        }
    }
}