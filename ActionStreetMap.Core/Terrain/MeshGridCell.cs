using System.Collections.Generic;
using ActionStreetMap.Core.Polygons;
using ActionStreetMap.Core.Polygons.Geometry;

namespace ActionStreetMap.Core.Terrain
{
    internal class MeshGridCell
    {
        public Mesh Mesh;

        public List<Vertex> Water;
        public List<Vertex> Surfaces;
        public List<Vertex> Roads;
        public List<Vertex> Bridges;
    }
}
