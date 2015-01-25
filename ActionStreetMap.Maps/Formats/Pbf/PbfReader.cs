using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ActionStreetMap.Core;
using ActionStreetMap.Maps.Entities;
using ActionStreetMap.Maps.Index.Spatial;
using Ionic.Zlib;
using ProtoBuf;
using ProtoBuf.Meta;

namespace ActionStreetMap.Maps.Formats.Pbf
{
    /// <summary>
    ///     Reads PBF files.
    /// </summary>
    internal class PbfReader : IReader, IEnumerator<PrimitiveBlock>
    {
        private readonly ReaderContext _context;

        private readonly RuntimeTypeModel _runtimeTypeModel;

        // Types of the objects to be deserialized.
        private readonly Type _blockHeaderType = typeof(BlockHeader);
        private readonly Type _blobType = typeof(Blob);
        private readonly Type _primitiveBlockType = typeof(PrimitiveBlock);
        private readonly Type _headerBlockType = typeof(HeaderBlock);
        private PrimitiveBlock _block;
        private Envelop _envelop;

        /// <summary>
        ///     Creates a new PBF reader.
        /// </summary>
        public PbfReader(ReaderContext context)
        {
            _context = context;
            _runtimeTypeModel = TypeModel.Create();
            _runtimeTypeModel.Add(_blockHeaderType, true);
            _runtimeTypeModel.Add(_blobType, true);
            _runtimeTypeModel.Add(_primitiveBlockType, true);
            _runtimeTypeModel.Add(_headerBlockType, true);
        }

        /// <summary>
        ///     Reads pbf file using <see cref="ReaderContext"/>.
        /// </summary>
        public void Read()
        {
            _envelop = new Envelop();
            while (MoveNext())
            {
                var block = Current;
                ProcessPrimitiveBlock(block);
                foreach (var primitiveGroup in block.primitivegroup)
                {
                    if (!primitiveGroup.IsNodeListEmpty)
                    {
                        foreach (var node in primitiveGroup.nodes)
                            ProcessNode(block, node);
                    }
                    if (!primitiveGroup.IsWayListEmpty)
                    {
                        foreach (var way in primitiveGroup.ways)
                            ProcessWay(block, way);
                    }

                    if (!primitiveGroup.IsRelationListEmpty)
                    {
                        foreach (var relation in primitiveGroup.relations)
                            ProcessRelation(block, relation);
                    }
                }
            }
            _context.Builder.ProcessBoundingBox(new BoundingBox(_envelop.MinPoint, _envelop.MaxPoint));
        }

        #region Process elements

        private void ProcessNode(PrimitiveBlock block, Formats.Pbf.Node node)
        {
            var latitude = .000000001 * (block.lat_offset + (block.granularity * (double)node.lat));
            var longitude = .000000001 * (block.lon_offset + (block.granularity * (double)node.lon));

            var elementNode = new Entities.Node();
            elementNode.Id = node.id;
            elementNode.Coordinate = new GeoCoordinate(latitude, longitude);

            if (node.keys.Any())
            {
                elementNode.Tags = new Dictionary<string, string>(node.keys.Count);
                for (int tagIdx = 0; tagIdx < node.keys.Count; tagIdx++)
                {
                    var keyBytes = block.stringtable.s[(int)node.keys[tagIdx]];
                    string key = Encoding.UTF8.GetString(keyBytes, 0, keyBytes.Length);
                    var valueBytes = block.stringtable.s[(int)node.vals[tagIdx]];
                    string value = Encoding.UTF8.GetString(valueBytes, 0, valueBytes.Length);
                    //if (elementNode.Tags.ContainsKey(key))
                    //    continue;
                    elementNode.Tags.Add(key, value);
                }
            }
            _envelop.Extend(elementNode.Coordinate);
            _context.Builder.ProcessNode(elementNode, node.keys.Count);
        }

        private void ProcessWay(PrimitiveBlock block, Formats.Pbf.Way way)
        {
            long nodeId = 0;
            var nodeIds = new List<long>();
            var refCount = way.refs.Count;
            for (int nodeIdx = 0; nodeIdx < refCount; nodeIdx++)
            {
                nodeId = nodeId + way.refs[nodeIdx];
                nodeIds.Add(nodeId);
            }

            var elementWay = new Entities.Way { Id = way.id, NodeIds = nodeIds };
            if (way.keys.Any())
            {
                var keyCount = way.keys.Count;
                elementWay.Tags = new Dictionary<string, string>(keyCount);
                for (int tagIdx = 0; tagIdx < keyCount; tagIdx++)
                {
                    var keyBytes = block.stringtable.s[(int)way.keys[tagIdx]];
                    string key = String.Intern(Encoding.UTF8.GetString(keyBytes, 0, keyBytes.Length));
                    var valueBytes = block.stringtable.s[(int)way.vals[tagIdx]];
                    string value = String.Intern(Encoding.UTF8.GetString(valueBytes, 0, valueBytes.Length));
                    elementWay.Tags.Add(key, value);
                }
            }
            _context.Builder.ProcessWay(elementWay, way.keys.Count);
        }

