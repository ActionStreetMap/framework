namespace ActionStreetMap.Core.Scene.Details
{
    /// <summary>
    ///     Represents a tree. Actually, it can define additional info like height, description, type, etc. as OSM supports this
    /// </summary>
    public class Tree
    {
        /// <summary> Gets or sets tree id. Can be ignored? </summary>
        public long Id { get; set; }

        /// <summary> Gets or sets type of tree. </summary>
        public int Type { get; set; }

        /// <summary> Gets or sets tree position. </summary>
        public MapPoint Point { get; set; }
    }
}
