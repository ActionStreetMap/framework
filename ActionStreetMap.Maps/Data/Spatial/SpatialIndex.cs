using System.Collections.Generic;
using System.IO;
using ActionStreetMap.Core;
using ActionStreetMap.Infrastructure.Reactive;
using ActionStreetMap.Maps.Extensions;

namespace ActionStreetMap.Maps.Data.Spatial
{
    /// <summary>
    ///     Represents readonly spatial index.
    /// </summary>
    internal class SpatialIndex : ISpatialIndex<uint>
    {
        private const uint Marker = uint.MaxValue;

        private readonly SpatialIndexNode _root;

        public SpatialIndex(SpatialIndexNode root)
	    {
	        _root = root;
	    }

        public IObservable<uint> Search(BoundingBox query)
        {
            return Search(new Envelop(query.MinPoint, query.MaxPoint));
        }

        private IObservable<uint> Search(IEnvelop envelope)
        {
            return Observable.Create<uint>(observer =>
            {
                var node = _root;
                if (!envelope.Intersects(node.Envelope))
                {
                    observer.OnCompleted();
                    return Disposable.Empty;
                }

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
                                    observer.OnNext(child.Data);
                                else if (envelope.Contains(child.Envelope))
                                    Collect(child, observer);
                                else
                                    nodesToSearch.Push(child);
                            }
                        }
                    }
                    node = nodesToSearch.TryPop();
                }
                observer.OnCompleted();
                return Disposable.Empty;
            });
        }

        private static void Collect(SpatialIndexNode node, IObserver<uint> observer)
        {
            var nodesToSearch = new Stack<SpatialIndexNode>();
            while (node.Envelope != null)
            {
                if (node.Children != null)
                {
                    if (node.IsLeaf)
                        foreach(var child in node.Children)
                            observer.OnNext(child.Data);
                    else
                        foreach (var n in node.Children)
                            nodesToSearch.Push(n);
                }
                node = nodesToSearch.TryPop();
            }
        }

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

        public static SpatialIndex Load(Stream stream)
        {
            using (var reader = new BinaryReader(stream))
            {
                SpatialIndex.SpatialIndexNode root;
                ReadNode(out root, reader);
                return new SpatialIndex(root);
            }
        }

        private static bool ReadNode(out SpatialIndex.SpatialIndexNode root, BinaryReader reader)
        {
            var data = reader.ReadUInt32();
            if (data == Marker)
            {
                root = default(SpatialIndex.SpatialIndexNode);
                return true;
            }

            var packedValues = reader.ReadByte();

            bool isPointEnvelop = (packedValues & 1) > 0;
            bool isLeaf = (packedValues >> 1) > 0;

            IEnvelop envelop = isPointEnvelop ?
                (IEnvelop)new PointEnvelop(reader.ReadInt32(), reader.ReadInt32()) :
                new Envelop(reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32());

            List<SpatialIndex.SpatialIndexNode> children = null;
            while (true)
            {
                SpatialIndex.SpatialIndexNode child;
                if (ReadNode(out child, reader))
                    break;
                if (children == null)
                    children = new List<SpatialIndex.SpatialIndexNode>();
                children.Add(child);
            }
            root = new SpatialIndex.SpatialIndexNode(data, envelop, children != null && children.Count > 0 ? children.ToArray() : null);
            root.IsLeaf = isLeaf;
            return false;
        }

        #endregion

        #region Static: Convert

        public static SpatialIndex ToReadOnly(RTree<uint> rTree)
        {
            return new SpatialIndex(VisitTree(rTree.Root));
        }

        private static SpatialIndexNode VisitTree(RTree<uint>.RTreeNode rNode)
        {
            var children = new SpatialIndexNode[rNode.Children.Count];
            for (int i = 0; i < rNode.Children.Count; i++)
                children[i] = VisitTree(rNode.Children[i]);

            return new SpatialIndexNode(rNode.Data, rNode.Envelope, children)
            {
                IsLeaf = rNode.IsLeaf
            };
        }

        #endregion

        #region Nested

        internal struct SpatialIndexNode
        {
            public uint Data;
            public IEnvelop Envelope;
            public bool IsLeaf;
            public SpatialIndexNode[] Children;

            public SpatialIndexNode(uint data, IEnvelop envelope, SpatialIndexNode[] children)
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
