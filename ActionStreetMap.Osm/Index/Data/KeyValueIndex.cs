using System;
using System.Collections.Generic;

namespace ActionStreetMap.Osm.Index.Data
{
    internal class KeyValueIndex
    {
        private readonly int _capacity;
        private readonly int _prefixLength;
        private readonly uint[] _buckets;

        #region Constructors

        public KeyValueIndex(int capacity, int prefixLength)
        {
            _capacity = capacity;
            _prefixLength = prefixLength;
            _buckets = new uint[capacity];
        }

        #endregion

        #region Public members

        public void Add(KeyValuePair<string, string> pair, uint offset)
        {
            var index = GetIndex(pair);
            _buckets[index] = offset;
        }

        public uint GetOffset(KeyValuePair<string, string> pair)
        {
            var index = GetIndex(pair);
            return _buckets[index];
        }

        #endregion

        #region Private

        private int GetIndex(KeyValuePair<string, string> pair)
        {
            int hash = HashString(pair.Key, 0, pair.Key.Length);
            hash = HashString(pair.Value, hash, Math.Min(pair.Value.Length, _prefixLength));

            hash += (hash << 3);
            hash ^= (hash >> 11);
            hash += (hash << 15);
            return Math.Abs(hash) % _capacity;
        }

        #endregion

        #region Static members

        private static int HashString(string s, int hash, int length)
        {
            for (int i = 0; i < length; i++)
            {
                hash += s[i];
                hash += (hash << 10);
                hash ^= (hash >> 6);
            }
            return hash;
        }

        #endregion
    }
}
