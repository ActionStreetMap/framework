using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ActionStreetMap.Core.Scene.Details
{
    /// <summary>
    ///     Represents a tree. Actually, it can define additional info like height, description, type, etc. as OSM supports this
    /// </summary>
    public class Tree
    {
        /// <summary>
        ///     Tree id. Can be ignored?
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        ///     Type of tree.
        /// </summary>
        public int Type { get; set; }

        /// <summary>
        ///     Gets or sets tree position
        /// </summary>
        public MapPoint Point { get; set; }
    }
}
