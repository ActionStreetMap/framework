using System;

namespace ActionStreetMap.Core.Utilities
{
    internal class RandomUtils
    {
        public static long LongRandom(long min, long max, Random rand)
        {
            if (min >= max)
                throw new ArgumentException();

            byte[] buf = new byte[8];
            rand.NextBytes(buf);
            long longRand = BitConverter.ToInt64(buf, 0);

            return (Math.Abs(longRand%(max - min)) + min);
        }
    }
}
