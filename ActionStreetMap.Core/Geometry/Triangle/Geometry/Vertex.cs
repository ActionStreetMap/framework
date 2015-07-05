
using ActionStreetMap.Core.Geometry.Triangle.Topology;

namespace ActionStreetMap.Core.Geometry.Triangle.Geometry
{
    using System;
    
    /// <summary> The vertex data structure. </summary>
    public class Vertex : Point
    {
        internal VertexType type;
        internal Otri tri;

        /// <summary> Initializes a new instance of the <see cref="Vertex" /> class. </summary>
        public Vertex(): this(0, 0, 0)
        {
        }

        /// <summary> Initializes a new instance of the <see cref="Vertex" /> class. </summary>
        /// <param name="x">The x coordinate of the vertex.</param>
        /// <param name="y">The y coordinate of the vertex.</param>
        public Vertex(double x, double y): this(x, y, 0)
        {
        }

        /// <summary> Initializes a new instance of the <see cref="Vertex" /> class. </summary>
        /// <param name="x">The x coordinate of the vertex.</param>
        /// <param name="y">The y coordinate of the vertex.</param>
        /// <param name="mark">The boundary mark.</param>
        public Vertex(double x, double y, ushort mark)
            : base(x, y, mark)
        {
            this.type = VertexType.InputVertex;
        }

        #region Public properties

        /// <summary> Gets the vertex type. </summary>
        public VertexType Type { get { return this.type; } }

        /// <summary> Gets the specified coordinate of the vertex. </summary>
        /// <param name="i">Coordinate index.</param>
        /// <returns>X coordinate, if index is 0, Y coordinate, if index is 1.</returns>
        public double this[int i]
        {
            get
            {
                if (i == 0) return X;
                if (i == 1) return Y;
                throw new ArgumentOutOfRangeException("Index must be 0 or 1.");
            }
        }

        #endregion
    }
}
