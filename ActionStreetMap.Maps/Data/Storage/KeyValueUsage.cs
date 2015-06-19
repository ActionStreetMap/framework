using System;
using System.Collections.Generic;
using System.IO;

namespace ActionStreetMap.Maps.Data.Storage
{
    /// <summary> Represents inverted index to search for elements which uses the corresponding key/value pair. </summary>
    internal class KeyValueUsage: IDisposable
    {
        private readonly Stream _stream;

        private uint _nextOffset;

        /// <summary> Creates instance of <see cref="KeyValueUsage"/>. </summary>
        /// <param name="stream">Stream.</param>
        public KeyValueUsage(Stream stream)
        {
            _stream = stream;
            _nextOffset = 2;
        }

        /// <summary> Inserts new and update last reference to point to it. </summary>
        public uint Insert(uint previousEntryOffset, uint usageOffset)
        {
            _stream.Seek(_nextOffset, SeekOrigin.Begin);
            var position = _stream.Position;
            WriteUint(usageOffset);
            WriteUint(previousEntryOffset);
            _nextOffset += 8;
            return (uint)position;
        }

        /// <summary> Gets element offsets which use given entry. </summary>
        /// <param name="offset">Entry offset.</param>
        /// <returns>Element offset collection.</returns>
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

        /// <inheritdoc />
        public void Dispose()
        {
            _stream.Dispose();
        }
    }
}
