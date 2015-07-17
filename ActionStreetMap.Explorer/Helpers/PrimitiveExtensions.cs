using ActionStreetMap.Core.Geometry;
using UnityEngine;

namespace ActionStreetMap.Explorer.Helpers
{
    internal static class PrimitiveExtensions
    {
        public static Vector3 ToVector3(this Vector2d vec2, float elevation = 0)
        {
            return new Vector3(vec2.FloatX, elevation, vec2.FloatY);
        }

        public static Vector2 ToVector2(this Vector2d vec2)
        {
            return new Vector2(vec2.FloatX, vec2.FloatY);
        }
    }
}
