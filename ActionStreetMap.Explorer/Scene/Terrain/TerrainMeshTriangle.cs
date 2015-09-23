using ActionStreetMap.Explorer.Customization;
using UnityEngine;

namespace ActionStreetMap.Explorer.Scene.Terrain
{
    /// <summary> Represents triagle of terrain mesh. </summary>
    internal class TerrainMeshTriangle
    {
        internal const int InvalidRegionIndex = -1;

        public Vector3 Vertex0;
        public Vector3 Vertex1;
        public Vector3 Vertex2;

        public Color Color;
        public TextureGroup.Texture Texture;

        internal int Region = InvalidRegionIndex;
    }
}
