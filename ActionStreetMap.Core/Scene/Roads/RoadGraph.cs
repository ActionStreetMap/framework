using System.Collections.Generic;

namespace ActionStreetMap.Core.Scene.Roads
{
    /// <summary> Represents road graph. </summary>
    public class RoadGraph
    {
        /// <summary> Return list of roads. </summary>
        public IEnumerable<Road> Roads { get ;private set; }

        /// <summary> Gets collection of detected junctions. </summary>
        public IEnumerable<RoadJunction> Junctions { get; private set; }

        /// <summary> Creates instace of <see cref="RoadGraph"/>. </summary>
        public RoadGraph(IEnumerable<Road> roads, IEnumerable<RoadJunction> junctions)
        {
            Roads = roads;
            Junctions = junctions;
        }
    }
}
