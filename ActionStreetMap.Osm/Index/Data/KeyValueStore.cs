using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace ActionStreetMap.Osm.Index.Data
{
    internal class KeyValueStore: IDisposable
    {
        private readonly Stream _stream;
        private readonly KeyValueIndex _index;

        private uint _nextOffset;
        private byte[] _buffer;

        public KeyValueStore(Stream stream)
        {
            _stream = stream;

            // TODO configure consts
            _index = new KeyValueIndex(60000, 5);

            // NOTE buffer size limited to byte.MaxValue which affect max string size
            _buffer = new byte[256];

            // NOTE skip header
            _nextOffset = 2;
        }

        public void Add(KeyValuePair<string, string> pair)
        {
            var offset = _index.GetOffset(pair);
            if (offset == 0)
            {
                _index.Add(pair, _nextOffset);
                InsertNew(pair);
            }
            else
                InsertNext(pair, offset);
        }

        public uint GetOffset(KeyValuePair<string, string> pair)
        {
            var offset = _index.GetOffset(pair);
            var entry = new Entry();
            do
            {
                offset = entry.Next == 0 ? offset : entry.Next;
                entry = ReadEntry(offset);
                
            } while (entry.Key != pair.Key || entry.Value != pair.Value);

            return offset;
        }

        #region Private members

        private void InsertNew(KeyValuePair<string, string> pair)
        {
            // maybe seek zero from end?
            if (_stream.Position != _nextOffset)
                _stream.Seek(_nextOffset, SeekOrigin.Begin);

            var entry = new Entry()
            {
                Key = pair.Key,
                Value = pair.Value,
                Next = 0
            };
            WriteEntry(entry);
        }

        private void InsertNext(KeyValuePair<string, string> pair, uint offset)
        {
            // seek for last item
            uint lastCollisionEntryOffset = offset;
            while (offset != 0)
            {
                SkipEntryData(offset);
                lastCollisionEntryOffset = offset;
                offset = ReadUint();
            }

            // write entry
            var lastEntryOffset = _nextOffset;
            InsertNew(pair);

            // let previous entry to point to newly created one
            SkipEntryData(lastCollisionEntryOffset);
            WriteUint(lastEntryOffset);
            _nextOffset -= 4; // revert change
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
            byte toWrite = (byte) Math.Min(_buffer.Length / 2, s.Length);
            Encoding.UTF8.GetBytes(s, 0, toWrite, _buffer, 0);
            _stream.WriteByte(toWrite);
            _stream.Write(_buffer, 0, toWrite);
            _nextOffset += (uint) (toWrite + 1);
        }

        private void WriteUint(uint value)
        {
            _buffer[0] = (byte) (0x000000FF & value);
            _buffer[1] = (byte)(0x000000FF & value >> 8);
            _buffer[2] = (byte)(0x000000FF & value >> 16);
            _buffer[3] = (byte)(0x000000FF & value >> 24);

            _stream.Write(_buffer, 0, 4);
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
            return new Entry()
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
            _stream.Read(_buffer, 0, count);
            var str = Encoding.UTF8.GetString(_buffer, 0, count);
            return str;
        }

        private uint ReadUint()
        {
            _stream.Read(_buffer, 0, 4);
            uint value = _buffer[0];
            value += (uint) (_buffer[1] << 8);
            value += (uint)(_buffer[2] << 16);
            value += (uint)(_buffer[3] << 24);

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
