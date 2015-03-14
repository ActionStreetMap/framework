using System.Collections.Generic;
using ActionStreetMap.Core.Polygons.Geometry;
using VertexPaths = System.Collections.Generic.List<System.Collections.Generic.List<ActionStreetMap.Core.Polygons.Geometry.Vertex>>;

namespace ActionStreetMap.Core.Terrain
{
    internal class MeshRegion
    {
        public VertexPaths Contours;
        public VertexPaths Holes;

        public List<MeshFillRegion> FillRegions;
    }

    internal class MeshFillRegion
    {
        public int SplatId;
        public Vertex Anchor;
    }
}
