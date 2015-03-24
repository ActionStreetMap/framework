using System.Collections.Generic;
using ActionStreetMap.Core.Geometry.Triangle.Geometry;
using Paths = System.Collections.Generic.List<System.Collections.Generic.List<ActionStreetMap.Core.Geometry.Clipping.IntPoint>>;

namespace ActionStreetMap.Core.Scene.Terrain
{
    internal class MeshCanvas
    {
        public Rectangle Rect;

        public Region Background;
        public Region Water;
        public List<Region> Surfaces;
        public Region CarRoads;
        public Region WalkRoads;

        internal class Region
        {
            public string GradientKey;
            public Paths Shape;
        }
    }
}
