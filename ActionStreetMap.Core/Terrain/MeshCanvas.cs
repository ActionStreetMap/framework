using System.Collections.Generic;
using ActionStreetMap.Core.Polygons.Geometry;
using Paths = System.Collections.Generic.List<System.Collections.Generic.List<ActionStreetMap.Core.Polygons.IntPoint>>;

namespace ActionStreetMap.Core.Terrain
{
    internal class MeshCanvas
    {
        public Rectangle Rect;

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
