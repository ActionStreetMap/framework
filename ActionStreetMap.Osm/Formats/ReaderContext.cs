using System.IO;
using ActionStreetMap.Osm.Index.Import;

namespace ActionStreetMap.Osm.Formats
{
    internal class ReaderContext
    {
        public Stream SourceStream;

        public IndexBuilder Builder;

        public bool SkipTags;
        public bool SkipNodes;
        public bool SkipWays;
        public bool SkipRels;

        public long[] SkipArray;

        public bool ReuseEntities;
    }
}
