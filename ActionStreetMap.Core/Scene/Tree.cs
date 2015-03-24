namespace ActionStreetMap.Core.Scene.Details
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
        public MapPoint Point { get; set; }
    }
}
