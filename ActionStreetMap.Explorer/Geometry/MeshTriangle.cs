using ActionStreetMap.Core;
using UnityEngine;

namespace ActionStreetMap.Explorer.Geometry
{
    public class MeshTriangle
    {
        public MapPoint Vertex0;
        public MapPoint Vertex1;
        public MapPoint Vertex2;

        public Color Color0;
        public Color Color1;
        public Color Color2;

        internal int Region;
    }
}
