using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using ActionStreetMap.Core;
using ActionStreetMap.Maps.Entities;

namespace ActionStreetMap.Maps.Formats.O5m
{
    /// <summary>
    ///     Reads o5m format. Ported from spliter utility written on Java. 
    ///     NOTE: refactor it first if you want to expose it as part of fwk
    /// </summary>
    internal class O5mReader : IReader, IDisposable
    {
        // O5M data set constants
        private const int NodeDataset = 0x10;
        private const int WayDataset = 0x11;
        private const int RelDataset = 0x12;
        private const int BboxDataset = 0xdb;
        private const int TimestampDataset = 0xdc;
        private const int HeaderDataset = 0xe0;
        private const int EodFlag = 0xfe;
        private const int ResetFlag = 0xff;

        private const int EofFlag = -1;

        // o5m constants
        private const int StringTableSize = 15000;
        private const int MaxStringPairSize = 250 + 2;
        private readonly string[] _relRefTypes = {"node", "way", "relation", "?"};
        private const double FACTOR = 1d/1000000000; // used with 100*<Val>*FACTOR 

        // performance: save byte position of first occurrence of a data set type (node, way, relation)
        // to allow skipping large parts of the stream
        private readonly long[] _firstPosInFile;

        private byte[] _ioBuf;

        private Stream _fis;
        private ReaderContext _context;

        private int _ioPos;

        // the o5m string table
        private String[,] _stringTable;
        private readonly String[] _stringPair;
        private int _currStringTablePos;

        // buffer for byte -> String conversions
        private readonly byte[] _cnvBuffer;

        private long _countBytes;
        // a counter that must be maintained by all routines that read data from the stream
        private int _bytesToRead;

        // for delta calculations
        private long _lastNodeId;
        private long _lastWayId;
        private long _lastRelId;
        private long[] _lastRef;
        private long _lastTs;
        private long _lastChangeSet;
        private int _lastLon, _lastLat;

        // reused entities
        private Node _node;
        private Way _way;
        private Relation _relation;
        private List<long> _reusableIdList;
        private List<RelationMember> _reusableRelMemberList;

        public O5mReader()
        {
            _ioBuf = new byte[8192];
            _cnvBuffer = new byte[4000]; // OSM data should not contain string pairs with length > 512
            _ioPos = 0;
            _stringPair = new String[2];
            _lastRef = new long[3];
 
            _firstPosInFile = new long[256];
            for (int i = 0; i < _firstPosInFile.Length; i++)
                _firstPosInFile[i] = -1;
        }

        private void InitializeFromContext(ReaderContext context)
        {
            _context = context;
            _fis = context.SourceStream;
            if (context.ReuseEntities)
            {
                _node = new Node();
                _way = new Way();
                _relation = new Relation();
                _reusableIdList = new List<long>(128);
                _reusableRelMemberList = new List<RelationMember>(128);
            }
        }

        public void Read(ReaderContext context)
        {
            InitializeFromContext(context);

            int start = _fis.ReadByte();
            ++_countBytes;
            if (start != ResetFlag)
                throw new IOException("Wrong header byte " + start);
            if (_context.SkipArray != null)
            {
                if (_context.SkipNodes)
                {
                    if (_context.SkipWays)
                        Skip(_context.SkipArray[RelDataset] - _countBytes); // jump to first relation
                    else
                        Skip(_context.SkipArray[WayDataset] - _countBytes); // jump to first way
                }
            }
            ReadFile();
        }

