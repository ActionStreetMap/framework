namespace ActionStreetMap.Core.Geometry.Triangle.Geometry
{
    /// <summary> Represents a straight line segment in 2D space. </summary>
    internal struct Edge
    {
        /// <summary> Gets the first endpoints index. </summary>
        public ushort P0;

        /// <summary> Gets the second endpoints index. </summary>
        public ushort P1;

        /// <summary> Initializes a new instance of the <see cref="Edge" /> class. </summary>
        public Edge(ushort p0, ushort p1)
        {
            P0 = p0;
            P1 = p1;
        }
    }
}