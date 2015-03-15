using System;
using System.Collections.Generic;
using ActionStreetMap.Core;
using ActionStreetMap.Infrastructure.Primitives;
using ActionStreetMap.Infrastructure.Utilities;
using ActionStreetMap.Explorer.Scene.Geometry.Primitives;

using UnityEngine;

namespace ActionStreetMap.Explorer.Scene.Geometry.ThickLine
{
    /// <summary> Builds thick 2D line in 3D space. Not thread safe. </summary>
    public class ThickLineBuilder: IDisposable
    {
        private const float MaxPointDistance = 8f;

        /// <summary> Points. </summary>
        protected List<Vector3> Points;

        /// <summary> Triangles. </summary>
        protected List<int> Triangles;

        /// <summary> Uv map. </summary>
        protected List<Vector2> Uv;

        /// <summary> Current Triangle index. </summary>
        protected int TrisIndex = 0;

        /// <summary> 
        ///     UV ratio. TODO ratio depends on texture 
        /// </summary>
        protected float Ratio = 20;

        private MutableTuple<Vector3, Vector3> _startPoints;

        private LineElement _currentElement;
        private LineElement _nextElement;

        private readonly IElevationProvider _elevationProvider;
        private readonly IObjectPool _objectPool;

        /// <summary> Creates instance of <see cref="ThickLineBuilder"/>. </summary>
        /// <param name="elevationProvider">Elevation provider.</param>
        /// <param name="objectPool">Object pool.</param>
        public ThickLineBuilder(IElevationProvider elevationProvider, IObjectPool objectPool)
        {
            _elevationProvider = elevationProvider;
            _objectPool = objectPool;

            // TODO determine best initial size
            Points = _objectPool.NewList<Vector3>(1024);
            Triangles = _objectPool.NewList<int>(2048);
            Uv = _objectPool.NewList<Vector2>(1024);
        }

        /// <summary> Builds line. </summary>
        /// <param name="rectangle">Rectangle.</param>
        /// <param name="elements">Line elements.</param>
        /// <param name="builder">Builds unity objects.</param>
        public virtual void Build(MapRectangle rectangle, List<LineElement> elements,
            Action<List<Vector3>, List<int>, List<Vector2>> builder)
        {
            var lineElements = _objectPool.NewList<LineElement>(8);
            ThickLineUtils.GetLineElementsInTile(rectangle.BottomLeft,
                rectangle.TopRight, elements, lineElements, _objectPool);
            var elementsCount = lineElements.Count;

            // TODO сurrent implementation of GetLineElementsInTile skip segment if its
            // points are located outside given rectangle
            if (elementsCount == 0) return;

            for (var i = 0; i < elementsCount; i++)
            {
                _currentElement = lineElements[i];
                _nextElement = i == elementsCount - 1 ? null : lineElements[i + 1];
                ProcessLine(lineElements);
            }

            builder(Points, Triangles, Uv);
            _objectPool.StoreList(lineElements);
        }

        #region Segment processing

        /// <summary> Process line segment. </summary>
        /// <param name="lineElements">Line elements.</param>
        protected void ProcessLine(List<LineElement> lineElements)
        {
            var lineSegments = GetThickSegments(_currentElement);

            // NOTE Sometimes the road has only one point (wrong pbf file?)
            if (lineSegments.Count == 0)
                return;

            ProcessFirstSegments(lineSegments);
            ProcessLastSegment(lineSegments, _currentElement.Width);
        }

        /// <summary> Processes first road segments except last one (if LineSegments.Count > 1). </summary>
        private void ProcessFirstSegments(List<ThickLineSegment> lineSegments)
        {
            var segmentsCount = lineSegments.Count;
            if (segmentsCount == 1)
            {
                AddTrapezoid(lineSegments[0].Left, lineSegments[0].Right);
                _startPoints = new MutableTuple<Vector3, Vector3>(lineSegments[0].Right.End, lineSegments[0].Left.End);
            }
            else
            {
                if (_startPoints == null)
                    _startPoints = new MutableTuple<Vector3, Vector3>(lineSegments[0].Right.Start, lineSegments[0].Left.Start);

                for (int i = 1; i < segmentsCount; i++)
                {
                    var s1 = lineSegments[i - 1];
                    var s2 = lineSegments[i];
                    switch (ThickLineHelper.GetDirection(s1, s2))
                    {
                        case ThickLineHelper.Direction.Straight:
                            StraightLineCase(s1, s2);
                            break;
                        case ThickLineHelper.Direction.Left:
                            TurnLeftCase(s1, s2);
                            break;
                        case ThickLineHelper.Direction.Right:
                            TurnRightCase(s1, s2);
                            break;
                    }
                }
            }
        }

