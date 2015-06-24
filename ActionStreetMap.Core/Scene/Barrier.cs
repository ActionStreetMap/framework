using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ActionStreetMap.Core.Scene
{
    /// <summary> Represents barrier. </summary>
    public class Barrier
    {
        /// <summary> Gets or sets Id. </summary>
        public long Id;

        /// <summary> Gets or sets barrier height. </summary>
        public float Height;

        /// <summary> Gets or sets building footprint. </summary>
        public List<MapPoint> Footprint;
    }
}
