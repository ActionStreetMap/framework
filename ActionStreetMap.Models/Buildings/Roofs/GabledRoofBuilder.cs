using System;
using System.Collections.Generic;
using ActionStreetMap.Core;
using ActionStreetMap.Core.Scene.World.Buildings;
using ActionStreetMap.Infrastructure.Dependencies;
using ActionStreetMap.Infrastructure.Primitives;
using ActionStreetMap.Infrastructure.Utilities;
using ActionStreetMap.Models.Geometry;
using ActionStreetMap.Models.Geometry.Primitives;
using UnityEngine;

namespace ActionStreetMap.Models.Buildings.Roofs
{
    /// <summary>
    ///     Builds gabled roof.
    ///     See http://wiki.openstreetmap.org/wiki/Key:roof:shape#Roof
    /// </summary>
    public class GabledRoofBuilder : IRoofBuilder
    {
        private readonly IObjectPool _objectPool;

        /// <inheritdoc />
        public string Name { get { return "gabled"; } }

        /// <inheritdoc />
        public bool CanBuild(Building building) { return true; }

        /// <summary>
        ///     Creates GabledRoofBuilder.
        /// </summary>
        /// <param name="objectPool">Object pool.</param>
        [Dependency]
        public GabledRoofBuilder(IObjectPool objectPool)
        {
            _objectPool = objectPool;
        }

        /// <inheritdoc />
        public MeshData Build(Building building, BuildingStyle style)
        {
            var context = new Context(style, _objectPool);
            var roofOffset = building.Elevation + building.Height + building.MinHeight;
            var roofHeight = roofOffset + (building.RoofHeight > 0 ? building.RoofHeight : style.Roof.Height);

            var polygon = new Polygon(building.Footprint, roofOffset);

            // 1. detect the longest segment
            float length;
            var longestSegment = GetLongestSegment(building.Footprint, out length);
        
            // 2. get direction vector
            Vector3 ridgeDirection = longestSegment.End - longestSegment.Start;
            ridgeDirection.Normalize();

            // 3. get centroid
            var centroidPoint = PolygonUtils.GetCentroid(building.Footprint);
            var centroidVector = new Vector3(centroidPoint.X, longestSegment.Start.y, centroidPoint.Y);

            // 4. get something like center line
            Vector3 p1 = centroidVector + length * length * ridgeDirection;
            Vector3 p2 = centroidVector - length * length * ridgeDirection;
            var centerSegment = new Segment(p1, p2);

            // 5. detect segments which have intesection with center line
            MutableTuple<int, Vector3> firstIntersect;
            MutableTuple<int, Vector3> secondIntersect;
            DetectIntersectSegments(polygon, centerSegment, out firstIntersect, out secondIntersect);
            if (firstIntersect.Item1 == -1 || secondIntersect.Item1 == -1)
                throw new AlgorithmException(String.Format(Strings.GabledRoofGenFailed, building.Id));
            // move vertices up to make ridge
            firstIntersect.Item2 = new Vector3(firstIntersect.Item2.x, roofHeight, firstIntersect.Item2.z);
            secondIntersect.Item2 = new Vector3(secondIntersect.Item2.x, roofHeight, secondIntersect.Item2.z);

            // 6. process all segments and create vertices
            FillMeshData(polygon, firstIntersect, secondIntersect, context);

            var result = new MeshData()
            {
                Vertices = context.Points.ToArray(),
                Triangles = context.Triangles.ToArray(),
                UV = context.UV.ToArray(),
                MaterialKey = style.Roof.Path,
            };
            context.Dispose();
            return result;
        }

        private Segment GetLongestSegment(List<MapPoint> footprint, out float length)
        {
            var result = _objectPool.NewList<MapPoint>();
            PolygonUtils.Simplify(footprint, result, 1, _objectPool);
            var polygon = new Polygon(result);
            Segment longestSegment = null;
            length = 0;
            for (int i = 0; i < polygon.Segments.Length; i++)
            {
                var segment = polygon.Segments[i];
                var segmentLength = segment.GetLength();
                if (segmentLength > length)
                {
                    longestSegment = segment;
                    length = segmentLength;
                }
            }
            _objectPool.Store(result);
            return longestSegment;
        }

        private void DetectIntersectSegments(Polygon polygon, Segment centerSegment, 
            out MutableTuple<int, Vector3> firstIntersect, 
            out MutableTuple<int, Vector3> secondIntersect)
        {
            firstIntersect = new MutableTuple<int, Vector3>(-1, new Vector3());
            secondIntersect = new MutableTuple<int, Vector3>(-1, new Vector3());
            for (int i = 0; i < polygon.Segments.Length; i++)
            {
                var segment = polygon.Segments[i];
                if (SegmentUtils.Intersect(segment, centerSegment))
                {
                    var intersectionPoint = SegmentUtils.IntersectionPoint(segment, centerSegment);
                    if (firstIntersect.Item1 == -1)
                    {
                        firstIntersect.Item1 = i;
                        firstIntersect.Item2 = intersectionPoint;
                    }
                    else
                    {
                        secondIntersect.Item1 = i;
                        secondIntersect.Item2 = intersectionPoint;
                        break;
                    }
                }
            }
        }

