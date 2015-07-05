using System;
using ActionStreetMap.Core.Geometry.Triangle.Geometry;

namespace ActionStreetMap.Core.Geometry.Triangle.Topology
{
    /// <summary> The triangle data structure. </summary>
    public class Triangle
    {
        // The ID is only used for mesh output.
        internal int Id;

        internal Otri[] neighbors;
        internal Vertex[] vertices;
        internal Osub[] subsegs;

        internal double area;
        internal bool infected;

        /// <summary> Initializes a new instance of the <see cref="Triangle" /> class. </summary>
        public Triangle()
        {
            // Three NULL vertices.
            vertices = new Vertex[3];

            // Initialize the three adjoining subsegments to be the omnipresent subsegment.
            subsegs = new Osub[3];

            // Initialize the three adjoining triangles to be "outer space".
            neighbors = new Otri[3];
        }

        #region Public properties

        /// <summary> Gets the first corners vertex id. </summary>
        public int P0 { get { return vertices[0] == null ? -1 : vertices[0].Id; } }

        /// <summary> Gets the seconds corners vertex id. </summary>
        public int P1 { get { return vertices[1] == null ? -1 : vertices[1].Id; } }

        /// <summary> Gets the third corners vertex id. </summary>
        public int P2 { get { return vertices[2] == null ? -1 : vertices[2].Id; } }

        /// <summary> Gets the specified corners vertex. </summary>
        public Vertex GetVertex(int index) { return vertices[index]; }

        public bool SupportsNeighbors { get { return true; } }

        /// <summary> Gets the first neighbors id. </summary>
        public int N0 { get { return neighbors[0].tri.Id; } }

        /// <summary> Gets the second neighbors id. </summary>
        public int N1 { get { return neighbors[1].tri.Id; } }

        /// <summary> Gets the third neighbors id. </summary>
        public int N2 { get { return neighbors[2].tri.Id; } }

        /// <summary> Gets a triangles' neighbor. </summary>
        /// <param name="index">The neighbor index (0, 1 or 2).</param>
        /// <returns>The neigbbor opposite of vertex with given index.</returns>
        public Triangle GetNeighbor(int index)
        {
            return neighbors[index].tri.Id == Mesh.DUMMY ? null : neighbors[index].tri;
        }

        /// <summary> Gets a triangles segment. </summary>
        /// <param name="index">The vertex index (0, 1 or 2).</param>
        /// <returns>The segment opposite of vertex with given index.</returns>
        public Segment GetSegment(int index)
        {
            return subsegs[index].seg.hash == Mesh.DUMMY ? null : subsegs[index].seg;
        }

        /// <summary> Gets the triangle area constraint. </summary>
        public double Area
        {
            get { return area; }
            set { area = value; }
        }

        #endregion

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return Id;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return String.Format("TID {0}", Id);
        }

        internal void Reset()
        {
            Id = 0;

            area = 0;
            infected = false;

            neighbors[0] = default(Otri);
            neighbors[1] = default(Otri);
            neighbors[2] = default(Otri);

            vertices[0] = null;
            vertices[1] = null;
            vertices[2] = null;

            subsegs[0] = default(Osub);
            subsegs[1] = default(Osub);
            subsegs[2] = default(Osub);
        }
    }
}