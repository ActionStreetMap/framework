using ActionStreetMap.Core.Geometry;

namespace ActionStreetMap.Core.Scene
{
    /// <summary> Represents a tree. </summary>
    public class Tree
    {
        // TODO define more properties supported by OSM 

        /// <summary> Tree id. </summary>
        public long Id;

        /// <summary> Type of tree. </summary>
        public int Type;

        /// <summary> Tree position. </summary>
        public Vector2d Point;
    }
}