        private void ReadFile()
        {
            bool done = false;
            while (!done)
            {
                long size = 0;
                int fileType = _fis.ReadByte();
                ++_countBytes;
                if (fileType >= 0 && fileType < 0xf0)
                {
                    if (_context.SkipArray == null)
                    {
                        // save first occurrence of a data set type
                        if (_firstPosInFile[fileType] == -1)
                            _firstPosInFile[fileType] = Math.Max(0, _countBytes - 1);
                    }
                    _bytesToRead = 0;
                    size = ReadUnsignedNum64FromStream();
                    _countBytes += size - _bytesToRead; // bytesToRead is negative 
                    _bytesToRead = (int) size;

                    bool doSkip = false;
                    if (fileType == NodeDataset && _context.SkipNodes) doSkip = true;
                    else if (fileType == WayDataset && _context.SkipWays) doSkip = true;
                    else if (fileType == RelDataset && _context.SkipRels) doSkip = true;
                    switch (fileType)
                    {
                        case NodeDataset:
                        case WayDataset:
                        case RelDataset:
                        case BboxDataset:
                        case TimestampDataset:
                        case HeaderDataset:
                            if (doSkip)
                            {
                                Skip(_bytesToRead);
                                continue;
                            }
                            if (_bytesToRead > _ioBuf.Length)
                                _ioBuf = new byte[_bytesToRead + 100];

                            int bytesRead = 0;
                            int neededBytes = _bytesToRead;
                            while (neededBytes > 0)
                            {
                                bytesRead += _fis.Read(_ioBuf, bytesRead, neededBytes);
                                neededBytes -= bytesRead;
                            }
                            _ioPos = 0;
                            break;
                    }
                }
                if (fileType == EofFlag) done = true;
                else if (fileType == NodeDataset) ReadNode();
                else if (fileType == WayDataset) ReadWay();
                else if (fileType == RelDataset) ReadRelation();
                else if (fileType == BboxDataset) ReadBBox();
                else if (fileType == TimestampDataset) ReadFileTimestamp();
                else if (fileType == HeaderDataset) ReadHeader();
                else if (fileType == EodFlag) done = true;
                else if (fileType == ResetFlag) Reset();
                else
                {
                    if (fileType < 0xf0) 
                        Skip(size); // skip unknown data set 
                }
            }
        }

        /// <summary>Skip the given number of bytes.</summary>
        /// <param name="bytes">Skip count.</param>
        private void Skip(long bytes)
        {
            _fis.Seek(bytes, SeekOrigin.Current);
        }

        /// <summary>
        ///     Resets the delta values and string table 
        /// </summary>
        private void Reset()
        {
            _lastNodeId = 0;
            _lastWayId = 0;
            _lastRelId = 0;
            _lastRef[0] = 0; _lastRef[1] = 0; _lastRef[2] = 0;
            _lastTs = 0;
            _lastChangeSet = 0;
            _lastLon = 0;
            _lastLat = 0;
            _stringTable = new string[2, StringTableSize];
            _currStringTablePos = 0;
        }

        #region read Elements

        private void ReadNode()
        {
            _lastNodeId += ReadSignedNum64();
            if (_bytesToRead == 0)
                return; // only nodeId: this is a delete action, we ignore it 
            ReadVersionTsAuthor();
            if (_bytesToRead == 0)
                return; // only nodeId+version: this is a delete action, we ignore it 
            int lon = ReadSignedNum32() + _lastLon;
            _lastLon = lon;
            int lat = ReadSignedNum32() + _lastLat;
            _lastLat = lat;

            double flon = 100L*lon*FACTOR;
            double flat = 100L*lat*FACTOR;
            Debug.Assert(flat >= -90.0 && flat <= 90.0);
            Debug.Assert(flon >= -180.0 && flon <= 180.0);

            var node = _context.ReuseEntities ? _node : new Node();
            node.Id = _lastNodeId;
            node.Coordinate = new GeoCoordinate(flat, flon);

            var tagCount = ReadTags(node);
            _context.Builder.ProcessNode(node, tagCount);
        }

        private void ReadWay()
        {
            _lastWayId += ReadSignedNum64();
            if (_bytesToRead == 0)
                return; // only wayId: this is a delete action, we ignore it 

            ReadVersionTsAuthor();
            if (_bytesToRead == 0)
                return; // only wayId + version: this is a delete action, we ignore it 
            var way = _context.ReuseEntities ? _way : new Way();
            way.Id = _lastWayId;
            long refSize = ReadUnsignedNum32();
            long stop = _bytesToRead - refSize;

            if (_context.ReuseEntities) _reusableIdList.Clear();
            way.NodeIds = _context.ReuseEntities ? _reusableIdList : new List<long>((int) (_bytesToRead - stop)/4);
            while (_bytesToRead > stop)
            {
                _lastRef[0] += ReadSignedNum64();
                way.NodeIds.Add(_lastRef[0]);
            }

            var tagCount =  ReadTags(way);
            _context.Builder.ProcessWay(way, tagCount);
        }

