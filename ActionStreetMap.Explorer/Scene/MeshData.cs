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

        public readonly Vector3[] Vertices;
        public readonly int[] Triangles;
        public readonly Color[] Colors;

        private int _nextIndex;
        private int _size;

        /// <summary> Gets or sets next vertex index. </summary>
        public int NextIndex
        {
            get { return _nextIndex; }
            internal set { _nextIndex = value; }
        }

        /// <summary> Creates instance of <see cref="MeshData"/>. </summary>
        public MeshData(IMeshIndex meshIndex, Vector3[] vertices, int[] triangles,
            Color[] colors)
        {
            Index = meshIndex;

            Vertices = vertices;
            Triangles = triangles;
            Colors = colors;

            _size = vertices.Length / 2;
        }

        /// <summary> Creates instance of <see cref="MeshData"/>. </summary>
        public MeshData(IMeshIndex meshIndex, int size)
        {
            Index = meshIndex;

            var fullVertCount = size * 2;

            Vertices = new Vector3[fullVertCount];
            Triangles = new int[fullVertCount];
            Colors = new Color[fullVertCount];

            _size = size;
        }

        /// <summary> Adds two sided triangle to mesh data. </summary>
        public void AddTriangle(Vector3 v0, Vector3 v1, Vector3 v2,
            Color frontColor, Color backColor)
        {
            var startIndex = _size + _nextIndex;

            // front side
            Vertices[_nextIndex] = v0;
            Colors[_nextIndex] = frontColor;
            Triangles[_nextIndex] = _nextIndex;

            Vertices[++_nextIndex] = v1;
            Colors[_nextIndex] = frontColor;
            Triangles[_nextIndex] = _nextIndex;

            Vertices[++_nextIndex] = v2;
            Colors[_nextIndex] = frontColor;
            Triangles[_nextIndex] = _nextIndex;

            _nextIndex++;

            // back side
            Vertices[startIndex] = v0;
            Colors[startIndex] = backColor;
            Triangles[startIndex] = startIndex + 2;

            Vertices[++startIndex] = v1;
            Colors[startIndex] = backColor;
            Triangles[startIndex] = startIndex;

            Vertices[++startIndex] = v2;
            Colors[startIndex] = backColor;
            Triangles[startIndex] = startIndex - 2;
        }
    }
}