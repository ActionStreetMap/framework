using System.Collections.Generic;
using System.IO;

namespace ActionStreetMap.Osm.Index.Spatial
{
    internal sealed class SpatialIndexSerializer
    {
        public const uint Marker = uint.MaxValue;

        public static int Statistic;

        #region Serialization

        public static void Serialize(RTree<uint> tree, BinaryWriter writer)
        {
            WriteNode(tree.Root, writer);
        }

        private static void WriteNode(RTree<uint>.RTreeNode node, BinaryWriter writer)
        {
            // write data
            Statistic++;
            writer.Write(node.Data);

            var isPointEnvelop = node.Envelope is PointEnvelop;
            // save one extra byte
            byte packedValues = (byte) ((isPointEnvelop ? 1 : 0) + (node.IsLeaf ? 2 : 0));
            writer.Write(packedValues);

            writer.Write(node.Envelope.MinPointLatitude);
            writer.Write(node.Envelope.MinPointLongitude);

            if (!isPointEnvelop)
            {
                writer.Write(node.Envelope.MaxPointLatitude);
                writer.Write(node.Envelope.MaxPointLongitude);
            }

            foreach (var rTreeNode in node.Children)
                WriteNode(rTreeNode, writer);

            writer.Write(Marker);
        }

        #endregion

        #region Deserialization

        public static SpatialIndex<uint> Deserialize(BinaryReader reader)
        {
            SpatialIndex<uint>.SpatialIndexNode root;
            ReadNode(out root, reader);
            return new SpatialIndex<uint>(root);
        }

        private static bool ReadNode(out SpatialIndex<uint>.SpatialIndexNode root, BinaryReader reader)
        {
            var data = reader.ReadUInt32();
            if (data == Marker)
            {
                root = default(SpatialIndex<uint>.SpatialIndexNode);
                return true;
            }

            var packedValues = reader.ReadByte();

            bool isPointEnvelop = (packedValues & 1) > 0;
            bool isLeaf = (packedValues >> 1) > 0;

            IEnvelop envelop = isPointEnvelop ? 
                (IEnvelop) new PointEnvelop(reader.ReadInt32(), reader.ReadInt32()) : 
                new Envelop(reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32());

            var children = new List<SpatialIndex<uint>.SpatialIndexNode>();
            while (true)
            {
                SpatialIndex<uint>.SpatialIndexNode child;
                if (ReadNode(out child, reader))
                    break;
                children.Add(child);
            }
            root = new SpatialIndex<uint>.SpatialIndexNode(data, envelop, children.Count > 0 ? children.ToArray() : null);
            root.IsLeaf = isLeaf;

            return false;
        }

        #endregion
    }
}

