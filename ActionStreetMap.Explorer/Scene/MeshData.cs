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
        public readonly Vector2[] UVs;

        private int _nextIndex;
        private int _size;

        /// <summary> Gets or sets next vertex index. </summary>
        public int NextIndex
        {
            get { return _nextIndex; }
            internal set { _nextIndex = value; }
        }

        /// <summary> Creates instance of <see cref="MeshData"/>. </summary>
        public MeshData(IMeshIndex meshIndex, int size)
        {
            Index = meshIndex;

            var fullVertCount = size * 2;

            Vertices = new Vector3[fullVertCount];
            Triangles = new int[fullVertCount];
            Colors = new Color[fullVertCount];
            UVs = new Vector2[fullVertCount];

            _size = size;
        }

        /// <summary> Adds two sided triangle to mesh data. </summary>
        public void AddTriangle(Vector3 v0, Vector3 v1, Vector3 v2, Color frontColor, 
            Color backColor, Vector2 uv0, Vector2 uv1, Vector2 uv2)
        {
            var startIndex = _size + _nextIndex;

            // front side
            Vertices[_nextIndex] = v0;
            Colors[_nextIndex] = frontColor;
            Triangles[_nextIndex] = _nextIndex;
            UVs[_nextIndex] = uv0;

            Vertices[++_nextIndex] = v1;
            Colors[_nextIndex] = frontColor;
            Triangles[_nextIndex] = _nextIndex;
            UVs[_nextIndex] = uv1;

            Vertices[++_nextIndex] = v2;
            Colors[_nextIndex] = frontColor;
            Triangles[_nextIndex] = _nextIndex;
            UVs[_nextIndex] = uv2;

            _nextIndex++;

            // back side
            Vertices[startIndex] = v0;
            Colors[startIndex] = backColor;
            Triangles[startIndex] = startIndex + 2;
            UVs[startIndex] = uv0;

            Vertices[++startIndex] = v1;
            Colors[startIndex] = backColor;
            Triangles[startIndex] = startIndex;
            UVs[startIndex] = uv1;

            Vertices[++startIndex] = v2;
            Colors[startIndex] = backColor;
            Triangles[startIndex] = startIndex - 2;
            UVs[startIndex] = uv2;
        }
    }
}