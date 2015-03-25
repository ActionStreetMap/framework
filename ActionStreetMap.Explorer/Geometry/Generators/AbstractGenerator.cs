using System.Collections.Generic;
using ActionStreetMap.Explorer.Utils;
using ActionStreetMap.Unity.Wrappers;
using UnityEngine;

namespace ActionStreetMap.Explorer.Geometry.Generators
{
    internal abstract class AbstractGenerator
    {
        private float _vertNoiseFreq = 0.1f;
        private float _colorNoiseFreq = 0.1f;
        private GradientWrapper _gradient;

        private readonly List<Vector3> _vertices;
        private readonly List<int> _triangles;
        private readonly List<Color> _colors;

        public abstract void Build();

        protected AbstractGenerator(MeshData meshData)
        {
            _vertices = meshData.Vertices;
            _triangles = meshData.Triangles;
            _colors = meshData.Colors;
        }

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

        public AbstractGenerator SetGradient(GradientWrapper gradient)
        {
            _gradient = gradient;
            return this;
        }

        protected void AddTriangle(Vector3 v0, Vector3 v1, Vector3 v2)
        {
            var noise = _vertNoiseFreq != 0 ? (Noise.Perlin3D(v0, _vertNoiseFreq) + 1f) / 2f : 0;
            _vertices.Add(new Vector3(v0.x + noise, v0.y + noise, v0.z + noise));

            noise = _vertNoiseFreq != 0 ? (Noise.Perlin3D(v1, _vertNoiseFreq) + 1f) / 2f : 0;
            _vertices.Add(new Vector3(v1.x + noise, v1.y + noise, v1.z + noise));

            noise = _vertNoiseFreq != 0 ? (Noise.Perlin3D(v2, _vertNoiseFreq) + 1f) / 2f : 0;
            _vertices.Add(new Vector3(v2.x + noise, v2.y + noise, v2.z + noise));

            var tris = _vertices.Count - 3;

            _triangles.Add(tris);
            _triangles.Add(tris + 2);
            _triangles.Add(tris + 1);

            var color = GradientUtils.GetColor(_gradient, v1, _colorNoiseFreq);

            _colors.Add(color);
            _colors.Add(color);
            _colors.Add(color);
        }
    }
}
