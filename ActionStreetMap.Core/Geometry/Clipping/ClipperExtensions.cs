using System.Collections.Generic;
using System.Linq;

namespace ActionStreetMap.Core.Geometry.Clipping
{
    /// <summary> Provides extension methods for improving performance by avoiding unnecessary memory traffic. </summary>
    /// <remarks> Logic should match original code. </remarks>
    internal static class ClipperExtensions
    {
        internal static bool AddPaths(this Clipper clipper, IEnumerable<List<IntPoint>> ppg, PolyType polyType,
            bool closed)
        {
            bool result = false;
            foreach (var item in ppg)
                if (clipper.AddPath(item, polyType, closed)) result = true;

            return result;
        }

        internal static bool AddPath(this Clipper clipper, List<MapPoint> pg, PolyType polyType, float scale,
            bool closed)
        {
            if (!closed)
                throw new ClipperException("AddPath: Open paths have been disabled.");

            int highI = pg.Count() - 1;
            if (closed) while (highI > 0 && (pg[highI] == pg[0])) --highI;
            while (highI > 0 && (pg[highI] == pg[highI - 1])) --highI;
            if ((closed && highI < 2) || (!closed && highI < 1)) return false;

            //create a new edge array ...
            List<TEdge> edges = new List<TEdge>(highI + 1);
            for (int i = 0; i <= highI; i++) edges.Add(ClipperPool.AllocEdge());

            bool IsFlat = true;

            //1. Basic (first) edge initialization ...
            var pg0 = new IntPoint(pg[0].X*scale, pg[0].Y*scale);
            var pgHighI = new IntPoint(pg[highI].X*scale, pg[highI].Y*scale);
            edges[1].Curr = new IntPoint(pg[1].X*scale, pg[1].Y*scale);
            clipper.RangeTest(pg0, ref clipper.m_UseFullRange);
            clipper.RangeTest(pgHighI, ref clipper.m_UseFullRange);
            clipper.InitEdge(edges[0], edges[1], edges[highI], pg0);
            clipper.InitEdge(edges[highI], edges[0], edges[highI - 1], pgHighI);
            for (int i = highI - 1; i >= 1; --i)
            {
                var pgI = new IntPoint(pg[i].X*scale, pg[i].Y*scale);
                clipper.RangeTest(pgI, ref clipper.m_UseFullRange);
                clipper.InitEdge(edges[i], edges[i + 1], edges[i - 1], pgI);
            }
            TEdge eStart = edges[0];

            //2. Remove duplicate vertices, and (when closed) collinear edges ...
            TEdge E = eStart, eLoopStop = eStart;
            for (;;)
            {
                //nb: allows matching start and end points when not Closed ...
                if (E.Curr == E.Next.Curr && (closed || E.Next != eStart))
                {
                    if (E == E.Next) break;
                    if (E == eStart) eStart = E.Next;
                    E = clipper.RemoveEdge(E);
                    eLoopStop = E;
                    continue;
                }
                if (E.Prev == E.Next)
                    break; //only two vertices
                if (closed &&
                    ClipperBase.SlopesEqual(E.Prev.Curr, E.Curr, E.Next.Curr, clipper.m_UseFullRange) &&
                    (!clipper.PreserveCollinear ||
                     !clipper.Pt2IsBetweenPt1AndPt3(E.Prev.Curr, E.Curr, E.Next.Curr)))
                {
                    //Collinear edges are allowed for open paths but in closed paths
                    //the default is to merge adjacent collinear edges into a single edge.
                    //However, if the PreserveCollinear property is enabled, only overlapping
                    //collinear edges (ie spikes) will be removed from closed paths.
                    if (E == eStart) eStart = E.Next;
                    E = clipper.RemoveEdge(E);
                    E = E.Prev;
                    eLoopStop = E;
                    continue;
                }
                E = E.Next;
                if ((E == eLoopStop) || (!closed && E.Next == eStart)) break;
            }

            if ((!closed && (E == E.Next)) || (closed && (E.Prev == E.Next)))
                return false;

            //3. Do second stage of edge initialization ...
            E = eStart;
            do
            {
                clipper.InitEdge2(E, polyType);
                E = E.Next;
                if (IsFlat && E.Curr.Y != eStart.Curr.Y) IsFlat = false;
            } while (E != eStart);

            //4. Finally, add edge bounds to LocalMinima list ...

            //Totally flat paths must be handled differently when adding them
            //to LocalMinima list to avoid endless loops etc ...
            if (IsFlat)
            {
                if (closed) return false;
            }

            clipper.m_edges.Add(edges);
            bool leftBoundIsForward;
            TEdge EMin = null;

            //workaround to avoid an endless loop in the while loop below when
            //open paths have matching start and end points ...
            if (E.Prev.Bot == E.Prev.Top) E = E.Next;

            for (;;)
            {
                E = clipper.FindNextLocMin(E);
                if (E == EMin) break;
                if (EMin == null) EMin = E;

                //E and E.Prev now share a local minima (left aligned if horizontal).
                //Compare their slopes to find which starts which bound ...
                LocalMinima locMin = new LocalMinima();
                locMin.Next = null;
                locMin.Y = E.Bot.Y;
                if (E.Dx < E.Prev.Dx)
                {
                    locMin.LeftBound = E.Prev;
                    locMin.RightBound = E;
                    leftBoundIsForward = false; //Q.nextInLML = Q.prev
                }
                else
                {
                    locMin.LeftBound = E;
                    locMin.RightBound = E.Prev;
                    leftBoundIsForward = true; //Q.nextInLML = Q.next
                }
                locMin.LeftBound.Side = EdgeSide.esLeft;
                locMin.RightBound.Side = EdgeSide.esRight;

                if (locMin.LeftBound.Next == locMin.RightBound)
                    locMin.LeftBound.WindDelta = -1;
                else locMin.LeftBound.WindDelta = 1;
                locMin.RightBound.WindDelta = -locMin.LeftBound.WindDelta;

                E = clipper.ProcessBound(locMin.LeftBound, leftBoundIsForward);
                if (E.OutIdx == ClipperBase.Skip) E = clipper.ProcessBound(E, leftBoundIsForward);

                TEdge E2 = clipper.ProcessBound(locMin.RightBound, !leftBoundIsForward);
                if (E2.OutIdx == ClipperBase.Skip) E2 = clipper.ProcessBound(E2, !leftBoundIsForward);

                if (locMin.LeftBound.OutIdx == ClipperBase.Skip)
                    locMin.LeftBound = null;
                else if (locMin.RightBound.OutIdx == ClipperBase.Skip)
                    locMin.RightBound = null;
                clipper.InsertLocalMinima(locMin);
                if (!leftBoundIsForward) E = E2;
            }
            return true;
        }

        internal static bool AddPaths(this Clipper clipper, IEnumerable<List<MapPoint>> ppg, PolyType polyType,
            float scale, bool closed)
        {
            bool result = false;
            foreach (var ppgItem in ppg)
                if (clipper.AddPath(ppgItem, polyType, scale, closed)) result = true;

            return result;
        }
    }
}