namespace ActionStreetMap.Core.Geometry.Triangle.Geometry
{
    /// <summary> Represents a straight line segment in 2D space. </summary>
    public class Edge
    {
        /// <summary> Gets the first endpoints index. </summary>
        public int P0 { get; private set; }

        /// <summary> Gets the second endpoints index. </summary>
        public int P1 { get; private set; }

        /// <summary> Initializes a new instance of the <see cref="Edge" /> class. </summary>
        public Edge(int p0, int p1)
        {
            P0 = p0;
            P1 = p1;
        }
    }
}