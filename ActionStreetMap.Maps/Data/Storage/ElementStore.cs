using System;
using System.Collections.Generic;
using System.IO;
using ActionStreetMap.Core;
using ActionStreetMap.Maps.Data.Helpers;
using ActionStreetMap.Maps.Entities;

namespace ActionStreetMap.Maps.Data.Storage
{
    internal sealed class ElementStore: IDisposable
    {
        private readonly KeyValueStore _keyValueStore;
        private readonly Stream _stream;

        // serialize specific
        private BinaryWriter _writer;
        private List<uint> _offsetBuffer;

        // deserialize specific
        private BinaryReader _reader;
        
        public ElementStore(KeyValueStore keyValueStore, Stream stream)
        {
            _keyValueStore = keyValueStore;
            _stream = stream;
        }

        #region Public methods

        /// <summary>
        ///     Inserts element into store and returns offset
        /// </summary>
        /// <param name="element"></param>
        /// <returns>Offset.</returns>
        public uint Insert(Element element)
        {
            // NOTE should be at correct position 
            if (_writer == null)
                _writer = new BinaryWriter(_stream);

            var offset = (uint) _stream.Position;
            WriteElement(element, offset, _writer);

            return offset;
        }

        public Element Get(uint offset)
        {
            if (_reader == null)
                _reader = new BinaryReader(_stream);

            // TODO read and restore previous position to support write/read sessions
            // NOTE see relation processing inside index builder
            var previousPosition = _stream.Position;
            _stream.Seek(offset, SeekOrigin.Begin);
            var element =  ReadElement(_reader);
            _stream.Seek(previousPosition, SeekOrigin.Begin);
            return element;
        }

        #endregion

        #region Private: Write

        private void WriteElement(Element element, uint offset, BinaryWriter writer)
        {
            writer.Write(element.Id);
            if (element is Way)
                WriteWay(element as Way, writer);
            else if (element is Relation)
                WriteRelation(element as Relation, writer);
            else
                WriteNode(element as Node, writer);

            // prepare tag offset buffer
            if(_offsetBuffer == null)
                _offsetBuffer = new List<uint>(128);
            _offsetBuffer.Clear();

            if (element.Tags != null)
            {
                foreach (KeyValuePair<string, string> pair in element.Tags)
                    _offsetBuffer.Add(_keyValueStore.Insert(pair, offset));
            }

            WriteTags(_offsetBuffer, writer);
        }

        private static void WriteNode(Node node, BinaryWriter writer)
        {
            writer.Write((byte)ElementType.Node);
            WriteCoordinate(node.Coordinate, writer);
        }

        private static void WriteWay(Way way, BinaryWriter writer)
        {
            writer.Write((byte)ElementType.Way);
            writer.Write((ushort)way.Coordinates.Count);
            foreach (var coordinate in way.Coordinates)
                WriteCoordinate(coordinate, writer);
        }

        private static void WriteRelation(Relation relation, BinaryWriter writer)
        {
            writer.Write((byte)ElementType.Relation);
            writer.Write((ushort)relation.Members.Count);
            foreach (var relationMember in relation.Members)
            {
                writer.Write(relationMember.Role);
                writer.Write(relationMember.Offset);
            }
        }

        private static void WriteTags(List<uint> tagOffsets, BinaryWriter writer)
        {
            var count = (ushort)tagOffsets.Count;
            writer.Write(count);
            foreach (uint offset in tagOffsets)
                writer.Write(offset);
        }

        private static void WriteCoordinate(GeoCoordinate coordinate, BinaryWriter writer)
        {
            var scaled = new ScaledGeoCoordinate(coordinate);
            writer.Write(scaled.Latitude);
            writer.Write(scaled.Longitude);
        }

        #endregion

        #region Private: Read

        private Element ReadElement(BinaryReader reader)
        {
            var elementId = reader.ReadInt64();
            var type = (ElementType) reader.ReadByte();
            Element element;
            switch (type)
            {
                 case ElementType.Node:
                    element = ReadNode(reader);
                    break;
                 case ElementType.Way:
                    element = ReadWay(reader);
                    break;
                 case ElementType.Relation:
                    element = ReadRelation(reader);
                    break;
                default:
                    throw new InvalidOperationException(String.Format("Unknown element type: {0}", type));
            }

            element.Id = elementId;
            var tags = ReadTags(_keyValueStore, reader);
            element.Tags = tags;
            return element;
        }

        private static Node ReadNode(BinaryReader reader)
        {
            return new Node { Coordinate = ReadCoordinate(reader)};
        }

        private static Way ReadWay(BinaryReader reader)
        {
            Way way = new Way();
            var count = reader.ReadUInt16();
            // TODO use object pool
            var coordinates = new List<GeoCoordinate>(count);
            for (int i = 0; i < count; i++)
                coordinates.Add(ReadCoordinate(reader));
            way.Coordinates = coordinates;
            return way;
        }

        private Relation ReadRelation(BinaryReader reader)
        {
            var relation = new Relation();
            var count = reader.ReadUInt16();
            relation.Members = new List<RelationMember>(count);
            
            for (int i = 0; i < count; i++)
            {
                var role = reader.ReadString();
                var offset = reader.ReadUInt32();
                var position = reader.BaseStream.Position;
                Element element = Get(offset);
                reader.BaseStream.Seek(position, SeekOrigin.Begin);
                relation.Members.Add(new RelationMember()
                {
                    Role = role,
                    Member = element
                });
            }
            return relation;
        }

        private static Dictionary<string, string> ReadTags(KeyValueStore keyValueStore, BinaryReader reader)
        {
            var count = reader.ReadUInt16();
            var tags = new Dictionary<string, string>(count);
            for (int i = 0; i < count; i++)
            {
                var offset = reader.ReadUInt32();
                var tag = keyValueStore.Get(offset);
                tags.Add(tag.Key, tag.Value);
            }
            return tags;
        }

        private static GeoCoordinate ReadCoordinate(BinaryReader reader)
        {
            var scaled = new ScaledGeoCoordinate(reader.ReadInt32(), reader.ReadInt32());
            return scaled.Unscale();
        }

        #endregion

        public void Dispose()
        {
            _keyValueStore.Dispose();
            if (_writer != null)
                _writer.Close();
            if(_reader != null)
                _reader.Close();
            _stream.Dispose();
        }
    }
}
