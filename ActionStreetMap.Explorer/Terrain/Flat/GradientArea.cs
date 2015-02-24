using System.Collections.Generic;
using ActionStreetMap.Core;
using UnityEngine;

namespace ActionStreetMap.Explorer.Terrain.Flat
{
    public class GradientArea
    {
        public Gradient Gradient { get; set; }
        public List<MapPoint> Points { get; set; }
    }
}