using ActionStreetMap.Explorer.Scene;
using UnityEngine;

namespace ActionStreetMap.Explorer.Interactions
{
    /// <summary> Provides the way to modify mesh via mesh index. </summary>
    public class MeshIndexBehaviour: MonoBehaviour
    {
        /// <summary> True, if mesh is modified and mesh collider should be recreated. </summary>
        public bool IsMeshModified { get; set; }

        /// <summary> Returns mesh index. </summary>
        public IMeshIndex Index { get; internal set; }

        void Update()
        {
            if (IsMeshModified)
            {
                var mesh = gameObject.GetComponent<MeshFilter>().mesh;
                mesh.RecalculateNormals();
                mesh.RecalculateBounds();

                DestroyImmediate(gameObject.GetComponent<MeshCollider>());
                gameObject.AddComponent<MeshCollider>();
                IsMeshModified = false;
            }
        }
    }
}
