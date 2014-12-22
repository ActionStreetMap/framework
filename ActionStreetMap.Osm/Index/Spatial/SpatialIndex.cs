using System.Collections.Generic;
using System.IO;
using System.Linq;

using ActionStreetMap.Osm.Extensions;

namespace ActionStreetMap.Osm.Index.Spatial
{
    /// <summary>
    ///     Represents spatial index.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class SpatialIndex<T>
    {
        private readonly SpatialIndexNode _root;

        public SpatialIndex(SpatialIndexNode root)
	    {
	        _root = root;
	    }

        public IEnumerable<T> Search(Envelop envelope)
        {
            var node = _root;

            if (!envelope.Intersects(node.Envelope))
                return Enumerable.Empty<T>();

            var retval = new List<SpatialIndexNode>();
            var nodesToSearch = new Stack<SpatialIndexNode>();

            while (node.Envelope != null)
            {
                if (node.Children != null)
                {
                    foreach (var child in node.Children)
                    {
                        if (envelope.Intersects(child.Envelope))
                        {
                            if (node.IsLeaf)
                                retval.Add(child);
                            else if (envelope.Contains(child.Envelope))
                                Collect(child, retval);
                            else
                                nodesToSearch.Push(child);
                        }
                    }
                }
                node = nodesToSearch.TryPop();
            }

            return retval.Select(n => n.Data);
        }

        private static void Collect(SpatialIndexNode node, List<SpatialIndexNode> result)
        {
            var nodesToSearch = new Stack<SpatialIndexNode>();
            while (node.Envelope != null)
            {
                if (node.Children != null)
                {
                    if (node.IsLeaf)
                        result.AddRange(node.Children);
                    else
                    {
                        foreach (var n in node.Children)
                            nodesToSearch.Push(n);
                    }
                }
                node = nodesToSearch.TryPop();
            }
        }

        public const uint Marker = uint.MaxValue;

        #region Static: Save

        public static void Save(RTree<uint> tree, Stream stream)
        {
            using (var writer = new BinaryWriter(stream))
            {
                WriteNode(tree.Root, writer);
            }
        }

        private static void WriteNode(RTree<uint>.RTreeNode node, BinaryWriter writer)
        {
            // write data
            writer.Write(node.Data);

            var isPointEnvelop = node.Envelope is PointEnvelop;
            // save one extra byte
            byte packedValues = (byte)((isPointEnvelop ? 1 : 0) + (node.IsLeaf ? 2 : 0));
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

        #region Static: Load

        public static SpatialIndex<uint> Load(Stream stream)
        {
            using (var reader = new BinaryReader(stream))
            {
                SpatialIndex<uint>.SpatialIndexNode root;
                ReadNode(out root, reader);
                return new SpatialIndex<uint>(root);
            }
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
                (IEnvelop)new PointEnvelop(reader.ReadInt32(), reader.ReadInt32()) :
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

        #region Nested 

        internal struct SpatialIndexNode
        {
            public T Data;
            public IEnvelop Envelope;
            public bool IsLeaf;
            public SpatialIndexNode[] Children;

            public SpatialIndexNode(T data, IEnvelop envelope, SpatialIndexNode[] children)
            {
                Data = data;
                Envelope = envelope;
                Children = children;
                IsLeaf = false;
            }
        }

        #endregion
    }
}
