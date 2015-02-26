using System;

namespace ActionStreetMap.Core.Elevation
{
    /// <summary>
    ///     Represents heightmap - object which provides information about elevation in given point.
    /// </summary>
    public class HeightMap: IDisposable
    {
        /// <summary> Gets or sets heightmap resolution. </summary>
        public int Resolution { get; set; }

        /// <summary> Gets or sets point of left bottom corner. </summary>
        public MapPoint LeftBottomCorner { get; set; }

        /// <summary> Gets or sets point of right upper corner. </summary>
        public MapPoint RightUpperCorner { get; set; }

        /// <summary> Gets or sets min elevation of given heightmap. </summary>
        public float MinElevation { get; set; }

        /// <summary> Gets or sets max elevation of given heightmap. </summary>
        public float MaxElevation { get; set; }

        /// <summary> True if heightmap is flat. </summary>
        public bool IsFlat { get; set; }

        /// <summary> Gets or sets actual heightmap data. </summary>
        public float[,] Data { get; set; }

        /// <summary> Gets or sets x axis offset (performance optimization). </summary>
        internal float XAxisRatio { get; set; }

        /// <summary> Gets or sets y axis offset  (performance optimization). </summary>
        internal float YAxisRatio { get; set; }

        /// <summary> Returns corresponding height for given point. </summary>
        public virtual float LookupHeight(MapPoint mapPoint)
        {
            var i = (int)Math.Round((mapPoint.X - LeftBottomCorner.X) / XAxisRatio);
            var j = (int)Math.Round((mapPoint.Y - LeftBottomCorner.Y) / YAxisRatio);

            // check out of range
            var bound = Resolution - 1;
            j = j > bound ? bound : j;
            i = i > bound ? bound : i;

            j = j < 0 ? 0 : j;
            i = i < 0 ? 0 : i;

            return Data[j, i];
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
        }

        /// <inheritdoc />
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Data = null;
            }
        }
    }
}
