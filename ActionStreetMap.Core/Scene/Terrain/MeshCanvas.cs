using System;
using System.Collections.Generic;
using ActionStreetMap.Core.Geometry;
using ActionStreetMap.Core.Geometry.Triangle;
using Paths = System.Collections.Generic.List<System.Collections.Generic.List<ActionStreetMap.Core.Geometry.Clipping.IntPoint>>;

namespace ActionStreetMap.Core.Scene.Terrain
{
    internal class MeshCanvas
    {
        public RenderMode RenderMode;
        public Rectangle2d Rect;

        public Region Background;
        public Region Water;
        public List<Region> Surfaces;
        public Region CarRoads;
        public Region WalkRoads;

        internal class Region
        {
            public string GradientKey;
            public string TextureAtlas;
            public string TextureKey;
            public float ElevationNoiseFreq;
            public float ColorNoiseFreq;
            public Action<Mesh> ModifyMeshAction;

            public Paths Shape;
        }
    }
}
