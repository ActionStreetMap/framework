using UnityEngine;

namespace ActionStreetMap.Explorer.Scene.Utils
{
    internal static class Vector3Utils
    {
        public static Vector3 GetIntermediatePoint(Vector3 v0, Vector3 v1)
        {
            var distance01 = Vector3.Distance(v0, v1);
            return v0 + (v1 - v0).normalized * distance01 / 2;
        }
    }
}
