using System;
using System.Collections.Generic;
using System.Linq;
using ActionStreetMap.Infrastructure.Utilities;

namespace ActionStreetMap.Osm.Index.Spatial
{
    [Serializable]
    public class SpatialIndex<T>
    {
        private SpatialIndexNode _root;
        public SpatialIndexNode Root { get { return _root; } }

        public SpatialIndex(SpatialIndexNode root)
	    {
	        _root = root;
	    }

        public int Statistics = 0;
        public IEnumerable<T> Search(IEnvelop envelope)
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
                
                Statistics++;
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

        [Serializable]
        public struct SpatialIndexNode
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
    }
}