        private void ReadRelation()
        {
            _lastRelId += ReadSignedNum64();
            if (_bytesToRead == 0)
                return; // only relId: this is a delete action, we ignore it 
            ReadVersionTsAuthor();
            if (_bytesToRead == 0)
                return; // only relId + version: this is a delete action, we ignore it 

            var rel = _context.ReuseEntities ? _relation : new Relation();
            rel.Id = _lastRelId;

            long refSize = ReadUnsignedNum32();
            long stop = _bytesToRead - refSize;
            if (_context.ReuseEntities) _reusableRelMemberList.Clear();
            rel.Members = _context.ReuseEntities ? _reusableRelMemberList : new List<RelationMember>((int) (_bytesToRead - stop)/4);
            while (_bytesToRead > stop)
            {
                long deltaRef = ReadSignedNum64();
                int refType = ReadRelRef();
                _lastRef[refType] += deltaRef;

                rel.Members.Add(new RelationMember
                {
                    TypeId = refType,
                    MemberId = _lastRef[refType],
                    Role = _stringPair[1],
                });
            }

            // tags
            var tagCount = ReadTags(rel);
            _context.Builder.ProcessRelation(rel, tagCount);
        }

        /// <summary>
        ///     Reads object type ("0".."2") concatenated with role (single string)
        /// </summary>
        /// <returns>0..3 for type (3 means unknown)</returns>
        private int ReadRelRef()
        {
            int refType;
            long toReadStart = _bytesToRead;
            int stringRef = ReadUnsignedNum32();
            if (stringRef == 0)
            {
                refType = _ioBuf[_ioPos++] - 0x30;
                --_bytesToRead;

                if (refType < 0 || refType > 2)
                    refType = 3;
                _stringPair[0] = _relRefTypes[refType];

                int buffPos = 0;
                _stringPair[1] = null;
                while (_stringPair[1] == null)
                {
                    int b = _ioBuf[_ioPos++];
                    --_bytesToRead;
                    _cnvBuffer[buffPos++] = (byte) b;

                    if (b == 0)
                        _stringPair[1] = Encoding.UTF8.Decode(_cnvBuffer, 0, buffPos - 1);
                }
                long bytes = toReadStart - _bytesToRead;
                if (bytes <= MaxStringPairSize)
                    StoreStringPair();
            }
            else
            {
                SetStringRefPair(stringRef);
                char c = _stringPair[0][0];
                switch (c)
                {
                    case 'n':
                        refType = 0;
                        break;
                    case 'w':
                        refType = 1;
                        break;
                    case 'r':
                        refType = 2;
                        break;
                    default:
                        refType = 3;
                        break;
                }
            }
            return refType;
        }

        #endregion

        #region Read methods

        /// <summary>
        ///     Reads and verify o5m header (known values are o5m2 and o5c2)
        /// </summary>
        private void ReadHeader()
        {
            if (_ioBuf[0] != 'o' || _ioBuf[1] != '5' || (_ioBuf[2] != 'c' && _ioBuf[2] != 'm') || _ioBuf[3] != '2')
            {
                throw new IOException("unsupported header");
            }
        }

        /// <summary>
        ///     Reads version, time stamp and change set and author. We are not interested in the values, but we have to maintain
        ///     the string table.
        /// </summary>
        private void ReadVersionTsAuthor()
        {
            int version = ReadUnsignedNum32();
            if (version != 0)
            {
                // version info
                long ts = ReadSignedNum64() + _lastTs;
                _lastTs = ts;
                if (ts != 0)
                {
                    long changeSet = ReadSignedNum32() + _lastChangeSet;
                    _lastChangeSet = changeSet;
                    ReadAuthor();
                }
            }
        }

