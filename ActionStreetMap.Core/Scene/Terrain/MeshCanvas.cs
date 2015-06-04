using System;
using System.Collections.Generic;
using ActionStreetMap.Core.Geometry.Triangle.Meshing;
using Paths = System.Collections.Generic.List<System.Collections.Generic.List<ActionStreetMap.Core.Geometry.Clipping.IntPoint>>;

namespace ActionStreetMap.Core.Scene.Terrain
{
    internal class MeshCanvas
    {
        public RenderMode RenderMode;
        public MapRectangle Rect;

        public Region Background;
        public Region Water;
        public List<Region> Surfaces;
        public Region CarRoads;
        public Region WalkRoads;

        internal class Region
        {
            public string GradientKey;
            public float ElevationNoiseFreq;
            public float ColorNoiseFreq;
            public Action<IMesh> ModifyMeshAction;

            public Paths Shape;
        }
    }
}
