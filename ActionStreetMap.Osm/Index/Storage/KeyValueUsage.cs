
using System;
using System.Collections.Generic;
using System.IO;

namespace ActionStreetMap.Osm.Index.Storage
{
    /// <summary>
    ///     Represents inverted index to search for elements which uses the corresponding key/value pair.
    /// </summary>
    internal class KeyValueUsage: IDisposable
    {
        private readonly Stream _stream;

        private uint _nextOffset;

        public KeyValueUsage(Stream stream)
        {
            _stream = stream;
        }

        /// <summary>
        ///     Inserts new entry into stream.
        /// </summary>
        /// <param name="usageOffset">Usage offset.</param>
        /// <returns>Entry offset.</returns>
        public uint Insert(uint usageOffset)
        {
            _stream.Seek(_nextOffset, SeekOrigin.Begin);
            var position = _stream.Position;
            WriteUint(usageOffset);
            WriteUint(0);
            _nextOffset += 8;
            return (uint) position;
        }

        /// <summary>
        ///     Inserts new and update last reference to point to it
        /// </summary>
        /// <param name="firstEntryOffset"></param>
        /// <param name="usageOffset"></param>
        public uint Insert(uint firstEntryOffset, uint usageOffset)
        {
            uint offset = 1;
            while (offset != 0)
            {
                _stream.Seek(firstEntryOffset, SeekOrigin.Begin);
                ReadUint();
                offset = ReadUint();
            }

            // go back to link position
            _stream.Seek(-4, SeekOrigin.Current);
            WriteUint(_nextOffset);

            return Insert(usageOffset);
        }

        public IEnumerable<uint> Get(uint offset)
        {
            uint next = offset;
            do
            {
                _stream.Seek(next, SeekOrigin.Begin);
                yield return ReadUint();
                next = ReadUint();
            } while (next != 0);
        }

        #region Private methods

        private void WriteUint(uint value)
        {
            _stream.WriteByte((byte)(0x000000FF & value));
            _stream.WriteByte((byte)(0x000000FF & value >> 8));
            _stream.WriteByte((byte)(0x000000FF & value >> 16));
            _stream.WriteByte((byte)(0x000000FF & value >> 24));
        }

        private uint ReadUint()
        {
            uint value = (byte) _stream.ReadByte();
            value += (uint)((byte)_stream.ReadByte() << 8);
            value += (uint)((byte)_stream.ReadByte() << 16);
            value += (uint)((byte)_stream.ReadByte() << 24);
            return value;
        }

        #endregion

        public void Dispose()
        {
            _stream.Dispose();
        }
    }
}