        private void ReadAuthor()
        {
            int stringRef = ReadUnsignedNum32();
            if (stringRef == 0)
            {
                long toReadStart = _bytesToRead;
                long uidNum = ReadUnsignedNum64();
                if (uidNum == 0)
                    _stringPair[0] = String.Empty;
                else
                {
                    _stringPair[0] = uidNum.ToString(CultureInfo.InvariantCulture);
                    _ioPos++; // skip terminating zero from uid
                    --_bytesToRead;
                }
                int buffPos = 0;
                _stringPair[1] = null;
                while (_stringPair[1] == null)
                {
                    int b = _ioBuf[_ioPos++];
                    --_bytesToRead;
                    _cnvBuffer[buffPos++] = (byte) b;
                    if (b == 0)
                        _stringPair[1] = Encoding.UTF8.Decode(_cnvBuffer, 0, buffPos - 1);
                }
                long bytes = toReadStart - _bytesToRead;
                if (bytes <= MaxStringPairSize)
                    StoreStringPair();
            }
            else
                SetStringRefPair(stringRef);
        }

        private int ReadTags(Element element)
        {
            int tagCount = 0;
            while (_bytesToRead > 0)
            {
                ReadStringPair();
                if (!_context.SkipTags)
                    element.AddTag(_stringPair[0], _stringPair[1]);
                tagCount++;
            }
            Debug.Assert(_bytesToRead == 0);
            return tagCount;
        }

        /// <summary>
        ///     Reads a string pair (see o5m definition)
        /// </summary>
        private void ReadStringPair()
        {
            int stringRef = ReadUnsignedNum32();
            if (stringRef == 0)
            {
                long toReadStart = _bytesToRead;
                int cnt = 0;
                int buffPos = 0;
                int start = 0;
                while (cnt < 2)
                {
                    int b = _ioBuf[_ioPos++];
                    --_bytesToRead;
                    _cnvBuffer[buffPos++] = (byte) b;

                    if (b == 0)
                    {
                        _stringPair[cnt] = Encoding.UTF8.Decode(_cnvBuffer, start, buffPos - start - 1);
                        ++cnt;
                        start = buffPos;
                    }
                }
                long bytes = toReadStart - _bytesToRead;
                if (bytes <= MaxStringPairSize)
                    StoreStringPair();
            }
            else
                SetStringRefPair(stringRef);
        }

        private void ReadBBox()
        {
            double minLong = 100L*ReadSignedNum32()*FACTOR;
            double minLat = 100L*ReadSignedNum32()*FACTOR;
            double maxLong = 100L*ReadSignedNum32()*FACTOR;
            double maxLat = 100L*ReadSignedNum32()*FACTOR;
            Debug.Assert(_bytesToRead == 0);

            var minPoint = new GeoCoordinate(minLat, minLong);
            var maxPoint = new GeoCoordinate(maxLat, maxLong);

            _context.Builder.ProcessBoundingBox(new BoundingBox(minPoint, maxPoint));
        }

        /// <summary>
        ///     Reads (and ignore) the file timestamp data set
        /// </summary>
        private void ReadFileTimestamp()
        {
            ReadSignedNum64();
        }

        /// <summary>
        ///     Reads a varying length unsigned number (see o5m definition).
        /// </summary>
        private long ReadUnsignedNum64FromStream()
        {
            int b = _fis.ReadByte();
            --_bytesToRead;
            long result = b;
            if ((b & 0x80) == 0)
                // just one byte
                return result;

            result &= 0x7f;
            long fac = 0x80;
            while (((b = _fis.ReadByte()) & 0x80) != 0)
            {
                // more bytes will follow
                --_bytesToRead;
                result += fac*(b & 0x7f);
                fac <<= 7;
            }
            --_bytesToRead;
            result += fac*b;
            return result;
        }

