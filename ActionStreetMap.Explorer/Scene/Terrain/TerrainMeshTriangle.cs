using UnityEngine;

namespace ActionStreetMap.Explorer.Scene.Terrain
{
    /// <summary> Represents triagle of terrain mesh. </summary>
    internal class TerrainMeshTriangle
    {
        public Vector3 Vertex0;
        public Vector3 Vertex1;
        public Vector3 Vertex2;

        public Color Color0;
        public Color Color1;
        public Color Color2;

        internal int Region;
    }
}
