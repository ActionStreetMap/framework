using ActionStreetMap.Core;
using UnityEngine;

namespace ActionStreetMap.Explorer.Scene.Terrain
{
    /// <summary> Represents triagle of terrain mesh. </summary>
    internal class TerrainMeshTriangle
    {
        public MapPoint Vertex0;
        public MapPoint Vertex1;
        public MapPoint Vertex2;

        public Color Color0;
        public Color Color1;
        public Color Color2;

        internal int Region;
    }
}
