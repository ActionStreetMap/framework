using System;
using ActionStreetMap.Core.Geometry.Triangle.Geometry;

namespace ActionStreetMap.Core.Geometry.Triangle.Topology
{
    /// <summary> The subsegment data structure. </summary>
    internal class Segment
    {
        // Hash for dictionary. Will be set by mesh instance.
        internal int hash;

        internal Osub[] subsegs;
        internal Vertex[] vertices;
        internal Otri[] triangles;
        internal ushort boundary;

        /// <summary> Initializes a new instance of the <see cref="Segment" /> class. </summary>
        public Segment()
        {
            // Four NULL vertices.
            vertices = new Vertex[4];

            // Set the boundary marker to zero.
            boundary = 0;

            // Initialize the two adjoining subsegments to be the omnipresent
            // subsegment.
            subsegs = new Osub[2];

            // Initialize the two adjoining triangles to be "outer space."
            triangles = new Otri[2];
        }

        #region Public properties

        /// <summary> Gets the first endpoints vertex id. </summary>
        public int P0 { get { return vertices[0].Id; } }

        /// <summary> Gets the seconds endpoints vertex id. </summary>
        public int P1 { get { return vertices[1].Id; } }

        /// <summary> Gets the segment boundary mark. </summary>
        public int Boundary { get { return boundary; } }

        #endregion

        /// <summary> Gets the segments endpoint. </summary>
        public Vertex GetVertex(int index)
        {
            return vertices[index]; // TODO: Check range?
        }

        /// <summary> Gets an adjoining triangle. </summary>
        public Triangle GetTriangle(int index)
        {
            return triangles[index].tri.Id == Mesh.DUMMY ? null : triangles[index].tri;
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return hash;
        }

        internal void Reset()
        {
            subsegs[0] = default(Osub);
            subsegs[1] = default(Osub); ;

            triangles[0] = default(Otri);
            triangles[1] = default(Otri);

            vertices[0] = null;
            vertices[1] = null;
            vertices[2] = null;
            vertices[3] = null;

            hash = 0;
            boundary = 0;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return String.Format("SID {0}", hash);
        }
    }
}