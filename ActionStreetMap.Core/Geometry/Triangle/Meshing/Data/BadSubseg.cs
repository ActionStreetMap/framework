using System;
using ActionStreetMap.Core.Geometry.Triangle.Geometry;
using ActionStreetMap.Core.Geometry.Triangle.Topology;

namespace ActionStreetMap.Core.Geometry.Triangle.Meshing.Data
{
    /// <summary> A queue used to store encroached subsegments. </summary>
    /// <remarks>
    ///     Each subsegment's vertices are stored so that we can check whether a
    ///     subsegment is still the same.
    /// </remarks>
    internal class BadSubseg
    {
        public Osub subseg; // An encroached subsegment.
        public Vertex org, dest; // Its two vertices.

        public override int GetHashCode()
        {
            return subseg.seg.hash;
        }

        public override string ToString()
        {
            return String.Format("B-SID {0}", subseg.seg.hash);
        }
    }
}