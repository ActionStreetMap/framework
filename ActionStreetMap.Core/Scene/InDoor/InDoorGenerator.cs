using System;
using System.Collections.Generic;
using System.Linq;
using ActionStreetMap.Core.Geometry;
using ActionStreetMap.Core.Geometry.Clipping;
using ActionStreetMap.Core.Geometry.Utils;

namespace ActionStreetMap.Core.Scene.InDoor
{
    internal sealed class InDoorGenerator
    {
        private const int InvalidIndex = -1;
        private const int Scale = 1000;
        private const int DoubleScale = Scale * Scale;
        private const int IntPrecisionError = 10;

        private const int MinArea = 200 * DoubleScale;

        public Floor Build(InDoorGeneratorSettings settings)
        {
            var intFootPrint = settings.ObjectPool.NewList<IntPoint>(settings.Footprint.Count);
            for (int i = 0; i < settings.Footprint.Count; i++)
            {
                var point = settings.Footprint[i];
                intFootPrint.Add(new IntPoint(point.X * Scale, point.Y * Scale));
            }

            // return null if footprint area is too small
            if (Clipper.Area(intFootPrint) < MinArea)
                return null;

            var floor = new Floor(settings.ObjectPool);

            var walls = CreateWalls(settings);

            InsertEntrances(settings, floor, walls);

            List<List<IntPoint>> transitPolygons;

            var connectedWalls = CreateConnectedAndTransit(settings, walls, out transitPolygons);

            var transitArea = CreateTransitArea(settings, floor, walls, transitPolygons);

            CreateApartments(settings, floor, intFootPrint, transitArea, connectedWalls);

            return floor;
        }

        #region Create transit area

        /// <summary>
        ///     Inserts Vertical Access Area (VAA): stairs, elevators, etc.
        ///     and merges it with corridors.
        /// </summary>
        private static List<List<IntPoint>> CreateTransitArea(InDoorGeneratorSettings settings,
            Floor floor, List<SkeletonEdge> walls, List<List<IntPoint>> transitPolygons)
        {
            foreach (var wall in walls)
            {
                if (wall.Size - settings.TransitAreaWidth > settings.VaaSizeWidth &&
                    wall.Distance > settings.VaaSizeHeight)
                {
                    var startOfVaa1 = wall.Start + (wall.End - wall.Start).Normalized() * settings.HalfTransitAreaWidth;

                    var orthogonalLeft = Vector2dUtils.OrthogonalLeft(wall.Start - wall.End).Normalized();
                    var endOfVaa1 = startOfVaa1 + orthogonalLeft * settings.VaaSizeHeight;
                    var startOfVaa2 = startOfVaa1 + (wall.End - startOfVaa1).Normalized() * settings.VaaSizeWidth;
                    var endOfVaa2 = startOfVaa2 + orthogonalLeft * settings.VaaSizeHeight;

                    settings.Clipper.AddPath(new List<IntPoint>
                    {
                        new IntPoint(startOfVaa1.X*Scale, startOfVaa1.Y*Scale),
                        new IntPoint(endOfVaa1.X*Scale, endOfVaa1.Y*Scale),
                        new IntPoint(endOfVaa2.X*Scale, endOfVaa2.Y*Scale),
                        new IntPoint(startOfVaa2.X*Scale, startOfVaa2.Y*Scale)
                    }, PolyType.ptClip, true);

                    floor.Stairs.Add(startOfVaa1);
                    floor.Stairs.Add(endOfVaa1);
                    floor.Stairs.Add(endOfVaa2);
                    floor.Stairs.Add(startOfVaa2);

                    break;
                }
            }

            settings.Clipper.AddPaths(transitPolygons, PolyType.ptSubject, true);
            var transitArea = new List<List<IntPoint>>(1);
            settings.Clipper.Execute(ClipType.ctUnion, transitArea);
            settings.Clipper.Clear();

            return transitArea;
        }

        #endregion

        #region Create walls

