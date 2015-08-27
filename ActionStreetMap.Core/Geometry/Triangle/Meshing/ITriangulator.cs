using System.Collections.Generic;
using ActionStreetMap.Core.Geometry.Triangle.Geometry;

namespace ActionStreetMap.Core.Geometry.Triangle.Meshing
{
    /// <summary> Interface for point set triangulation. </summary>
    internal interface ITriangulator
    {
        /// <summary> Triangulates a point set. </summary>
        Mesh Triangulate(List<Vertex> points);
    }
}