        /// <summary> Processes last road segment of current RoadElement. </summary>
        private void ProcessLastSegment(List<ThickLineSegment> lineSegments, float width)
        {
            var segmentsCount = lineSegments.Count;

            // We have to connect last segment with first segment of next road element
            if (_nextElement != null && 
                _currentElement.Points[_currentElement.Points.Count - 1].Equals(_nextElement.Points[0]))
            {
                var first = lineSegments[segmentsCount - 1];

                MapPoint secondPoint = ThickLineUtils.GetNextIntermediatePoint(
                    _elevationProvider,
                    _nextElement.Points[0],
                    _nextElement.Points[1], MaxPointDistance);

                var second = ThickLineHelper.GetThickSegment(_nextElement.Points[0], secondPoint, width);

                Vector3 nextIntersectionPoint;
                switch (ThickLineHelper.GetDirection(first, second))
                {
                    case ThickLineHelper.Direction.Straight:
                        AddTrapezoid(second.Right.Start, second.Left.Start, second.Left.End, second.Right.End);
                        _startPoints = new MutableTuple<Vector3, Vector3>(first.Right.End, first.Left.End);
                        break;
                    case ThickLineHelper.Direction.Left:
                        nextIntersectionPoint = SegmentUtils.IntersectionPoint(first.Left, second.Left);
                        AddTrapezoid(_startPoints.Item1, _startPoints.Item2, nextIntersectionPoint, first.Right.End);
                        AddTriangle(first.Right.End, nextIntersectionPoint, second.Right.Start, true);
                        _startPoints = new MutableTuple<Vector3, Vector3>(second.Right.Start, nextIntersectionPoint);
                        break;
                    case ThickLineHelper.Direction.Right:
                        nextIntersectionPoint = SegmentUtils.IntersectionPoint(first.Right, second.Right);
                        AddTrapezoid(_startPoints.Item1, _startPoints.Item2, first.Left.End, nextIntersectionPoint);
                        AddTriangle(first.Left.End, nextIntersectionPoint, second.Left.Start, false);
                        _startPoints = new MutableTuple<Vector3, Vector3>(nextIntersectionPoint, second.Left.Start);
                        break;
                }
            }
            else
            {
                var lastSegment = lineSegments[segmentsCount - 1];
                AddTrapezoid(_startPoints.Item1, _startPoints.Item2,
                    lastSegment.Left.End, lastSegment.Right.End);
                _startPoints = null;
            }
        }

        #endregion

        #region Turn/Straight cases

        private void StraightLineCase(ThickLineSegment first, ThickLineSegment second)
        {
            AddTrapezoid(_startPoints.Item1, _startPoints.Item2, first.Left.End, first.Right.End);
            _startPoints = new MutableTuple<Vector3, Vector3>(first.Right.End, first.Left.End);
        }

        private void TurnRightCase(ThickLineSegment first, ThickLineSegment second)
        {
            var intersectionPoint = SegmentUtils.IntersectionPoint(first.Right, second.Right);
            AddTrapezoid(_startPoints.Item1, _startPoints.Item2, first.Left.End, intersectionPoint);
            AddTriangle(first.Left.End, intersectionPoint, second.Left.Start, false);
            _startPoints = new MutableTuple<Vector3, Vector3>(intersectionPoint, second.Left.Start);
        }

        private void TurnLeftCase(ThickLineSegment first, ThickLineSegment second)
        {
            var intersectionPoint = SegmentUtils.IntersectionPoint(first.Left, second.Left);
            AddTrapezoid(_startPoints.Item1, _startPoints.Item2, intersectionPoint, first.Right.End);
            AddTriangle(first.Right.End, intersectionPoint, second.Right.Start, true);
            _startPoints = new MutableTuple<Vector3, Vector3>(second.Right.Start, intersectionPoint);
        }

        #endregion

        #region Add shapes

        /// <summary> Adds triangle. </summary>
        protected virtual void AddTriangle(Vector3 first, Vector3 second, Vector3 third, bool invert)
        {
            Points.Add(first);
            Points.Add(second);
            Points.Add(third);

            Triangles.Add(TrisIndex + 0);
            Triangles.Add(TrisIndex + (invert ? 1 : 2));
            Triangles.Add(TrisIndex + (invert ? 2 : 1));

            Uv.Add(new Vector2(0f, 0f));
            Uv.Add(new Vector2(1f, 0f));
            Uv.Add(new Vector2(0f, 1f));

            TrisIndex += 3;
        }

        private void AddTrapezoid(Segment left, Segment right)
        {
            AddTrapezoid(right.Start, left.Start, left.End, right.End);
        }

        /// <summary> Adds trapezoid. </summary>
        protected virtual void AddTrapezoid(Vector3 rightStart, Vector3 leftStart, Vector3 leftEnd, Vector3 rightEnd)
        {
            Points.Add(rightStart);
            Points.Add(leftStart);
            Points.Add(leftEnd);
            Points.Add(rightEnd);

            Triangles.Add(TrisIndex + 0);
            Triangles.Add(TrisIndex + 1);
            Triangles.Add(TrisIndex + 2);
            Triangles.Add(TrisIndex + 2);
            Triangles.Add(TrisIndex + 3);
            Triangles.Add(TrisIndex + 0);
            TrisIndex += 4;

            var distance = Vector3.Distance(rightStart, rightEnd);
            float tiles = distance/Ratio;

            Uv.Add(new Vector2(1f, 0f));
            Uv.Add(new Vector2(0f, 0f));
            Uv.Add(new Vector2(0f, tiles));
            Uv.Add(new Vector2(1, tiles));
        }

        #endregion

        #region Getting segments and turn types

        private List<ThickLineSegment> GetThickSegments(LineElement lineElement)
        {
            var points = ThickLineUtils.GetIntermediatePoints(_elevationProvider, lineElement.Points, MaxPointDistance);
            var lineSegments = new List<ThickLineSegment>(points.Count);
            for (int i = 1; i < points.Count; i++)
                lineSegments.Add(ThickLineHelper.GetThickSegment(points[i - 1], points[i], lineElement.Width));

            return lineSegments;
        }

        #endregion

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
        }

        /// <summary> Dispose pattern implementation. </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Returns objects back to pool.
                _objectPool.StoreList(Points);
                _objectPool.StoreList(Triangles);
                _objectPool.StoreList(Uv);
            }
        }
    }
}