        /// <summary> Creates walls from skeleton. </summary>
        private static List<SkeletonEdge> CreateWalls(InDoorGeneratorSettings settings)
        {
            var edges = new List<SkeletonEdge>();
            foreach (var edgeOutput in settings.Skeleton.Edges)
            {
                var lastIndex = edgeOutput.Polygon.Count - 1;
                for (var i = 0; i <= lastIndex; i++)
                {
                    var start = edgeOutput.Polygon[i];
                    var end = edgeOutput.Polygon[i == lastIndex ? 0 : i + 1];

                    var sDist = settings.Skeleton.Distances[start];
                    var eDist = settings.Skeleton.Distances[end];
                    edges.Add(new SkeletonEdge(start, end, sDist != 0 || eDist != 0,
                        Math.Min(sDist, eDist), settings.HalfTransitAreaWidth));
                }
            }
            return edges;
        }

        /// <summary> Creates skeleton lines connected to outer walls and transit polygons. </summary>
        private static List<Wall> CreateConnectedAndTransit(InDoorGeneratorSettings settings,
            List<SkeletonEdge> walls, out List<List<IntPoint>> transitPolygons)
        {
            var offset = new ClipperOffset();
            var connected = new List<Wall>();
            var paths = new List<List<IntPoint>>();
            var skeleton = settings.Skeleton;
            foreach (var wall in walls)
            {
                var start = new IntPoint(wall.Start.X * Scale, wall.Start.Y * Scale);
                var end = new IntPoint(wall.End.X * Scale, wall.End.Y * Scale);
                if (!wall.IsOuter)
                    paths.Add(new List<IntPoint> { start, end });
                else if (wall.IsSkeleton &&
                        (skeleton.Distances[wall.Start] < settings.HalfTransitAreaWidth ||
                         skeleton.Distances[wall.End] < settings.HalfTransitAreaWidth))
                {
                    var undesired = new Wall(wall.Start, wall.End, false);
                    // TODO do I need this check here?
                    //if (!connected.Contains(undesired))
                    connected.Add(undesired);
                }
            }

            transitPolygons = new List<List<IntPoint>>(1);
            offset.AddPaths(paths, JoinType.jtMiter, EndType.etClosedLine);
            offset.Execute(ref transitPolygons, settings.HalfTransitAreaWidth * Scale);
            return connected;
        }

        #endregion

        #region Insert entrances

        private static void InsertEntrances(InDoorGeneratorSettings settings, Floor floor,
            List<SkeletonEdge> walls)
        {
            var footprint = settings.Footprint;

            // doors are not set
            if (settings.Doors == null)
                FindDoorPosition(settings);

            foreach (var door in settings.Doors)
            {
                var start = footprint[door.Key];
                var end = footprint[(door.Key + 1) % footprint.Count];

                var centerOffset = door.Value;
                var vec = (end - start).Normalized();
                var startOfDoor = start + vec * (centerOffset - settings.HalfTransitAreaWidth);
                var endOfDoor = start + vec * (centerOffset + settings.HalfTransitAreaWidth);
                floor.Entrances.Add(new LineSegment2d(startOfDoor, endOfDoor));

                var centerOfDoor = start + vec * centerOffset;
                vec.Negate();
                var doorRay = new LineParametric2d(centerOfDoor, Vector2dUtils.OrthogonalRight(vec));
                InsertEntrance(settings, walls, doorRay, centerOffset + settings.HalfTransitAreaWidth);
            }
        }

        private static void FindDoorPosition(InDoorGeneratorSettings settings)
        {
            var footprint = settings.Footprint;
            var index = 0;
            var maxDistance = 0d;
            for (int i = 0; i < footprint.Count; i++)
            {
                var start = footprint[i];
                var end = footprint[i == footprint.Count - 1 ? 0 : i + 1];
                var distance = start.DistanceTo(end);
                if (distance > maxDistance)
                {
                    index = i;
                    maxDistance = distance;
                }
            }
            settings.Doors = new List<KeyValuePair<int, double>>
                {
                    new KeyValuePair<int, double>(index, maxDistance/2d)
                };
        }

