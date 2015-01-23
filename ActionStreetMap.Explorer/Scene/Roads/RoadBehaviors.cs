using ActionStreetMap.Core.Scene.Roads;
using UnityEngine;

namespace ActionStreetMap.Explorer.Scene.Roads
{
    /// <summary>
    ///     Represents road behavior which can be useful to tweak road after game starts.
    /// </summary>
    public class RoadBehaviors: MonoBehaviour
    {
        /// <summary>
        ///     Gets or sets Road.
        /// </summary>
        public Road Road { get; set; }
    }

    /// <summary>
    ///     Represents road junction behavior which can be useful to tweak road junction after game starts.
    /// </summary>
    public class JunctionBehavior : MonoBehaviour
    {
        /// <summary>
        ///     Gets or sets RoadJunction.
        /// </summary>
        public RoadJunction Junction { get; set; }
    }
}
