using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ActionStreetMap.Osm.Entities;

namespace ActionStreetMap.Osm.Index.Search
{
    internal class DocumentSerializer
    {
        private const byte NodeType = 0;
        private const byte WayType = 1;
        private const byte RelationType = 1;

        #region Serialize

        public static byte[] Serialize(Document document)
        {
            // TODO use buffer
            using (var stream = new MemoryStream())
            {
                var element = document.Element;
                var writer = new BinaryWriter(stream, Encoding.Unicode);
                //writer.Write(document.DocNumber);
                writer.Write(element.Id);
                SerializeElement(element, writer);
                return stream.ToArray();
            }
        }

        private static void SerializeElement(Element element, BinaryWriter writer)
        {
            SerializeTags(element.Tags, writer);
            if (element is Way)
                SerializeWay(element as Way, writer);
            else if (element is Relation)
                SerializeRelation(element as Relation, writer);
            else 
                SerializeNode(element as Node, writer);
        }

        private static void SerializeNode(Node node, BinaryWriter writer)
        {
            writer.Write(NodeType);
            var scaled = new ScaledGeoCoordinate(node.Coordinate);
            writer.Write(scaled.Latitude);
            writer.Write(scaled.Longitude);
        }

        private static void SerializeWay(Way way, BinaryWriter writer)
        {
            throw new NotImplementedException();
        }

        private static void SerializeRelation(Relation relation, BinaryWriter writer)
        {
            throw new NotImplementedException();
        }

        private static void SerializeTags(Dictionary<string, string> tags, BinaryWriter writer)
        {
            var count = tags.Count;
            writer.Write(count);
            foreach (KeyValuePair<string, string> pair in tags)
            {
                writer.Write(pair.Key);
                writer.Write(pair.Value);
            }
        }

        #endregion

        #region Deserialize

        public static Document Deserialize(byte[] data)
        {
            var reader = new BinaryReader(new MemoryStream(data), Encoding.Unicode);
            //var docId = reader.ReadInt32();
            var elementId = reader.ReadInt64();
            var element = DeserializeElement(reader);
            element.Id = elementId;
            return new Document(element)
            {
                //DocNumber = docId
            };
        }

        private static Element DeserializeElement(BinaryReader reader)
        {
            var tags = DeserializeTags(reader);
            var typeId = reader.ReadByte();
            Element element;
            if (typeId == WayType)
                element = DeserializeWay(reader);
            else if (typeId == RelationType)
                element = DeserializeRelation(reader);
            else
                element = DeserializeNode(reader);

            element.Tags = tags;

            return element;
        }

        private static Node DeserializeNode(BinaryReader reader)
        {
            Node node = new Node();
            var scaled = new ScaledGeoCoordinate(reader.ReadInt32(), reader.ReadInt32());
            node.Coordinate = scaled.Unscale();

            return node;
        }

        private static Way DeserializeWay(BinaryReader reader)
        {
            throw new NotImplementedException();
        }

        private static Relation DeserializeRelation(BinaryReader reader)
        {
            throw new NotImplementedException();
        }

        private static Dictionary<string, string> DeserializeTags(BinaryReader reader)
        {
            var count = reader.ReadInt32();
            var tags = new Dictionary<string, string>(count);
            for (int i = 0; i < count; i++)
            {
                var key = reader.ReadString();
                var value = reader.ReadString();
                tags.Add(key, value);
            }
            return tags;
        }

        #endregion
    }
}
