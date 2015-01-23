
using System;

namespace ActionStreetMap.Explorer.Scene.Utils
{
    internal static class RandomHelper
    {
        public static int GetIndex(long seed, int count)
        {
            return (int)( seed % count);
        }

        public static double NextDouble(this Random rng, double min, double max)
        {
            return min + (rng.NextDouble() * (max - min));
        }
    }
}
