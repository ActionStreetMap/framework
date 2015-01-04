using System.Collections.Generic;

namespace ActionStreetMap.Core.Scene.World.Roads
{
    /// <summary>
    ///     Represents road junction.
    /// </summary>
    public class RoadJunction
    {
        /// <summary>
        ///     Gets junction center point.
        /// </summary>
        public MapPoint Center { get; private set; }

        /// <summary>
        ///     Gets junction connections.
        /// </summary>
        public List<Connection> Connections { get; private set; }

        /// <summary>
        ///     Creates the instance of <see cref="RoadJunction"/>
        /// </summary>
        /// <param name="center">Junction center point.</param>
        public RoadJunction(MapPoint center)
        {
            Center = center;
            Connections = new List<Connection>(4);
        }

        #region Nested classes

        /// <summary>
        ///     Represents road connection which belongs to junction.
        /// </summary>
        public class Connection
        {
            /// <summary>
            ///     Gets connection point.
            /// </summary>
            public MapPoint Point { get; private set; }

            /// <summary>
            ///     Gets road type.
            /// </summary>
            public string Type { get; private set; }

            /// <summary>
            ///     Gets road width.
            /// </summary>
            public float Width { get; private set; }

            /// <summary>
            ///     Creates instance of <see cref="Connection"/>.
            /// </summary>
            /// <param name="point">Connection point.</param>
            /// <param name="type">Road type.</param>
            /// <param name="width">Road width.</param>
            public Connection(MapPoint point, string type, float width)
            {
                Point = point;
                Type = type;
                Width = width;
            }
        }

        #endregion
    }
}
