using System.Collections.Generic;
using ActionStreetMap.Core.Unity;

namespace ActionStreetMap.Core.Scene.Roads
{
    /// <summary> Represents a road including game object. </summary>
    public class Road
    {
        /// <summary> Gets or sets list of road elements. </summary>
        public List<RoadElement> Elements { get; set; }
    }
}