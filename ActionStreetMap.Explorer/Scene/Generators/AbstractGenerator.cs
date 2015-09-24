using System;
using ActionStreetMap.Explorer.Customization;
using ActionStreetMap.Explorer.Utils;
using ActionStreetMap.Unity.Wrappers;
using UnityEngine;

namespace ActionStreetMap.Explorer.Scene.Generators
{
    internal abstract class AbstractGenerator
    {
        private float _vertNoiseFreq = 0.05f;
        private float _colorNoiseFreq = 0.1f;
        private GradientWrapper _gradient;

        private Vector2 _uv0;
        private Vector2 _uv1;
        private Vector2 _uv2;

        public abstract int CalculateVertexCount();
        public abstract void Build(MeshData meshData);

        public AbstractGenerator SetVertexNoiseFreq(float vertNoiseFreq)
        {
            _vertNoiseFreq = vertNoiseFreq;
            return this;
        }

        public AbstractGenerator SetColorNoiseFreq(float colorNoiseFreq)
        {
            _colorNoiseFreq = colorNoiseFreq;
            return this;
        }

        public virtual AbstractGenerator SetGradient(GradientWrapper gradient)
        {
            _gradient = gradient;
            return this;
        }

        public virtual AbstractGenerator SetTexture(TextureGroup.Texture texture)
        {
            _uv0 = texture.Map(new Vector2(0, 0));
            _uv1 = texture.Map(new Vector2(1, 0));
            _uv2 = texture.Map(new Vector2(.5f, .5f));
            return this;
        }

        protected void AddTriangle(MeshData meshData, Vector3 v0, Vector3 v1, Vector3 v2)
        {
            var useVertNoise = Math.Abs(_vertNoiseFreq) > 0.0001;

            var noise = useVertNoise ? (Noise.Perlin3D(v0, _vertNoiseFreq) + 1f) / 2f : 0;
            var p0 = new Vector3(v0.x + noise, v0.y + noise, v0.z + noise);

            noise = useVertNoise ? (Noise.Perlin3D(v1, _vertNoiseFreq) + 1f) / 2f : 0;
            var p1 = new Vector3(v1.x + noise, v1.y + noise, v1.z + noise);

            noise = useVertNoise ? (Noise.Perlin3D(v2, _vertNoiseFreq) + 1f) / 2f : 0;
            var p2 = new Vector3(v2.x + noise, v2.y + noise, v2.z + noise);

            var color = GradientUtils.GetColor(_gradient, v1, _colorNoiseFreq);
            meshData.AddTriangle(p0, p1, p2, color, color, _uv0, _uv1, _uv2);
        }
    }
}