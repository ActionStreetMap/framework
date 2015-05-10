using System;
using System.Collections.Generic;
using ActionStreetMap.Core;
using ActionStreetMap.Explorer.Utils;
using ActionStreetMap.Unity.Wrappers;
using UnityEngine;

namespace ActionStreetMap.Explorer.Geometry.Generators
{
    internal abstract class AbstractGenerator
    {
        private readonly MeshData _meshData;
        private float _vertNoiseFreq = 0.05f;
        private float _colorNoiseFreq = 0.1f;
        private GradientWrapper _gradient;


        public abstract void Build();

        protected AbstractGenerator(MeshData meshData)
        {
            _meshData = meshData;
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
            var useVertNoise = Math.Abs(_vertNoiseFreq) > 0.0001;

            var noise = useVertNoise ? (Noise.Perlin3D(v0, _vertNoiseFreq) + 1f) / 2f : 0;
            var p0 = new MapPoint(v0.x + noise, v0.z + noise, v0.y + noise);

            noise = useVertNoise ? (Noise.Perlin3D(v1, _vertNoiseFreq) + 1f) / 2f : 0;
            var p1 = new MapPoint(v1.x + noise, v1.z + noise, v1.y + noise);

            noise = useVertNoise ? (Noise.Perlin3D(v2, _vertNoiseFreq) + 1f) / 2f : 0;
            var p2 = new MapPoint(v2.x + noise, v2.z + noise, v2.y + noise);

            _meshData.AddTriangle(p0, p1, p2, GradientUtils.GetColor(_gradient, v1, _colorNoiseFreq));
        }
    }
}