        private static void InsertEntrance(InDoorGeneratorSettings settings, List<SkeletonEdge> walls,
            LineParametric2d ray, double minimalSideSize)
        {
            var doorEdge = default(SkeletonEdge);
            var distance = double.MaxValue;
            var intersectPoint = Vector2d.Empty;

            int i = -1;
            foreach (var edge in walls)
            {
                i++;
                if (edge.IsOuter && !edge.IsSkeleton) continue;

                var currIntersect = LineParametric2d.Collide(ray, edge.Line, 0.00001);
                if (currIntersect == Vector2d.Empty ||
                    !Vector2dUtils.IsPointOnSegment(edge.Start, edge.End, currIntersect))
                    continue;

                var currDistance = currIntersect.DistanceTo(ray.A);
                if (distance > currDistance/* && currDistance > minimalSideSize*/)
                {
                    doorEdge = edge;
                    distance = currDistance;
                    intersectPoint = currIntersect;
                }
            }

            var startDist = Math.Min(distance, settings.Skeleton.Distances[doorEdge.Start]);
            var endDistance = Math.Min(distance, settings.Skeleton.Distances[doorEdge.End]);

            walls.Add(new SkeletonEdge(doorEdge.Start, intersectPoint, false, startDist, settings.HalfTransitAreaWidth));
            walls.Add(new SkeletonEdge(intersectPoint, doorEdge.End, false, endDistance, settings.HalfTransitAreaWidth));
            walls.Add(new SkeletonEdge(ray.A, intersectPoint, false, distance, settings.HalfTransitAreaWidth));
        }

        #endregion

        #region Create apartments

        private static void CreateApartments(InDoorGeneratorSettings settings, Floor floor,
            List<IntPoint> footprint, List<List<IntPoint>> transitArea, List<Wall> connected)
        {
            var extrudedPolygons = new List<List<IntPoint>>(4);
            settings.Clipper.AddPaths(transitArea, PolyType.ptClip, true);
            settings.Clipper.AddPath(footprint, PolyType.ptSubject, true);
            settings.Clipper.Execute(ClipType.ctDifference, extrudedPolygons);
            settings.Clipper.Clear();

            //SVGBuilder.SaveToFile(extrudedPolygons, "regions_ex.svg", 0.01, 100);

            foreach (var extrudedPolygon in extrudedPolygons)
            {
                // Clipper may produce small polygons on building offsets
                if (Clipper.Area(extrudedPolygon) / DoubleScale < settings.MinimalArea)
                    continue;

                var firstOuterWallIndex = InvalidIndex;
                var firstTransitWallIndex = InvalidIndex;
                var outerWallCount = 0;
                double outerWallLength = 0;
                var extrudedWalls = new List<Wall>();
                var lastItemIndex = extrudedPolygon.Count - 1;

                var mergePoint = Vector2d.Empty;
                double skippedDistance = 0;
                for (var i = 0; i <= lastItemIndex; i++)
                {
                    var start = extrudedPolygon[i];
                    var end = extrudedPolygon[i == lastItemIndex ? 0 : i + 1];

                    var isOuterWall = ClipperUtils.CalcMinDistance(start, footprint) < IntPrecisionError &&
                                      ClipperUtils.CalcMinDistance(end, footprint) < IntPrecisionError;
                    var p1 = new Vector2d(start.X / Scale, start.Y / Scale);
                    var p2 = new Vector2d(end.X / Scale, end.Y / Scale);

                    // NOTE this allows to skip artifacts of clipper offset library
                    // which I don't know to avoid by clipper API. 
                    var distance = p1.DistanceTo(p2);
                    if (distance < settings.HalfTransitAreaWidth &&
                        skippedDistance < settings.TransitAreaWidth)
                    {
                        skippedDistance += distance;
                        if (mergePoint != Vector2d.Empty)
                            continue;
                        mergePoint = p1;
                        continue;
                    }
                    if (mergePoint != Vector2d.Empty)
                    {
                        p1 = mergePoint;
                        mergePoint = Vector2d.Empty;
                        skippedDistance = 0;
                    }

                    if (isOuterWall)
                    {
                        outerWallCount++;
                        outerWallLength += ClipperUtils.Distance(start, end) / Scale;
                        if (firstOuterWallIndex == InvalidIndex)
                            firstOuterWallIndex = extrudedWalls.Count;
                    }

                    if (!isOuterWall && firstTransitWallIndex == InvalidIndex)
                        firstTransitWallIndex = extrudedWalls.Count;

                    extrudedWalls.Add(new Wall(p1, p2, isOuterWall));
                }

                firstOuterWallIndex = firstOuterWallIndex != 0
                    ? firstOuterWallIndex
                    : firstTransitWallIndex + extrudedWalls.Count - outerWallCount;

                var context = new FloorContext(outerWallCount, extrudedWalls.Count - outerWallCount,
                    firstOuterWallIndex, firstTransitWallIndex);
                CreateAparments(settings, floor, context, extrudedWalls, connected, outerWallLength);
            }
        }

