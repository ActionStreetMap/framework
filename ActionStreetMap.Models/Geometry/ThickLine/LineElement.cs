using System.Collections.Generic;
using ActionStreetMap.Core;

namespace ActionStreetMap.Models.Geometry.ThickLine
{
    /// <summary>
    ///     Represents line element.
    /// </summary>
    public class LineElement
    {
        /// <summary>
        ///     Gets line element width.
        /// </summary>
        public float Width { get; private set; }

        /// <summary>
        ///     Gets or sets line points.
        /// </summary>
        public List<MapPoint> Points { get; set; }

        /// <summary>
        ///     Creates LineElement.
        /// </summary>
        /// <param name="points">Line element points.</param>
        /// <param name="width">Line element width.</param>
        public LineElement(List<MapPoint> points, float width)
        {
            Points = points;
            Width = width;
        }
    }
}
