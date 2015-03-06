// ----------------------------------------------------------------------- 
// <copyright file="ITriangulator.cs" company="">
//     Triangle.NET code by Christian Woltering, http://triangle.codeplex.com/
// </copyright>
// ----------------------------------------------------------------------- 

using System.Collections.Generic;
using ActionStreetMap.Core.Polygons.Geometry;

namespace ActionStreetMap.Core.Polygons.Meshing
{
    /// <summary> Interface for point set triangulation. </summary>
    public interface ITriangulator
    {
        /// <summary> Triangulates a point set. </summary>
        /// <param name="points"> Collection of points. </param>
        /// <returns> Mesh </returns>
        Mesh Triangulate(ICollection<Vertex> points);
    }
}