        private void ProcessRelation(PrimitiveBlock block, Formats.Pbf.Relation relation)
        {
            var elementRelation = new Entities.Relation();
            elementRelation.Id = relation.id;
            if (relation.types.Count > 0)
            {
                elementRelation.Members = new List<RelationMember>();
                long memberId = 0;
                for (int memberIdx = 0; memberIdx < relation.types.Count; memberIdx++)
                {
                    memberId = memberId + relation.memids[memberIdx];
                    var roleBytes = block.stringtable.s[relation.roles_sid[memberIdx]];
                    string role = String.Intern(Encoding.UTF8.GetString(roleBytes, 0, roleBytes.Length));
                    var member = new RelationMember();
                    member.MemberId = memberId;
                    member.Role = role;
                    member.TypeId = (int) relation.types[memberIdx];
                    elementRelation.Members.Add(member);
                }
            }
            if (relation.keys.Count > 0)
            {
                elementRelation.Tags = new Dictionary<string, string>(relation.keys.Count);
                for (int tagIdx = 0; tagIdx < relation.keys.Count; tagIdx++)
                {
                    var keyBytes = block.stringtable.s[(int)relation.keys[tagIdx]];
                    string key = String.Intern(Encoding.UTF8.GetString(keyBytes, 0, keyBytes.Length));
                    var valueBytes = block.stringtable.s[(int)relation.vals[tagIdx]];
                    string value = String.Intern(Encoding.UTF8.GetString(valueBytes, 0, valueBytes.Length));
                    elementRelation.Tags.Add(key, value);
                }
            }
            _context.Builder.ProcessRelation(elementRelation, relation.keys.Count);
        }

        #endregion

        #region IEnumerable implementation

        /// <summary>
        ///     Processes next element in sequence. Supports caching of deserialized data.
        /// </summary>
        public bool MoveNext()
        {
            return (_block = ProcessBlock()) != null;
        }

        public void Reset()
        {
            if (_context.SourceStream.CanSeek)
                _context.SourceStream.Seek(0, SeekOrigin.Begin);
        }

        public PrimitiveBlock Current { get { return _block; } }
        object IEnumerator.Current { get { return Current; } }

        #endregion

        /// <summary>
        ///     Read next block. Use this API if you don't want to cache content.
        /// </summary>
        private PrimitiveBlock ProcessBlock()
        {
            bool notFoundBut = true;
            _block = null;
            while (notFoundBut)
            {
                // continue if there is still data but not a primitiveblock.
                notFoundBut = false; // not found.
                int length;
                if (Serializer.TryReadLengthPrefix(_context.SourceStream, PrefixStyle.Fixed32, out length))
                {
                    // TODO: remove some of the v1 specific code.
                    // TODO: this means also to use the built-in capped streams.

                    // code borrowed from: http://stackoverflow.com/questions/4663298/protobuf-net-deserialize-open-street-maps

                    // I'm just being lazy and re-using something "close enough" here
                    // note that v2 has a big-endian option, but Fixed32 assumes little-endian - we
                    // actually need the other way around (network byte order):
                    length = IntLittleEndianToBigEndian((uint) length);

                    BlockHeader header;
                    // again, v2 has capped-streams built in, but I'm deliberately
                    // limiting myself to v1 features
                    using (var tmp = new LimitedStream(_context.SourceStream, length))
                    {
                        header = _runtimeTypeModel.Deserialize(tmp, null, _blockHeaderType) as BlockHeader;
                    }
                    Blob blob;
                    using (var tmp = new LimitedStream(_context.SourceStream, header.datasize))
                    {
                        blob = _runtimeTypeModel.Deserialize(tmp, null, _blobType) as Blob;
                    }

                    // construct the source stream, compressed or not.
                    Stream sourceStream;
                    if (blob.zlib_data == null)
                    {
                        // use a regular uncompressed stream.
                        sourceStream = new MemoryStream(blob.raw);
                    }
                    else
                    {
                        // construct a compressed stream.
                        var ms = new MemoryStream(blob.zlib_data);
                        sourceStream = new ZLibStreamWrapper(ms);
                    }

                    // use the stream to read the block.
                    using (sourceStream)
                    {
                        if (header.type == "OSMHeader")
                        {
                            _runtimeTypeModel.Deserialize(sourceStream, null, _headerBlockType);
                            notFoundBut = true;
                        }

                        if (header.type == "OSMData")
                        {
                            _block = _runtimeTypeModel.Deserialize(sourceStream, _block, _primitiveBlockType) as PrimitiveBlock;
                        }
                    }
                }
            }
            return _block;
        }

