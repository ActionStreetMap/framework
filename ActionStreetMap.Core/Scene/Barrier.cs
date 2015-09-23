using System.Collections.Generic;
using ActionStreetMap.Core.Geometry;

namespace ActionStreetMap.Core.Scene
{
    /// <summary> Represents barrier. </summary>
    public class Barrier
    {
        /// <summary> Id. </summary>
        public long Id;

        /// <summary> Barrier height. </summary>
        public float Height;

        /// <summary> Building footprint. </summary>
        public List<Vector2d> Footprint;
    }
}
