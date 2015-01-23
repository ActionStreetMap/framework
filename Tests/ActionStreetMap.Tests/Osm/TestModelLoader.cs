using System;
using System.Collections.Generic;
using ActionStreetMap.Core.Tiling;
using ActionStreetMap.Core.Tiling.Models;

namespace ActionStreetMap.Tests.Osm
{
    public class TestModelLoader: IModelLoader
    {
        public List<Relation> Relations = new List<Relation>();
        public List<Area> Areas = new List<Area>();
        public List<Way> Ways = new List<Way>();
        public List<Node> Nodes = new List<Node>();
        public List<Canvas> Canvases = new List<Canvas>();

        public void PrepareTile(Tile tile)
        {
            
        }

        public void LoadRelation(Tile tile, Relation relation)
        {
            lock (Relations)
            {
                Relations.Add(relation);
            }
        }

        public void LoadArea(Tile tile, Area area)
        {
            lock (Areas)
            {
                Areas.Add(area);
            }
        }

        public void LoadWay(Tile tile, Way way)
        {
            lock (Ways)
            {
                Ways.Add(way);
            }
        }

        public void LoadNode(Tile tile, Node node)
        {
            lock (Nodes)
            {
                Nodes.Add(node);
            }
        }

        public void CompleteTile(Tile tile)
        {
            lock (Canvases)
            {
                Canvases.Add(tile.Canvas);
            }
        }
    }
}