        /// <summary>
        ///     Processes primitive block
        /// </summary>
        private void ProcessPrimitiveBlock(PrimitiveBlock block)
        {
            if (block.primitivegroup != null)
            {
                foreach (PrimitiveGroup primitivegroup in block.primitivegroup)
                {
                    if (primitivegroup.dense != null)
                    {
                        int keyValsIdx = 0;
                        long currentId = 0;
                        long currentLat = 0;
                        long currentLon = 0;

                        var count = primitivegroup.dense.id.Count;
                        var nodes = new List<Formats.Pbf.Node>();
                        for (int idx = 0; idx < count; idx++)
                        {
                            // do the delta decoding stuff.
                            currentId = currentId + primitivegroup.dense.id[idx];
                            currentLat = currentLat + primitivegroup.dense.lat[idx];
                            currentLon = currentLon + primitivegroup.dense.lon[idx];

                            var node = new Formats.Pbf.Node {id = currentId, lat = currentLat, lon = currentLon};

                            // get the keys/vals.
                            List<int> keysVals = primitivegroup.dense.keys_vals;
                            var keysValsCount = keysVals.Count;
                            node.Initialize();
                            while (keysValsCount > keyValsIdx && keysVals[keyValsIdx] != 0)
                            {
                                node.keys.Add((uint)keysVals[keyValsIdx]);
                                keyValsIdx++;
                                node.vals.Add((uint)keysVals[keyValsIdx]);
                                keyValsIdx++;
                            }
                            keyValsIdx++;
                            nodes.Add(node);
                        }
                        primitivegroup.nodes = nodes;
                    }
                }
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _context.SourceStream.Dispose();
            _block = null;
        }

        // 4-byte number
        private static int IntLittleEndianToBigEndian(uint i)
        {
            return (int) (((i & 0xff) << 24) + ((i & 0xff00) << 8) + ((i & 0xff0000) >> 8) + ((i >> 24) & 0xff));
        }
    }

    #region Stream classes

    internal abstract class InputStream : Stream
    {
        private long _pos;

        protected abstract int ReadNextBlock(byte[] buffer, int offset, int count);

        public override sealed int Read(byte[] buffer, int offset, int count)
        {
            int bytesRead, totalRead = 0;
            while (count > 0 && (bytesRead = ReadNextBlock(buffer, offset, count)) > 0)
            {
                count -= bytesRead;
                offset += bytesRead;
                totalRead += bytesRead;
                _pos += bytesRead;
            }
            return totalRead;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override long Position
        {
            get { return _pos; }
            set { if (_pos != value) throw new NotImplementedException(); }
        }

        public override long Length
        {
            get { throw new NotImplementedException(); }
        }

        public override void Flush()
        {
            throw new NotImplementedException();
        }

        public override bool CanWrite
        {
            get { return false; }
        }

        public override bool CanRead
        {
            get { return true; }
        }

        public override bool CanSeek
        {
            get { return false; }
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }
    }

    internal class ZLibStreamWrapper : InputStream
    {
        private readonly ZlibStream _reader;

        public ZLibStreamWrapper(Stream stream)
        {
            _reader = new ZlibStream(stream, CompressionMode.Decompress);
        }

        protected override int ReadNextBlock(byte[] buffer, int offset, int count)
        {
            return _reader.Read(buffer, offset, count);
        }
    }

    // deliberately doesn't dispose the base-stream    
    internal class LimitedStream : InputStream
    {
        private readonly Stream _stream;
        private long _remaining;

        public LimitedStream(Stream stream, long length)
        {
            if (length < 0) throw new ArgumentOutOfRangeException("length");
            if (stream == null) throw new ArgumentNullException("stream");
            if (!stream.CanRead) throw new ArgumentException("stream");
            _stream = stream;
            _remaining = length;
        }

        protected override int ReadNextBlock(byte[] buffer, int offset, int count)
        {
            if (count > _remaining) count = (int) _remaining;
            int bytesRead = _stream.Read(buffer, offset, count);
            if (bytesRead > 0) _remaining -= bytesRead;
            return bytesRead;
        }
    }
    
    #endregion
}