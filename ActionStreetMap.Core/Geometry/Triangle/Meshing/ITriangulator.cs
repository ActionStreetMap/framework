// -----------------------------------------------------------------------
// <copyright file="ITriangulator.cs" company="">
// Triangle.NET code by Christian Woltering, http://triangle.codeplex.com/
// </copyright>
// -----------------------------------------------------------------------

namespace ActionStreetMap.Core.Geometry.Triangle.Meshing
{
    using System.Collections.Generic;
    using ActionStreetMap.Core.Geometry.Triangle.Geometry;

    /// <summary>
    /// Interface for point set triangulation.
    /// </summary>
    public interface ITriangulator
    {
        /// <summary>
        /// Triangulates a point set.
        /// </summary>
        /// <param name="points">Collection of points.</param>
        /// <returns>Mesh</returns>
        IMesh Triangulate(ICollection<Vertex> points);
    }
}