        private static void CreateAparments(InDoorGeneratorSettings settings, Floor floor,
            FloorContext context, List<Wall> extrudedWalls, List<Wall> connectedSkeletonWalls,
            double outerWallLength)
        {
            var currentWidthStep = settings.PreferedWidthStep;
            var remainingOuterWallLength = outerWallLength;
            var lastUsedOuterIndex = InvalidIndex;
            var usedTransitIndex = InvalidIndex;

            var stopIterationIndex = context.FirstOuterWallIndex + context.OuterWallCount;
            for (var i = context.FirstOuterWallIndex; i < stopIterationIndex; i++)
            {
                var index = i == extrudedWalls.Count ? 0 : i % extrudedWalls.Count;

                var wall = extrudedWalls[index];
                var start = wall.Start;
                var end = wall.End;

                var distance = start.DistanceTo(end);

                remainingOuterWallLength -= distance;

                // last apartment
                if (remainingOuterWallLength < currentWidthStep)
                {
                    InsertAparment(settings, floor, context, extrudedWalls, Vector2d.Empty,
                        Vector2d.Empty, ref lastUsedOuterIndex, ref usedTransitIndex);
                    break;
                }

                var remainingCurrentWallLength = distance;
                do
                {
                    if (remainingCurrentWallLength < currentWidthStep)
                    {
                        currentWidthStep = settings.MinimalWidthStep;
                        break;
                    }
                    remainingCurrentWallLength -= currentWidthStep;

                    // get intermediate point
                    var vec = (end - start).Normalized();
                    var outerWallSplitPoint = start + vec * currentWidthStep;

                    var ortogonalRight = new Vector2d(-vec.Y, vec.X);
                    var someFarPointOnOuterWall = outerWallSplitPoint + ortogonalRight * 1000;

                    Vector2d transitWallSplitPoint;
                    int transitWallIndex;
                    if (HasIntersectPoint(extrudedWalls, connectedSkeletonWalls, outerWallSplitPoint,
                        someFarPointOnOuterWall, out transitWallSplitPoint, out transitWallIndex))
                    {
                        context.OuterWallIndex = index;
                        context.TransitWallIndex = transitWallIndex;

                        InsertAparment(settings, floor, context, extrudedWalls, outerWallSplitPoint,
                            transitWallSplitPoint, ref lastUsedOuterIndex, ref usedTransitIndex);
                        currentWidthStep = settings.PreferedWidthStep;
                    }
                    start = outerWallSplitPoint;
                } while (true);
            }
        }

        private static void InsertAparment(InDoorGeneratorSettings settings, Floor floor, FloorContext context,
            List<Wall> extrudedWalls, Vector2d outerWallSplitPoint, Vector2d transitWallSplitPoint,
            ref int lastOuterIndex, ref int lastTransitIntex)
        {
            var apartment = new Apartment(settings.ObjectPool);

            AddOuterWalls(settings, floor, context, apartment, extrudedWalls,
                outerWallSplitPoint, lastOuterIndex);

            AddTransitWalls(settings, floor, context, apartment, extrudedWalls,
                transitWallSplitPoint, lastTransitIntex);

            if (outerWallSplitPoint != Vector2d.Empty)
                floor.PartitionWalls.Add(new LineSegment2d(outerWallSplitPoint, transitWallSplitPoint));

            floor.Apartments.Add(apartment);

            lastOuterIndex = context.OuterWallIndex;
            lastTransitIntex = context.TransitWallIndex;
        }

