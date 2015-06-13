
using System;

namespace ActionStreetMap.Core.Geometry.Triangle.Meshing
{
    using System.Collections.Generic;
    using ActionStreetMap.Core.Geometry.Triangle.Topology;
    using ActionStreetMap.Core.Geometry.Triangle.Geometry;

    /// <summary>
    /// Mesh interface.
    /// </summary>
    internal interface IMesh : IDisposable
    {
        /// <summary>
        /// Gets the vertices of the mesh.
        /// </summary>
        ICollection<Vertex> Vertices { get; }

        /// <summary>
        /// Gets the edges of the mesh.
        /// </summary>
        IEnumerable<Edge> Edges { get; }

        /// <summary>
        /// Gets the segments (constraint edges) of the mesh.
        /// </summary>
        ICollection<Segment> Segments { get; }

        /// <summary>
        /// Gets the triangles of the mesh.
        /// </summary>
        ICollection<Triangle> Triangles { get; }

        /// <summary>
        /// Gets the holes of the mesh.
        /// </summary>
        IList<Point> Holes { get; }

        /// <summary>
        /// Gets the bounds of the mesh.
        /// </summary>
        Rectangle Bounds { get; }

        /// <summary>
        /// Renumber mesh vertices and triangles.
        /// </summary>
        void Renumber();

        /// <summary>
        /// Refine the mesh.
        /// </summary>
        void Refine(QualityOptions quality);
    }
}
