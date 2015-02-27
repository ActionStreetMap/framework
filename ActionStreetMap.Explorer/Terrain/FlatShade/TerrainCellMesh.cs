using UnityEngine;

namespace ActionStreetMap.Explorer.Terrain.FlatShade
{
    /// <summary> Represents terrain mesh. </summary>
    public class TerrainCellMesh
    {
        /// <summary> Gets or sets name of mesh. </summary>
        public string Name { get; set; }

        /// <summary> Gets or sets vertices. </summary>
        public Vector3[] Vertices { get; set; }

        /// <summary> Gets or sets triangles. </summary>
        public int[] Triangles { get; set; }

        /// <summary> Gets or sets Color. </summary>
        public Color[] Colors { get; set; }
    }
}
