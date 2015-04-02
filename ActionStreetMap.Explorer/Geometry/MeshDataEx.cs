using System.Collections.Generic;
using ActionStreetMap.Core.Unity;

namespace ActionStreetMap.Explorer.Geometry
{
    public class MeshDataEx
    {
        /// <summary> Triangles. </summary>
        public List<MeshTriangle> Triangles;

        /// <summary> Material key. </summary>
        public string MaterialKey;

        /// <summary> Built game object. </summary>
        public IGameObject GameObject;
    }
}
