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

        /// <summary> Gets or sets zIndex. </summary>
        public float ZIndex { get; set; }

        /// <summary> Gets or sets splat index. </summary>
        public int SplatIndex { get; set; }

        /// <summary> Gets or sets detail index. </summary>
        public int DetailIndex { get; set; }

        /// <summary> Gets or sets average elevation. </summary>
        public float AverageElevation { get; set; }

        /// <summary> Gets or sets map points for this surcafe. </summary>
        public List<MapPoint> Points { get; set; }

        /// <summary> Gets or sets points for holes inside this surcafe. </summary>
        public List<List<MapPoint>> Holes { get; set; }
    }
}
