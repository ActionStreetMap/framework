using System.Linq;
using System.Collections.Generic;

namespace ActionStreetMap.Core.Scene.World.Roads
{
    /// <summary>
    ///     Represents certain part of road.
    /// </summary>
    public class RoadElement
    {
        /// <summary>
        ///     Gets or sets original road element id.
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        ///     Gets or sets associated address.
        /// </summary>
        public Address Address { get; set; }

        /// <summary>
        ///     Gets or sets lane count.
        /// </summary>
        public int Lanes { get; set; }

        /// <summary>
        ///     Gets or sets road width.
        /// </summary>
        public float Width { get; set; }

        /// <summary>
        ///     True if this road element isn't connected with previous one.
        /// </summary>
        public bool IsNotContinuation { get; set; }

        /// <summary>
        ///     Gets or sets actual type of road element.
        /// </summary>
        public RoadType Type { get; set; }

        /// <summary>
        ///     Gets or sets middle points of road.
        /// </summary>
        public List<MapPoint> Points { get; set; }

        /// <summary>
        ///     Gets or sets height on terrain.
        /// </summary>
        public float ZIndex { get; set; }

        /// <summary>
        ///     Gets junction at start.
        /// </summary>
        public RoadJunction Start { get; set; }

        /// <summary>
        ///     Gets junction at end.
        /// </summary>
        public RoadJunction End { get; set; }

        /// <inheritdoc />
        public override string ToString()
        {
            return string.Format("{0}:[{1}..{2}]", Id, Points.First(), Points.Last());
        }
    }
}