using System;
using System.Collections.Generic;
using ActionStreetMap.Core;
using ActionStreetMap.Infrastructure.Utilities;

namespace ActionStreetMap.Explorer.Scene.Geometry.Polygons
{
    /// <summary>
    ///     Implements simple scan-line algorithm. Code ported from existing java code found in Internet.
    /// </summary>
    public sealed class ScanLine
    {
        /// <summary>
        ///     Fills polygon using points.
        /// </summary>
        /// <param name="points">Polygon points.</param>
        /// <param name="fillAction">Fille action.</param>
        /// <param name="objectPool">Object pool.</param>
        public static void FillPolygon(List<MapPoint> points, Action<int, int, int> fillAction, IObjectPool objectPool)
        {
            var edges = objectPool.NewList<Edge>(16);
            // Holds all cutpoints from current scanline with the polygon
            var list = objectPool.NewList<int>(32); 
            // create edges array from polygon vertice vector
            // make sure that first vertice of an edge is the smaller one
            CreateEdges(points, edges);

            // sort all edges by Y coordinate, smallest one first, lousy bubblesort
            Edge tmp;

            for (int i = 0; i < edges.Count - 1; i++)
                for (int j = 0; j < edges.Count - 1; j++)
                {
                    if (edges[j].StartY > edges[j + 1].StartY)
                    {
                        // swap both edges
                        tmp = edges[j];
                        edges[j] = edges[j + 1];
                        edges[j + 1] = tmp;
                    }
                }

            // find biggest Y-coord of all vertices
            int scanlineEnd = 0;
            for (int i = 0; i < edges.Count; i++)
            {
                if (scanlineEnd < edges[i].EndY)
                    scanlineEnd = edges[i].EndY;
            }          

            // scanline starts at smallest Y coordinate
            // move scanline step by step down to biggest one
            for (int scanline = edges[0].StartY; scanline <= scanlineEnd; scanline++)
            {
                list.Clear();

                // loop all edges to see which are cut by the scanline
                for (int i = 0; i < edges.Count; i++)
                {
                    // here the scanline intersects the smaller vertice
                    if (scanline == edges[i].StartY)
                    {
                        if (scanline == edges[i].EndY)
                        {
                            // the current edge is horizontal, so we add both vertices
                            edges[i].Deactivate();
                            list.Add((int)edges[i].CurrentX);
                        }
                        else
                        {
                            edges[i].Activate();
                            // we don't insert it in the _reusableBuffer cause this vertice is also
                            // the (bigger) vertice of another edge and already handled
                        }
                    }

                    // here the scanline intersects the bigger vertice
                    if (scanline == edges[i].EndY)
                    {
                        edges[i].Deactivate();
                        list.Add((int)edges[i].CurrentX);
                    }

                    // here the scanline intersects the edge, so calc intersection point
                    if (scanline > edges[i].StartY && scanline < edges[i].EndY)
                    {
                        edges[i].Update();
                        list.Add((int)edges[i].CurrentX);
                    }
                }

                // now we have to sort our _reusableBuffer with our X-coordinates, ascendend
                for (int i = 0; i < list.Count; i++)
                    for (int j = 0; j < list.Count - 1; j++)
                    {
                        if (list[j] > list[j + 1])
                        {
                            int swaptmp = list[j];
                            list[j] = list[j + 1];
                            list[j + 1] = swaptmp;
                        }
                    }

                if (list.Count < 2 || list.Count % 2 != 0)
                    continue;

                // so fill all line segments on current scanline
                for (int i = 0; i < list.Count; i += 2)
                    fillAction(scanline, list[i], list[i + 1]);
            }

            objectPool.StoreList(edges);
            objectPool.StoreList(list);
        }

        /// <summary>
        ///     Create from the polygon vertices an array of edges.
        ///     Note that the first vertice of an edge is always the one with the smaller Y coordinate one of both
        /// </summary>
        private static void CreateEdges(List<MapPoint> polygon, List<Edge> edges)
        {
            for (int i = 0; i < polygon.Count; i++)
            {
                var nextIndex = i == polygon.Count - 1 ? 0 : i + 1;
                edges.Add(polygon[i].Y < polygon[nextIndex].Y
                    ? new Edge(polygon[i], polygon[nextIndex])
                    : new Edge(polygon[nextIndex], polygon[i]));
            }
        }

        #region Helper types

        /// <summary>
        ///     Represents an edge of polygon.
        /// </summary>
        private class Edge
        {
            /// <summary>
            ///     Start vertice.
            /// </summary>
            public int StartX;
            public int StartY;

            /// <summary>
            ///     End vertice
            /// </summary>
            public int EndX;
            public int EndY;

            /// <summary>
            ///     Slope
            /// </summary>
            public readonly float M;

            /// <summary>
            ///     X-coord of intersection with scanline
            /// </summary>
            public float CurrentX;

            public Edge(MapPoint a, MapPoint b)
            {
                StartX = (int) Math.Round(a.X);
                StartY = (int) Math.Round(a.Y);

                EndX = (int)Math.Round(b.X);
                EndY = (int)Math.Round(b.Y);

                // M = dy / dx
                M = (StartY - EndY) / (float)(StartX - EndX);
            }

            /// <summary>
            ///     Called when scanline intersects the first vertice of this edge.
            ///     That simply means that the intersection point is this vertice.
            /// </summary>
            public void Activate()
            {
                CurrentX = StartX;
            }

            /// <summary>
            ///     Update the intersection point from the scanline and this edge.
            ///     Instead of explicitly calculate it we just increment with 1/M every time
            ///     it is intersected by the scanline.
            /// </summary>
            public void Update()
            {
                CurrentX += 1 / M;
            }


            /// <summary>
            ///     Called when scanline intersects the second vertice,
            ///     so the intersection point is exactly this vertice and from now on
            ///     we are done with this edge
            /// </summary>
            public void Deactivate()
            {
                CurrentX = EndX;
            }
        }

        #endregion
    }
}