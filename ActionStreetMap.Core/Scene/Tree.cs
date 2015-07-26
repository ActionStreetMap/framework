using ActionStreetMap.Core.Geometry;

namespace ActionStreetMap.Core.Scene
{
    /// <summary> Represents a tree. </summary>
    public class Tree
    {
        // TODO define more properties supported by OSM 

        /// <summary> Gets or sets tree id. Can be ignored? </summary>
        public long Id { get; set; }

        /// <summary> Gets or sets type of tree. </summary>
        public int Type { get; set; }

        /// <summary> Gets or sets tree position. </summary>
        public Vector2d Point { get; set; }
    }
}