        /// <summary>
        ///     Reads a varying length signed number (see o5m definition)
        /// </summary>
        private int ReadSignedNum32()
        {
            int b = _ioBuf[_ioPos++];
            --_bytesToRead;
            int result = b;
            if ((b & 0x80) == 0)
            {
                // just one byte
                if ((b & 0x01) == 1)
                    return -1 - (result >> 1);
                return result >> 1;
            }
            int sign = b & 0x01;
            result = (result & 0x7e) >> 1;
            int fac = 0x40;
            while (((b = _ioBuf[_ioPos++]) & 0x80) != 0)
            {
                // more bytes will follow
                --_bytesToRead;
                result += fac*(b & 0x7f);
                fac <<= 7;
            }
            --_bytesToRead;
            result += fac*b;
            if (sign == 1) // negative
                return -1 - result;
            return result;
        }

        /// <summary>
        ///     Reads a varying length signed number (see o5m definition)
        /// </summary>
        private long ReadSignedNum64()
        {
            int b = _ioBuf[_ioPos++];
            --_bytesToRead;
            long result = b;
            if ((b & 0x80) == 0)
            {
                // just one byte
                if ((b & 0x01) == 1)
                    return -1 - (result >> 1);
                return result >> 1;
            }
            int sign = b & 0x01;
            result = (result & 0x7e) >> 1;
            long fac = 0x40;
            while (((b = _ioBuf[_ioPos++]) & 0x80) != 0)
            {
                // more bytes will follow
                --_bytesToRead;
                result += fac*(b & 0x7f);
                fac <<= 7;
            }
            --_bytesToRead;
            result += fac*b;
            if (sign == 1) // negative
                return -1 - result;
            return result;
        }

        /// <summary>
        ///     Reads a varying length unsigned number (see o5m definition) is similar to the 64 bit version.
        /// </summary>
        private int ReadUnsignedNum32()
        {
            int b = _ioBuf[_ioPos++];
            --_bytesToRead;
            long result = b;
            if ((b & 0x80) == 0)
            {
                // just one byte
                return (int) result;
            }
            result &= 0x7f;
            long fac = 0x80;
            while (((b = _ioBuf[_ioPos++]) & 0x80) != 0)
            {
                // more bytes will follow
                --_bytesToRead;
                result += fac*(b & 0x7f);
                fac <<= 7;
            }
            --_bytesToRead;
            result += fac*b;
            return (int) result;
        }

        /// <summary>
        ///     Reads a varying length unsigned number (see o5m definition)
        /// </summary>
        private long ReadUnsignedNum64()
        {
            int b = _ioBuf[_ioPos++];
            --_bytesToRead;
            long result = b;
            if ((b & 0x80) == 0)
            {
                // just one byte
                return result;
            }
            result &= 0x7f;
            long fac = 0x80;
            while (((b = _ioBuf[_ioPos++]) & 0x80) != 0)
            {
                // more bytes will follow
                --_bytesToRead;
                result += fac*(b & 0x7f);
                fac <<= 7;
            }
            --_bytesToRead;
            result += fac*b;
            return result;
        }

        #endregion

        #region Store

        /// <summary>
        ///     Store a new string pair (length check must be performed by caller)
        /// </summary>
        private void StoreStringPair()
        {           
            _stringTable[0, _currStringTablePos] = _stringPair[0];
            _stringTable[1, _currStringTablePos] = _stringPair[1];
            ++_currStringTablePos;
            if (_currStringTablePos >= StringTableSize)
                _currStringTablePos = 0;
        }

        /// <summary>
        ///     Set stringPair to the values referenced by given string reference. No checking is performed.
        /// </summary>
        /// <param name="ref">ref valid values are 1 .. STRING_TABLE_SIZE</param>
        private void SetStringRefPair(int @ref)
        {
            int pos = _currStringTablePos - @ref;
            if (pos < 0)
                pos += StringTableSize;
            if (pos < 0 || pos >= StringTableSize)
                throw new IOException("Invalid string table reference: " + @ref);
            _stringPair[0] = _stringTable[0, pos];
            _stringPair[1] = _stringTable[1, pos];
        }

        #endregion

        public void Dispose()
        {
            _fis.Dispose();
        }
    }
}