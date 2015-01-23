using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ActionStreetMap.Core.Tiling.Models;

namespace ActionStreetMap.Core.Scene.Details
{
    public class Surface
    {
        public const int DefaultDetailIndex = -1;

        public Surface()
        {
            DetailIndex = DefaultDetailIndex;
        }

        public float ZIndex { get; set; }
        public int SplatIndex { get; set; }
        public int DetailIndex { get; set; }

        public float AverageElevation { get; set; }

        /// <summary>
        ///     Gets or sets map points for this surcafe.
        /// </summary>
        public List<MapPoint> Points { get; set; }

        /// <summary>
        ///     Gets or sets points for holes inside this surcafe.
        /// </summary>
        public List<List<MapPoint>> Holes { get; set; }
    }
}
