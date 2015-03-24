using System.Collections.Generic;
using System.Linq;

namespace ActionStreetMap.Core.Scene
{
    /// <summary> Represents certain part of road. </summary>
    public class RoadElement
    {
        /// <summary> Gets or sets original road element id. </summary>
        public long Id { get; set; }

        /// <summary> Gets or sets associated address. </summary>
        public Address Address { get; set; }

        /// <summary> Gets or sets lane count. </summary>
        public int Lanes { get; set; }

        /// <summary> Gets or sets road width. </summary>
        public float Width { get; set; }

        /// <summary> Gets or sets actual type of road element. </summary>
        public RoadType Type { get; set; }

        /// <summary> Gets or sets middle points of road. </summary>
        public List<MapPoint> Points { get; set; }

        /// <inheritdoc />
        public override string ToString()
        {
            return string.Format("{0}:[{1}..{2}]", Id, Points.First(), Points.Last());
        }

        /// <summary> Represents general road type. </summary>
        public enum RoadType : byte
        {
            /// <summary> Road for cars. </summary>
            Car,
            /// <summary> Road for bikes. </summary>
            Bike,
            /// <summary> Road for pedestrians. </summary>
            Pedestrian
        }
    }
}