        private static void AddOuterWalls(InDoorGeneratorSettings settings, Floor floor, FloorContext context,
            Apartment apartment, List<Wall> extrudedWalls, Vector2d outerWallSplitPoint, int lastOuterIndex)
        {
            // this is last apartment
            if (outerWallSplitPoint == Vector2d.Empty)
            {
                context.OuterWallIndex = context.LastOuterWallIndex;
                outerWallSplitPoint = extrudedWalls[context.LastOuterWallIndex].End;
            }

            // this is first apartment
            if (!floor.OuterWalls.Any())
            {
                // copy all previous walls without splitting
                if (context.OuterWallIndex != context.FirstOuterWallIndex)
                {
                    for (var i = context.FirstOuterWallIndex; i < context.OuterWallIndex;
                        i = (i + 1) % extrudedWalls.Count)
                    {
                        var wall = extrudedWalls[i];
                        AddOuterWall(floor, apartment, wall.Start, wall.End);
                    }
                }
                // add part of current wall
                AddOuterWall(floor, apartment, extrudedWalls[context.OuterWallIndex].Start, outerWallSplitPoint);
            }
            // this is intermediate or last apartment
            else
            {
                if (context.OuterWallIndex != lastOuterIndex)
                {
                    var index = lastOuterIndex;
                    while (index != context.OuterWallIndex)
                    {
                        var wall = extrudedWalls[index];

                        var startPoint = index == lastOuterIndex
                            ? floor.OuterWalls.Last().End
                            : wall.Start;

                        AddOuterWall(floor, apartment, startPoint, wall.End);
                        index = (index + 1) % extrudedWalls.Count;
                    }
                }
                AddOuterWall(floor, apartment, floor.OuterWalls.Last().End, outerWallSplitPoint);
            }
        }

        private static void AddOuterWall(Floor floor, Apartment apartment, Vector2d start, Vector2d end)
        {
            floor.OuterWalls.Add(new LineSegment2d(start, end));
            apartment.OuterWalls.Add(floor.OuterWalls.Count - 1);
        }

        private static void AddTransitWalls(InDoorGeneratorSettings settings, Floor floor, FloorContext context,
            Apartment apartment, List<Wall> extrudedWalls, Vector2d transitWallSplitPoint, int lastTransitIndex)
        {
            // this is last apartment
            if (transitWallSplitPoint == Vector2d.Empty)
            {
                context.TransitWallIndex = context.FirstTransitWallIndex;
                transitWallSplitPoint = extrudedWalls[context.FirstTransitWallIndex].Start;
            }

            // this is first apartment
            if (!floor.TransitWalls.Any())
            {
                // copy all next walls without splitting
                if (context.TransitWallIndex != context.LastTransitWallIndex)
                {
                    for (var i = context.LastTransitWallIndex; i > context.TransitWallIndex;
                        i = --i < 0 ? extrudedWalls.Count + i : i)
                    {
                        var wall = extrudedWalls[i];
                        AddTransitWall(floor, apartment, wall.End, wall.Start);
                    }
                }
                AddTransitWall(floor, apartment, extrudedWalls[context.TransitWallIndex].End, transitWallSplitPoint);
            }
            // this is intermediate or last apartment
            else
            {
                if (context.TransitWallIndex != lastTransitIndex)
                {
                    var index = lastTransitIndex;
                    while (index != context.TransitWallIndex)
                    {
                        var wall = extrudedWalls[index];
                        AddTransitWall(floor, apartment, floor.TransitWalls.Last().End, wall.Start);

                        index = --index < 0 ? extrudedWalls.Count + index : index;
                        if (index == context.LastOuterWallIndex)
                            index = context.LastTransitWallIndex;
                    }
                }
                AddTransitWall(floor, apartment, floor.TransitWalls.Last().End, transitWallSplitPoint);
            }
        }

        private static void AddTransitWall(Floor floor, Apartment apartment, Vector2d start, Vector2d end)
        {
            floor.TransitWalls.Add(new LineSegment2d(start, end));
            apartment.TransitWalls.Add(floor.TransitWalls.Count - 1);
        }

