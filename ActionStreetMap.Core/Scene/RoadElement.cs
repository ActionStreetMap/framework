using System.Collections.Generic;
using System.Linq;
using ActionStreetMap.Core.Geometry;

namespace ActionStreetMap.Core.Scene
{
    /// <summary> Represents certain part of road. </summary>
    public class RoadElement
    {
        /// <summary> Original road element id. </summary>
        public long Id;

        /// <summary> Lane count. </summary>
        public int Lanes;

        /// <summary> Road width. </summary>
        public float Width;

        /// <summary> Actual type of road element. </summary>
        public RoadType Type;

        /// <summary> Middle points of road. </summary>
        public List<Vector2d> Points;

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