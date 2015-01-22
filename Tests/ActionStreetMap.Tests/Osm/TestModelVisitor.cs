using System;
using System.Collections.Generic;
using ActionStreetMap.Core.Tiling;
using ActionStreetMap.Core.Tiling.Models;

namespace ActionStreetMap.Tests.Osm
{
    public class TestModelVisitor: IModelVisitor
    {
        public List<Relation> Relations = new List<Relation>();
        public List<Area> Areas = new List<Area>();
        public List<Way> Ways = new List<Way>();
        public List<Node> Nodes = new List<Node>();
        public List<Canvas> Canvases = new List<Canvas>();

        public void VisitTile(Tile tile)
        {
            
        }

        public void VisitRelation(Relation relation)
        {
            lock (Relations)
            {
                Relations.Add(relation);
            }
        }

        public void VisitArea(Area area)
        {
            lock (Areas)
            {
                Areas.Add(area);
            }
        }

        public void VisitWay(Way way)
        {
            lock (Ways)
            {
                Ways.Add(way);
            }
        }

        public void VisitNode(Node node)
        {
            lock (Nodes)
            {
                Nodes.Add(node);
            }
        }

        public void VisitCanvas(Canvas canvas)
        {
            lock (Canvases)
            {
                Canvases.Add(canvas);
            }
        }
    }
}
