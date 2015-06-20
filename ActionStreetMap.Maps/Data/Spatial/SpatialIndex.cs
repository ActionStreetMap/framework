using System.Collections.Generic;
using System.IO;
using ActionStreetMap.Core;
using ActionStreetMap.Infrastructure.Reactive;
using ActionStreetMap.Maps.Data.Helpers;
using ActionStreetMap.Maps.Helpers;

namespace ActionStreetMap.Maps.Data.Spatial
{
    /// <summary> Represents readonly spatial index. </summary>
    internal class SpatialIndex : ISpatialIndex<uint>
    {
        private const uint Marker = uint.MaxValue;

        private readonly SpatialIndexNode _root;

        /// <summary> Creates <see cref="SpatialIndex"/>. </summary>
        public SpatialIndex(SpatialIndexNode root)
	    {
	        _root = root;
	    }

        /// <inheritdoc />
        public IObservable<uint> Search(BoundingBox query)
        {
            return Search(query, MapConsts.MaxZoomLevel);
        }

        /// <inheritdoc />
        public IObservable<uint> Search(BoundingBox query, int zoomLevel)
        {
            return Search(new Envelop(query.MinPoint, query.MaxPoint), zoomLevel);
        }

        /// <inheritdoc />
        public void Insert(uint data, BoundingBox boundingBox)
        {
            throw new System.NotSupportedException();
        }

        public void Remove(uint data, BoundingBox boundingBox)
        {
            throw new System.NotSupportedException();
        }

        private IObservable<uint> Search(IEnvelop envelope, int zoomLevel)
        {
            var minMargin = ZoomHelper.GetMinMargin(zoomLevel);
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
                            var childEnvelope = child.Envelope;
                            if (envelope.Intersects(childEnvelope))
                            {
                                if (node.IsLeaf && childEnvelope.Margin >= minMargin)
                                    observer.OnNext(child.Data);
                                else if (envelope.Contains(childEnvelope))
                                    Collect(child, minMargin, observer);
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

        private static void Collect(SpatialIndexNode node, long minMargin, IObserver<uint> observer)
        {
            var nodesToSearch = new Stack<SpatialIndexNode>();
            while (node.Envelope != null)
            {
                if (node.Children != null)
                {
                    if (node.IsLeaf)
                        foreach(var child in node.Children)
                            if (child.Envelope.Margin >= minMargin)
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
                SpatialIndexNode root;
                ReadNode(reader, out root);
                return new SpatialIndex(root);
            }
        }

        private static bool ReadNode(BinaryReader reader, out SpatialIndexNode root)
        {
            var data = reader.ReadUInt32();
            if (data == Marker)
            {
                root = default(SpatialIndexNode);
                return true;
            }

            var packedValues = reader.ReadByte();

            bool isPointEnvelop = (packedValues & 1) > 0;
            bool isLeaf = (packedValues >> 1) > 0;

            IEnvelop envelop = isPointEnvelop ?
                (IEnvelop)new PointEnvelop(reader.ReadInt32(), reader.ReadInt32()) :
                new Envelop(reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32());

            List<SpatialIndexNode> children = null;
            while (true)
            {
                SpatialIndexNode child;
                if (ReadNode(reader, out child))
                    break;
                if (children == null)
                    children = new List<SpatialIndexNode>(64);
                children.Add(child);
            }

            root = new SpatialIndexNode(data, envelop, children != null && children.Count > 0 ? children.ToArray() : null);
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
