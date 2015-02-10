using ActionStreetMap.Core.Tiling.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ActionStreetMap.Tests
{
    internal static class TestExtensions
    {
        public static TagCollection ToTags(this Dictionary<string, string> dict)
        {
            TagCollection tags = new TagCollection(dict.Count);
            foreach (var kv in dict)
                tags.Add(kv.Key, kv.Value);
            return tags.Complete();
        }
    }
}
