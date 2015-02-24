using UnityEngine;

namespace ActionStreetMap.Explorer.Terrain.Unity
{
    /// <summary> Defines terrain element. </summary>
    public class TerrainElement
    {
        /// <summary> ZIndex. </summary>
        public float ZIndex;

        /// <summary> Splat index. </summary>
        public int SplatIndex;

        /// <summary> Detail index. </summary>
        public int DetailIndex;

        /// <summary> Gets or sets map points. </summary>
        public Vector3[] Points { get; set; }
    }
}