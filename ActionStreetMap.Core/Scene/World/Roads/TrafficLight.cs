using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ActionStreetMap.Core.Unity;

namespace ActionStreetMap.Core.Scene.World.Roads
{
    /// <summary>
    ///     Represents traffic light
    /// </summary>
    public class TrafficLight
    {
        /// <summary>
        ///     Gets or sets game object wrapper which holds game engine specific classes.
        /// </summary>
        public IGameObject GameObject { get; set; }

        /// <summary>
        ///     Gets traffic light position.
        /// </summary>
        public MapPoint Position { get; private set; }

        /// <summary>
        ///     Gets or sets traffic light state.
        /// </summary>
        public State CurrentState { get; set; }

        /// <summary>
        ///     Creates instance of <see cref="TrafficLight"/>.
        /// </summary>
        /// <param name="position">Position.</param>
        public TrafficLight(MapPoint position)
        {
            Position = position;
        }

        /// <summary>
        ///     Represents traffic light state.
        /// </summary>
        public enum State
        {
            Undefined,
            Green,
            Yellow,
            Red
        }
    }
}
