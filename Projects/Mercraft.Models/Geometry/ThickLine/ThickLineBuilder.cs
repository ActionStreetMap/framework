﻿using System;
using System.Collections.Generic;
using Mercraft.Core;
using Mercraft.Core.Elevation;
using Mercraft.Infrastructure.Primitives;
using Mercraft.Models.Geometry.Primitives;
using Mercraft.Models.Utils;
using UnityEngine;

namespace Mercraft.Models.Geometry.ThickLine
{
    /// <summary>
    ///     Builds thick 2D line in 3D space.
    /// </summary>
    public class ThickLineBuilder
    {
        // TODO this value depends on heightmap accuracy
        private const float MaxPointDistance = 4f;

        private HeightMap _heightMap;

        /// <summary>
        ///     Points.
        /// </summary>
        protected List<Vector3> Points = new List<Vector3>(1024);

        /// <summary>
        ///     Triangles.
        /// </summary>
        protected List<int> Triangles = new List<int>(2048);

        /// <summary>
        ///     Uv map.
        /// </summary>
        protected List<Vector2> Uv = new List<Vector2>(1024);

        /// <summary>
        ///     Current Triangle index.
        /// </summary>
        protected int TrisIndex = 0;

        /// <summary>
        ///     UV ratio. TODO ratio depends on texture
        /// </summary>
        protected float Ratio = 20;

        private Tuple<Vector3, Vector3> _startPoints;

        private int _elementIndex;
        private bool _isLastElement;

        private readonly HeightMapProcessor _heightMapProcessor = new HeightMapProcessor();

        /// <summary>
        ///     Builds line.
        /// </summary>
        /// <param name="heightMap">Heightmap.</param>
        /// <param name="elements">Line elements.</param>
        /// <param name="builder">Builds unity objects.</param>
        public virtual void Build(HeightMap heightMap, List<LineElement> elements,
            Action<List<Vector3>, List<int>, List<Vector2>> builder)
        {
            _heightMap = heightMap;

            var lineElements = ThickLineUtils.GetLineElementsInTile(heightMap.LeftBottomCorner, 
                heightMap.RightUpperCorner, elements);
            var elementsCount = lineElements.Count;
            for (_elementIndex = 0; _elementIndex < elementsCount; _elementIndex++)
            {
                _isLastElement = _elementIndex == elementsCount - 1;

                var lineElement = lineElements[_elementIndex];
                // it means that there is no connection to previous line element
                if (lineElement.IsNotContinuation)
                    _startPoints = null;

                ProcessLine(lineElement, lineElements);
            }

            builder(Points, Triangles, Uv);

            // reset state defaults
            _heightMap = null;
            TrisIndex = 0;
            _startPoints = null;
            _isLastElement = false;
            Points.Clear();
            Triangles.Clear();
            Uv.Clear();
        }

        #region Segment processing

        /// <summary>
        ///     Process line segment.
        /// </summary>
        /// <param name="lineElement">Line element.</param>
        /// <param name="lineElements">Line elements.</param>
        protected void ProcessLine(LineElement lineElement, List<LineElement> lineElements)
        {
            var lineSegments = GetThickSegments(lineElement);

            // NOTE Sometimes the road has only one point (wrong pbf file?)
            if (lineSegments.Count == 0)
                return;

            ProcessFirstSegments(lineSegments);
            ProcessLastSegment(lineElements, lineSegments, lineElement.Width);
        }

