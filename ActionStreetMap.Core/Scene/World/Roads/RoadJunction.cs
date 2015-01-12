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
        ///     Gest or sets polygon points
        /// </summary>
        public List<MapPoint> Polygon { get; internal set; }

        /// <summary>
        ///     Gets junction center point.
        /// </summary>
        public MapPoint Center { get; private set; }

        /// <summary>
        ///     Gets junction connections.
        /// </summary>
        public List<RoadElement> Connections { get; private set; }

        /// <summary>
        ///     Creates the instance of <see cref="RoadJunction"/>
        /// </summary>
        /// <param name="center">Junction center point.</param>
        public RoadJunction(MapPoint center)
        {
            Center = center;
            Connections = new List<RoadElement>(2);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return String.Format("{0} c={1}", Center, Connections.Count);
        }
    }
}
