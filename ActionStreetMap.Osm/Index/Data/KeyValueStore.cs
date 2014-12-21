using System;
using System.Collections.Generic;
using System.IO;

namespace ActionStreetMap.Osm.Index.Data
{
    internal sealed class KeyValueStore: IDisposable
    {
        private readonly Stream _stream;
        private readonly byte[] _byteBuffer;
        private readonly char[] _charBuffer;

        private readonly KeyValueIndex _index;
        private readonly int _prefixLength;

        private uint _nextOffset;

        public KeyValueStore(KeyValueIndex index, Stream stream)
        {
            _prefixLength = index.PrefixLength;
            _stream = stream;

            // TODO configure consts
            _index = index;

            // NOTE buffer size limited to byte.MaxValue which affect max string size
            _byteBuffer = new byte[256];
            _charBuffer = new char[256];

            // NOTE skip header
            _nextOffset = 2;
            stream.Seek(_nextOffset, SeekOrigin.Begin);
        }

        /// <summary>
        ///     Inserts pair into store if it's not there
        /// </summary>
        /// <param name="pair">Pair.</param>
        /// <returns>Pair offset.</returns>
        public uint Insert(KeyValuePair<string, string> pair)
        {
            var offset = _index.GetOffset(pair);
            if (offset == 0)
            {
                // TODO hash is calculated twice here
                _index.Add(pair, _nextOffset);
                return InsertNew(pair);
            }
            return InsertNext(pair, offset);
        }

        /// <summary>
        ///     Searches pairs by given query.
        /// </summary>
        /// <param name="query">Query.</param>
        /// <returns>List of matched pairs.</returns>
        public IEnumerable<KeyValuePair<string,string>> Search(KeyValuePair<string, string> query)
        {
            foreach (var result in GetEntries(query))
                yield return new KeyValuePair<string, string>(result.Key, result.Value);
        }

        /// <summary>
        ///     Gets pair by given offset.
        /// </summary>
        /// <param name="offset">Offset.</param>
        /// <returns>Pair.</returns>
        public KeyValuePair<string, string> Get(uint offset)
        {
            var entry = ReadEntry(offset);
            return new KeyValuePair<string, string>(entry.Key, entry.Value);
        }

        #region Private members

        private IEnumerable<Entry> GetEntries(KeyValuePair<string, string> query)
        {
            var offset = _index.GetOffset(query);
            if (offset == 0) yield break;

            do
            {
                var entry = ReadEntry(offset);
                var subStringLength = Math.Min(entry.Value.Length, _prefixLength);
                if (entry.Key == query.Key ||
                    entry.Value.Substring(0, subStringLength) == query.Value.Substring(0, subStringLength))
                    yield return entry;
                offset = entry.Next;

            } while (offset != 0);
        }

        private uint InsertNew(KeyValuePair<string, string> pair)
        {
            // maybe seek zero from end?
            if (_stream.Position != _nextOffset)
                _stream.Seek(_nextOffset, SeekOrigin.Begin);
            var offset = _nextOffset;
            var entry = new Entry
            {
                Key = pair.Key,
                Value = pair.Value,
                Next = 0
            };
            WriteEntry(entry);
            return offset;
        }

        private uint InsertNext(KeyValuePair<string, string> pair, uint offset)
        {
            // seek for last item
            uint lastCollisionEntryOffset = offset;
            while (offset != 0)
            {
                //SkipEntryData(offset);
                var entry = ReadEntry(offset);
                // Do not insert duplicates
                if (entry.Key == pair.Key && entry.Value == pair.Value)
                    return offset;

                lastCollisionEntryOffset = offset;
                offset = entry.Next;
            }

            // write entry
            var lastEntryOffset = InsertNew(pair);
            
            // let previous entry to point to newly created one
            SkipEntryData(lastCollisionEntryOffset);
            WriteUint(lastEntryOffset);
            _nextOffset -= 4; // revert change
            return lastEntryOffset;
        }

        #endregion

        #region Stream write operations

        private void WriteEntry(Entry entry)
        {
            WriteString(entry.Key);
            WriteString(entry.Value);
            WriteUint(entry.Usage);
            WriteUint(entry.Next);
        }

        private void WriteString(string s)
        {
            byte toWrite = (byte) (Math.Min(_byteBuffer.Length / sizeof(char), s.Length) * sizeof(char));
            
            for (int i = 0; i < s.Length; i++)
                _charBuffer[i] = s[i];

            Buffer.BlockCopy(_charBuffer, 0, _byteBuffer, 0, toWrite);

            _stream.WriteByte(toWrite);
            _stream.Write(_byteBuffer, 0, toWrite);
            _nextOffset += (uint) (toWrite + 1);
        }

        private void WriteUint(uint value)
        {
            _byteBuffer[0] = (byte)(0x000000FF & value);
            _byteBuffer[1] = (byte)(0x000000FF & value >> 8);
            _byteBuffer[2] = (byte)(0x000000FF & value >> 16);
            _byteBuffer[3] = (byte)(0x000000FF & value >> 24);

            _stream.Write(_byteBuffer, 0, 4);
            _nextOffset += 4;
        }

        #endregion

        #region Stream read operations

        private void SkipEntryData(uint offset)
        {
            _stream.Seek(offset, SeekOrigin.Begin);

            // skip key
            var count = _stream.ReadByte();
            _stream.Seek(count, SeekOrigin.Current);
            // skip value
            count = _stream.ReadByte();
            _stream.Seek(count, SeekOrigin.Current);

            // skip usage
            ReadUint();
        }

        private Entry ReadEntry(uint offset)
        {
            _stream.Seek(offset, SeekOrigin.Begin);
            return new Entry
            {
                Key = ReadString(),
                Value = ReadString(),
                Usage = ReadUint(),
                Next = ReadUint()
            };
        }

        private string ReadString()
        {
            var count = _stream.ReadByte();
            _stream.Read(_byteBuffer, 0, count);
            Buffer.BlockCopy(_byteBuffer, 0, _charBuffer, 0, count);
            var str = new string(_charBuffer, 0, count/2);
            return str;
        }

        private uint ReadUint()
        {
            _stream.Read(_byteBuffer, 0, 4);
            uint value = _byteBuffer[0];
            value += (uint) (_byteBuffer[1] << 8);
            value += (uint)(_byteBuffer[2] << 16);
            value += (uint)(_byteBuffer[3] << 24);

            return value;
        }

        #endregion

        #region Nested

        private struct Entry
        {
            public string Key;
            public string Value;
            public uint Usage;
            public uint Next;
        }

        #endregion

        public void Dispose()
        {
            _stream.Dispose();
        }
    }
}
