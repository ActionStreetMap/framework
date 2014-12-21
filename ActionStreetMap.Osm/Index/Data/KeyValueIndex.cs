using System;
using System.Collections.Generic;
using System.IO;

namespace ActionStreetMap.Osm.Index.Data
{
    internal sealed class KeyValueIndex
    {
        private readonly int _capacity;

        private readonly uint[] _buckets;

        #region Constructors

        public KeyValueIndex(int capacity, int prefixLength)
        {
            _capacity = capacity;
            _buckets = new uint[capacity];
            PrefixLength = prefixLength;
        }

        #endregion

        public int PrefixLength { get; private set; }

        #region Public members

        public void Add(KeyValuePair<string, string> pair, uint offset)
        {
            var index = GetIndex(pair);
            _buckets[index] = offset;
        }

        /// <summary>
        ///     Gets offset for given pair.
        /// </summary>
        /// <param name="pair">Pair.</param>
        /// <returns>Offset.</returns>
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
            hash = HashString(pair.Value, hash, Math.Min(pair.Value.Length, PrefixLength));

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

        #region Static members

        public static void Save(KeyValueIndex index, Stream stream)
        {
            using (var writer = new BinaryWriter(stream))
            {
                writer.Write(index._capacity);
                writer.Write(index.PrefixLength);

                var buckets = index._buckets;
                for (int i = 0; i < buckets.Length; i++)
                    writer.Write(buckets[i]);
            }
        }

        public static KeyValueIndex Load(Stream stream)
        {
            using (var reader = new BinaryReader(stream))
            {
                var capacity = reader.ReadInt32();
                var prefixLength = reader.ReadInt32();
                var index = new KeyValueIndex(capacity, prefixLength);
                
                var buckets = index._buckets;
                for (int i = 0; i < buckets.Length; i++)
                    buckets[i] = reader.ReadUInt32();

                return index;
            }
        }

        #endregion
    }
}
