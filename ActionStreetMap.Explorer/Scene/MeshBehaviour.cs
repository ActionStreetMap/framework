using System.Collections.Generic;
using ActionStreetMap.Core;
using ActionStreetMap.Explorer.Geometry;
using UnityEngine;

namespace ActionStreetMap.Explorer.Scene
{
    public class MeshBehaviour: MonoBehaviour
    {
        internal MeshIndex Index { get; set; }
       
        public List<int> GetAffectedIndices(MapPoint center, float radius)
        {
            return Index.GetAfectedIndecies(center, radius);
        }
    }
}