        private void FillMeshData(Polygon polygon, MutableTuple<int, Vector3> firstIntersect,
            MutableTuple<int, Vector3> secondIntersect, Context context)
        {
            var count = polygon.Segments.Length;
            int i = secondIntersect.Item1;
            Vector3 startRidgePoint = default(Vector3);
            Vector3 endRidgePoint = default(Vector3);
            do 
            {
                var segment = polygon.Segments[i];
                var nextIndex = i == count - 1 ? 0 : i + 1;
                // front faces
                if (i == firstIntersect.Item1 || i == secondIntersect.Item1)
                {
                    startRidgePoint = i == firstIntersect.Item1 ? firstIntersect.Item2 : secondIntersect.Item2;
                    AddTriangle(segment.Start, segment.End, startRidgePoint, context);
                    i = nextIndex;
                    continue;
                }
                // side faces
                if (nextIndex == firstIntersect.Item1 || nextIndex == secondIntersect.Item1)
                    endRidgePoint = nextIndex == firstIntersect.Item1 ? firstIntersect.Item2 : secondIntersect.Item2;
                else
                    endRidgePoint = GetPointOnLine(firstIntersect.Item2, secondIntersect.Item2, segment.End);

                AddTrapezoid(segment.Start, segment.End, endRidgePoint, startRidgePoint, context);

                startRidgePoint = endRidgePoint;
                i = nextIndex;
            } while (i != secondIntersect.Item1);
        }

        private void AddTriangle(Vector3 first, Vector3 second, Vector3 third, Context context)
        {
            context.Points.Add(first);
            context.Points.Add(second);
            context.Points.Add(third);

            context.Triangles.Add(context.TrisIndex + 0);
            context.Triangles.Add(context.TrisIndex + 1);
            context.Triangles.Add(context.TrisIndex + 2);

            // TODO process UV map different way
            context.UV.Add(context.Style.Roof.FrontUvMap.RightUpper);
            context.UV.Add(context.Style.Roof.FrontUvMap.RightUpper);
            context.UV.Add(context.Style.Roof.FrontUvMap.RightUpper);

            context.TrisIndex += 3;
        }

        private void AddTrapezoid(Vector3 rightStart, Vector3 leftStart, Vector3 leftEnd, Vector3 rightEnd, Context context)
        {
            context.Points.Add(rightStart);
            context.Points.Add(leftStart);
            context.Points.Add(leftEnd);
            context.Points.Add(rightEnd);

            context.Triangles.Add(context.TrisIndex + 0);
            context.Triangles.Add(context.TrisIndex + 1);
            context.Triangles.Add(context.TrisIndex + 2);
            context.Triangles.Add(context.TrisIndex + 2);
            context.Triangles.Add(context.TrisIndex + 3);
            context.Triangles.Add(context.TrisIndex + 0);
            context.TrisIndex += 4;

            // TODO process UV map different way
            context.UV.Add(context.Style.Roof.FrontUvMap.RightUpper);
            context.UV.Add(context.Style.Roof.FrontUvMap.RightUpper);
            context.UV.Add(context.Style.Roof.FrontUvMap.RightUpper);
            context.UV.Add(context.Style.Roof.FrontUvMap.RightUpper);
        }

        /// <summary>
        /// Gets point on line. See http://stackoverflow.com/questions/5227373/minimal-perpendicular-vector-between-a-point-and-a-line
        /// </summary>
        private Vector3 GetPointOnLine(Vector3 a, Vector3 b, Vector3 p)
        {
            var d = (a - b).normalized;
            var x = a + Vector3.Dot(p - a, d) * d;
            return x;
        }

        #region Nested classes

        private sealed class Context: IDisposable
        {
            private readonly IObjectPool _objectPool;

            public readonly BuildingStyle Style;
            public readonly List<Vector3> Points;
            public readonly List<int> Triangles;
            public readonly List<Vector2> UV;
            
            public int TrisIndex;          

            public Context(BuildingStyle style, IObjectPool objectPool)
            {
                Style = style;
                _objectPool = objectPool;
                Points = objectPool.NewList<Vector3>(64);
                Triangles = objectPool.NewList<int>(128);
                UV = objectPool.NewList<Vector2>(64);
            }

            public void Dispose()
            {
                _objectPool.Store(Points);
                _objectPool.Store(Triangles);
                _objectPool.Store(UV);
            }
        }

        #endregion
    }
}
