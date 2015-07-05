using System;
using ActionStreetMap.Core.Geometry.Triangle.Geometry;
using ActionStreetMap.Core.Geometry.Triangle.Topology;

namespace ActionStreetMap.Core.Geometry.Triangle.Meshing.Data
{
    /// <summary> A queue used to store bad triangles. </summary>
    /// <remarks>
    ///     The key is the square of the cosine of the smallest angle of the triangle.
    ///     Each triangle's vertices are stored so that one can check whether a
    ///     triangle is still the same.
    /// </remarks>
    internal class BadTriangle
    {
        public Otri poortri; // A skinny or too-large triangle.
        public double key; // cos^2 of smallest (apical) angle.
        public Vertex org, dest, apex; // Its three vertices.
        public BadTriangle next; // Pointer to next bad triangle.

        internal void Reset()
        {
            poortri = default(Otri);
            key = 0;

            org = null;
            dest = null;
            apex = null;
            next = null;
        }

        public override string ToString()
        {
            return String.Format("B-TID {0}", poortri.tri.Id);
        }
    }
}