        /// <summary>
        ///     Processes first road segments except last one (if LineSegments.Count > 1)
        /// </summary>
        private void ProcessFirstSegments(List<ThickLineSegment> lineSegments)
        {
            var segmentsCount = lineSegments.Count;
            if (segmentsCount == 1)
            {
                AddTrapezoid(lineSegments[0].Left, lineSegments[0].Right);
                _startPoints = new Tuple<Vector3, Vector3>(lineSegments[0].Right.End, lineSegments[0].Left.End);
            }
            else
            {
                if (_startPoints == null)
                    _startPoints = new Tuple<Vector3, Vector3>(lineSegments[0].Right.Start, lineSegments[0].Left.Start);

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

        /// <summary>
        ///     Processes last road segment of current RoadElement
        /// </summary>
        private void ProcessLastSegment(List<LineElement> lineElements, List<ThickLineSegment> lineSegments, float width)
        {
            var segmentsCount = lineSegments.Count;

            // We have to connect last segment with first segment of next road element
            if (!_isLastElement)
            {
                var first = lineSegments[segmentsCount - 1];
                var nextRoadElement = lineElements[_elementIndex + 1];

                // NOTE we couldn't connect last segment of current element with next cause it's marked as not continuation
                if (nextRoadElement.IsNotContinuation)
                    return;

                MapPoint secondPoint = _heightMap.IsFlat
                    ? nextRoadElement.Points[1]
                    // we split lineElement to smaller parts in non-flat mode
                    : ThickLineUtils.GetNextIntermediatePoint(_heightMap,
                        nextRoadElement.Points[0],
                        nextRoadElement.Points[1], MaxPointDistance);

                var second = ThickLineHelper.GetThickSegment(nextRoadElement.Points[0], secondPoint, width);

                Vector3 nextIntersectionPoint;
                switch (ThickLineHelper.GetDirection(first, second))
                {
                    case ThickLineHelper.Direction.Straight:
                        AddTrapezoid(second.Right.Start, second.Left.Start, second.Left.End, second.Right.End);
                        _startPoints = new Tuple<Vector3, Vector3>(first.Right.End, first.Left.End);
                        break;
                    case ThickLineHelper.Direction.Left:
                        nextIntersectionPoint = SegmentUtils.IntersectionPoint(first.Left, second.Left);
                        AddTrapezoid(_startPoints.Item1, _startPoints.Item2, nextIntersectionPoint, first.Right.End);
                        AddTriangle(first.Right.End, nextIntersectionPoint, second.Right.Start, true);
                        _startPoints = new Tuple<Vector3, Vector3>(second.Right.Start, nextIntersectionPoint);
                        break;
                    case ThickLineHelper.Direction.Right:
                        nextIntersectionPoint = SegmentUtils.IntersectionPoint(first.Right, second.Right);
                        AddTrapezoid(_startPoints.Item1, _startPoints.Item2, first.Left.End, nextIntersectionPoint);
                        AddTriangle(first.Left.End, nextIntersectionPoint, second.Left.Start, false);
                        _startPoints = new Tuple<Vector3, Vector3>(nextIntersectionPoint, second.Left.Start);
                        break;
                }
            }
            else
            {
                // TODO do I need this?
                var lastSegment = lineSegments[segmentsCount - 1];
                AddTrapezoid(_startPoints.Item1, _startPoints.Item2,
                    lastSegment.Left.End, lastSegment.Right.End);
            }
        }

        #endregion

        #region Turn/Straight cases

        private void StraightLineCase(ThickLineSegment first, ThickLineSegment second)
        {
            AddTrapezoid(_startPoints.Item1, _startPoints.Item2, first.Left.End, first.Right.End);
            _startPoints = new Tuple<Vector3, Vector3>(first.Right.End, first.Left.End);
        }

        private void TurnRightCase(ThickLineSegment first, ThickLineSegment second)
        {
            var intersectionPoint = SegmentUtils.IntersectionPoint(first.Right, second.Right);
            AddTrapezoid(_startPoints.Item1, _startPoints.Item2, first.Left.End, intersectionPoint);
            AddTriangle(first.Left.End, intersectionPoint, second.Left.Start, false);
            _startPoints = new Tuple<Vector3, Vector3>(intersectionPoint, second.Left.Start);
        }

        private void TurnLeftCase(ThickLineSegment first, ThickLineSegment second)
        {
            var intersectionPoint = SegmentUtils.IntersectionPoint(first.Left, second.Left);
            AddTrapezoid(_startPoints.Item1, _startPoints.Item2, intersectionPoint, first.Right.End);
            AddTriangle(first.Right.End, intersectionPoint, second.Right.Start, true);
            _startPoints = new Tuple<Vector3, Vector3>(second.Right.Start, intersectionPoint);
        }

        #endregion

        #region Add shapes

        /// <summary>
        ///     Adds triangle.
        /// </summary>
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

        /// <summary>
        ///     Adds trapezoid.
        /// </summary>
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
            var lineSegments = new List<ThickLineSegment>();

            List<MapPoint> points;
            if (_heightMap.IsFlat)
                points = lineElement.Points;
            else
            {
                _heightMapProcessor.Recycle(_heightMap);
                // we should add intermediate points between given to follow elevation changes more smooth 
                points = ThickLineUtils.GetIntermediatePoints(_heightMap, lineElement.Points, MaxPointDistance);
                for (int i = 0; i < points.Count - 1; i++)
                    _heightMapProcessor.AdjustLine(points[i], points[i + 1], lineElement.Width);
                _heightMapProcessor.Clear();
            }

            for (int i = 1; i < points.Count; i++)
                lineSegments.Add(ThickLineHelper.GetThickSegment(points[i - 1], points[i], lineElement.Width));

            return lineSegments;
        }

        #endregion
    }
}