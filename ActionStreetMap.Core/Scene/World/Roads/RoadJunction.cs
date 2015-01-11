using System;
using System.Collections.Generic;
using ActionStreetMap.Core.Unity;

namespace ActionStreetMap.Core.Scene.World.Roads
{
    /// <summary>
    ///     Represents road junction.
    /// </summary>
    public class RoadJunction
    {
        /// <summary>
        ///     Gets or sets game object wrapper which holds game engine specific classes
        /// </summary>
        public IGameObject GameObject { get; set; }

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
            Connections = new List<Connection>(2);
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
            public RoadElement Element { get; private set; }

            /// <summary>
            ///     Creates instance of <see cref="Connection"/>.
            /// </summary>
            /// <param name="point">Connection point.</param>
            /// <param name="element">Road element.</param>
            public Connection(MapPoint point, RoadElement element)
            {
                Point = point;
                Element = element;
            }
        }

        #endregion

        /// <inheritdoc />
        public override string ToString()
        {
            return String.Format("{0} c={1}", Center, Connections.Count);
        }
    }
}