        /// <summary> Tries to find intersection with transit walls. </summary>
        private static bool HasIntersectPoint(List<Wall> extrudedWalls,
            List<Wall> connectedSkeletonWalls, Vector2d p1, Vector2d p2,
            out Vector2d intersectPoint, out int transitWallIndex)
        {
            intersectPoint = new Vector2d();
            transitWallIndex = -1;
            var distance = double.MaxValue;
            for (var i = 0; i < extrudedWalls.Count; i++)
            {
                var wall = extrudedWalls[i];
                if (wall.IsOuter) continue;

                var start = wall.Start;
                var end = wall.End;

                double r;
                if (!Vector2dUtils.LineIntersects(p1, p2, start, end, out r))
                    continue;

                var currIntersectPoint = new Vector2d(
                    Math.Round(p1.X + (r * (p2.X - p1.X))),
                    Math.Round((p1.Y + (r * (p2.Y - p1.Y)))));

                // skip intersections of skeleton edges which are connected with footprint
                var skip = false;
                foreach (var connectedWall in connectedSkeletonWalls)
                {
                    if (Vector2dUtils.LineIntersects(p1, currIntersectPoint,
                        connectedWall.Start, connectedWall.End, out r))
                    {
                        skip = true;
                        break;
                    }
                }

                if (skip) continue;

                if (Vector2dUtils.IsPointOnSegment(start, end, currIntersectPoint))
                {
                    var currDistance = p1.DistanceTo(currIntersectPoint);
                    if (currDistance > 0 && currDistance < distance)
                    {
                        intersectPoint = currIntersectPoint;
                        transitWallIndex = i;
                        distance = currDistance;
                    }
                }
            }
            return transitWallIndex != -1;
        }

        #endregion

        #region Nested classes

        private class FloorContext
        {
            public int OuterWallIndex;
            public int TransitWallIndex;
            public readonly int FirstOuterWallIndex;
            public readonly int FirstTransitWallIndex;
            public readonly int LastOuterWallIndex;
            public readonly int LastTransitWallIndex;
            public readonly int OuterWallCount;

            public FloorContext(int outerWallCount, int transitWallCount,
                int firstOuterWallIndex, int firstTransitWallIndex)
            {
                OuterWallCount = outerWallCount;

                FirstOuterWallIndex = firstOuterWallIndex;
                FirstTransitWallIndex = firstTransitWallIndex;

                var total = outerWallCount + transitWallCount;

                LastOuterWallIndex = (firstOuterWallIndex + outerWallCount - 1) % total;
                LastTransitWallIndex = (firstTransitWallIndex + transitWallCount - 1) % total;

                OuterWallIndex = -1;
                TransitWallIndex = -1;
            }
        }

        private struct SkeletonEdge
        {
            public readonly Vector2d Start;
            public readonly Vector2d End;
            public readonly LineLinear2d Line;

            public readonly bool IsSkeleton;
            public readonly double Distance;
            public readonly double Size;

            public readonly bool IsOuter;

            public SkeletonEdge(Vector2d start, Vector2d end, bool isSkeleton, double distance,
                double outerLimit)
            {
                Start = start;
                End = end;
                Line = new LineLinear2d(start, end);
                IsSkeleton = isSkeleton;
                Distance = distance;
                IsOuter = distance < outerLimit;

                Size = Start.DistanceTo(End);
            }


            public override string ToString()
            {
                return string.Format("[({0}, {1}) ({2}, {3})]: o:{4} s:{5}",
                    Start.X, Start.Y, End.X, End.Y, IsOuter, IsSkeleton);
            }
        }

        private struct Wall
        {
            public readonly Vector2d Start;
            public readonly Vector2d End;
            public readonly bool IsOuter;

            public Wall(Vector2d start, Vector2d end, bool isOuter)
            {
                Start = start;
                End = end;
                IsOuter = isOuter;
            }

            public override string ToString()
            {
                return String.Format("[({0},{1}), ({2},{3})]:{4}", Start.X, Start.Y, End.X, End.Y, IsOuter);
            }
        }

        #endregion
    }
}