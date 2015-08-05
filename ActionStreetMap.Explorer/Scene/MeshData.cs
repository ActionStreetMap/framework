using System;
using ActionStreetMap.Core.Unity;
using UnityEngine;

namespace ActionStreetMap.Explorer.Scene
{
    /// <summary> Represents mesh data. </summary>
    public class MeshData
    {
        public string MaterialKey;
        public IGameObject GameObject;
        public IMeshIndex Index;

        public Vector3[] Vertices;
        public int[] Triangles;
        public Color[] Colors;

        private int _nextIndex;
        private bool _isInitialized;
        private bool _isTwoSided;

        /// <summary> Next vertex index. </summary>
        public int NextIndex { get { return _nextIndex; } }

        public MeshData(IMeshIndex meshIndex)
        {
            Index = meshIndex;
        }

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

            Vertices[_nextIndex] = v0;
            Vertices[_nextIndex + 1] = v1;
            Vertices[_nextIndex + 2] = v2;

            Colors[_nextIndex] = color;
            Colors[_nextIndex + 1] = color;
            Colors[_nextIndex + 2] = color;

            Triangles[_nextIndex] = _nextIndex;
            Triangles[_nextIndex + 1] = _nextIndex + 1;
            Triangles[_nextIndex + 2] = _nextIndex + 2;

            if (_isTwoSided)
            {
                var startIndex = Vertices.Length + _nextIndex;
                Triangles[startIndex] = _nextIndex;
                Triangles[startIndex + 1] = _nextIndex + 2;
                Triangles[startIndex + 2] = _nextIndex + 1;
            }

            _nextIndex += 3;
        }
    }
}
