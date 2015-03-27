
using System;

namespace ActionStreetMap.Explorer.Utils
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

        public static float NextFloat(this Random rng, float min, float max)
        {
            return (float) (min + (rng.NextDouble() * (max - min)));
        }
    }
}
