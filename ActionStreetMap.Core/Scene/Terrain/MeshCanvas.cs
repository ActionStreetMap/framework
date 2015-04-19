using System;
using System.Collections.Generic;
using ActionStreetMap.Core.Geometry.Triangle;
using Paths = System.Collections.Generic.List<System.Collections.Generic.List<ActionStreetMap.Core.Geometry.Clipping.IntPoint>>;

namespace ActionStreetMap.Core.Scene.Terrain
{
    internal class MeshCanvas
    {
        public MapRectangle Rect;

        public Region Background;
        public Region Water;
        public List<Region> Surfaces;
        public Region CarRoads;
        public Region WalkRoads;

        internal class Region
        {
            public string GradientKey;
            public Action<Mesh> ModifyMeshAction;

            public Paths Shape;
        }
    }
}
