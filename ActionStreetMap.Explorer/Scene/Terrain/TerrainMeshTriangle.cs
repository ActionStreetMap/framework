using ActionStreetMap.Core.Geometry;
using UnityEngine;

namespace ActionStreetMap.Explorer.Scene.Terrain
{
    /// <summary> Represents triagle of terrain mesh. </summary>
    internal class TerrainMeshTriangle
    {
        public Vector2d Vertex0;
        public Vector2d Vertex1;
        public Vector2d Vertex2;

        public Color Color0;
        public Color Color1;
        public Color Color2;

        internal int Region;
    }
}
