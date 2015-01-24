using System;
using System.Collections.Generic;

namespace ActionStreetMap.Core.Scene.Details
{
    /// <summary> Represents surface. </summary>
    public class Surface
    {
        /// <summary> Default detail index. </summary>
        public const int DefaultDetailIndex = -1;

        /// <summary> Creates instance of <see cref="Surface"/>. </summary>
        public Surface()
        {
            DetailIndex = DefaultDetailIndex;
        }

        /// <summary> ZIndex. </summary>
        public float ZIndex { get; set; }

        /// <summary> Splat index. </summary>
        public int SplatIndex { get; set; }

        /// <summary> Detail index. </summary>
        public int DetailIndex { get; set; }

        /// <summary> Average elevation. </summary>
        public float AverageElevation { get; set; }

        /// <summary> Gets or sets map points for this surcafe. </summary>
        public List<MapPoint> Points { get; set; }

        /// <summary> Gets or sets points for holes inside this surcafe. </summary>
        public List<List<MapPoint>> Holes { get; set; }
    }
}
