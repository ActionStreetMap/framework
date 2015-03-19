using System.Collections.Generic;
using ActionStreetMap.Core;
using ActionStreetMap.Core.MapCss.Domain;
using ActionStreetMap.Core.Polygons;
using ActionStreetMap.Core.Polygons.Meshing.Iterators;
using ActionStreetMap.Core.Polygons.Tools;
using ActionStreetMap.Core.Unity;

namespace ActionStreetMap.Explorer.Terrain
{
    public class MeshContext
    {
        public Rule Rule;
        public IGameObject Object;

        public MapRectangle Rectangle;
        public Mesh Mesh;
        public QuadTree Tree;
        public RegionIterator Iterator;

        public Dictionary<int, int> TriangleMap;

        public List<UnityEngine.Vector3> Vertices;
        public List<int> Triangles;
        public List<UnityEngine.Color> Colors;
    }
}