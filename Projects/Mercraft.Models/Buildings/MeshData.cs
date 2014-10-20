﻿using UnityEngine;

namespace Mercraft.Models.Buildings
{
    /// <summary>
    ///     Stored data associated with mesh 
    /// </summary>
    public class MeshData
    {
        /// <summary>
        ///     Vertices
        /// </summary>
        public Vector3[] Vertices;

        /// <summary>
        ///     Triangles.
        /// </summary>
        public int[] Triangles;

        /// <summary>
        ///     UV map.
        /// </summary>
        public Vector2[] UV;
        
        /// <summary>
        ///     Material key.
        /// </summary>
        public string MaterialKey;
    }
}