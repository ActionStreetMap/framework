using UnityEngine;

namespace ActionStreetMap.Explorer.Infrastructure
{
    internal static class CoreExtensions
    {
        public static Color ToUnityColor(this Core.Unity.Color32 color32)
        {
            return new Color32(color32.R, color32.G, color32.B, color32.A);
        }
    }
}
