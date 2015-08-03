using System;
using ActionStreetMap.Core.Unity;
using ActionStreetMap.Explorer.Scene.Indices;
using UnityEngine;

namespace ActionStreetMap.Explorer.Scene
{
    /// <summary> Represents mesh data. </summary>
    public class MeshData
    {
        public string MaterialKey;
        public IGameObject GameObject;
        public IMeshIndex Index = DummyMeshIndex.Default;

        public Vector3[] Vertices;
        public int[] Triangles;
        public Color[] Colors;

        private int _lastIndex;
        private bool _isInitialized;
        private bool _isTwoSided;

        /// <summary> Initializes mesh data using given size. </summary>
        public void Initialize(int size, bool isTwoSided = false)
        {
            if (_isInitialized)
                throw new InvalidOperationException(Strings.MultiplyMeshDataInitialization);

            _isTwoSided = isTwoSided;

            Vertices = new Vector3[size];
            Triangles = new int[isTwoSided ? size * 2 : size];
            Colors = new Color[size];

            _isInitialized = true;
        }

        /// <summary> Adds one sided triangle to mesh data </summary>
        public void AddTriangle(Vector3 v0, Vector3 v1, Vector3 v2, Color color)
        {
            if (!_isInitialized)
                throw new InvalidOperationException(Strings.NotInitializedMeshDataUsage);

            Vertices[_lastIndex] = v0;
            Vertices[_lastIndex + 1] = v1;
            Vertices[_lastIndex + 2] = v2;

            Colors[_lastIndex] = color;
            Colors[_lastIndex + 1] = color;
            Colors[_lastIndex + 2] = color;

            Triangles[_lastIndex] = _lastIndex;
            Triangles[_lastIndex + 1] = _lastIndex + 1;
            Triangles[_lastIndex + 2] = _lastIndex + 2;

            if (_isTwoSided)
            {
                var startIndex = Vertices.Length + _lastIndex;
                Triangles[startIndex] = _lastIndex;
                Triangles[startIndex + 1] = _lastIndex + 2;
                Triangles[startIndex + 2] = _lastIndex + 1;
            }

            _lastIndex += 3;
        }
    }
}
