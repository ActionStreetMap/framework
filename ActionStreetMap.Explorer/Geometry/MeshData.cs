using System.Collections.Generic;
using ActionStreetMap.Core.Unity;
using UnityEngine;

namespace ActionStreetMap.Explorer.Geometry
{
    /// <summary> Stored data associated with mesh using arrays. </summary>
    public class MeshData
    {
        /// <summary> Vertices. </summary>
        public List<Vector3> Vertices;

        /// <summary> Triangles. </summary>
        public List<int> Triangles;

        /// <summary> UV map. </summary>
        public List<Vector2> UV;
        
        /// <summary> Material key. </summary>
        public string MaterialKey;

        /// <summary> Built game object. </summary>
        public IGameObject GameObject;
    }
}