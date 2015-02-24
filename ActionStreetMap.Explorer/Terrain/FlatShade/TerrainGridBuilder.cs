using System;
using System.Collections.Generic;
using ActionStreetMap.Unity.Wrappers;
using UnityEngine;

namespace ActionStreetMap.Explorer.Terrain.FlatShade
{
    /// <summary> Builds terrain grid. </summary>
    internal class TerrainGridBuilder
    {
        // defines count of cells for one side
        private const int GridRowCount = 3;
        private readonly float _size;
        private readonly float _cellSize;
        private readonly int _resolution;
        private readonly int _cellResolution;
        private readonly TerrainCellBuilder[,] _cells;

        // left bottom corner
        private Vector2 _position;

        public TerrainGridBuilder(float size, int resolution)
        {
            _size = size;
            _resolution = resolution;
            _cellSize = size/GridRowCount;
            _cellResolution = resolution/GridRowCount;
            _cells = new TerrainCellBuilder[GridRowCount, GridRowCount];

            // generate cells
            for (int y = 0; y < GridRowCount; y++)
                for (int x = 0; x < GridRowCount; x++)
                    _cells[y, x] = new TerrainCellBuilder(_cellSize, _cellResolution);
        }

        /// <summary> Builds grid starting from position using given heightmap. </summary>
        public TerrainGridBuilder Move(Vector2 position, float[,] heightmap, GradientWrapper gradient)
        {
            _position = position;
            for (int y = 0; y < GridRowCount; y++)
                for (int x = 0; x < GridRowCount; x++)
                {
                    var subTilePosition = new Vector2(_position.x + x*_cellSize, _position.y + y*_cellSize);
                    _cells[y, x].Move(subTilePosition, heightmap, _cellResolution * x, _cellResolution * y, gradient);
                }
            return this;
        }

        /// <summary> Fills grid with given areas. </summary>
        public TerrainGridBuilder Fill(List<GradientArea> areas)
        {
            var ratio = _resolution/_size;
            foreach (var area in areas)
            {
                var pointsCount = area.Points.Count;
                // TODO use list from object pool
                var segments = new List<Segment2D>(pointsCount);
                for (int i = 0; i < pointsCount; i++)
                {
                    var endIndex = i == pointsCount - 1 ? 0 : i + 1;
                    var start = new Vector2((area.Points[i].X - _position.x)*ratio,
                        (area.Points[i].Y - _position.y)*ratio);
                    var end = new Vector2((area.Points[endIndex].X - _position.x)*ratio,
                        (area.Points[endIndex].Y - _position.y)*ratio);
                    segments.Add(new Segment2D(start, end));
                }
                ScanAndFill(segments, _resolution, area.Gradient);
            }
            return this;
        }

        /// <summary> Builds terrain meshes. </summary>
        public IEnumerable<Mesh> Build()
        {
            for (int y = 0; y < GridRowCount; y++)
            {
                for (int x = 0; x < GridRowCount; x++)
                {
                    var meshData = new Mesh {name = String.Format("cell_{0}_{1}", y, x)};
                    _cells[y, x].Update(meshData);
                    yield return  meshData;
                }
            }
        }

        /// <summary> Fills cells with given gradient. </summary>
        private void Fill(GradientWrapper gradient, int line, int start, int end)
        {
            var yIndex = line / _cellResolution;
            var xStartIndex = start / _cellResolution;
            var xEndIndex = end / _cellResolution;

            var subTileLine = line % _cellResolution;

            // multiply horizontal tile affected
            if (xStartIndex != xEndIndex)
            {
                var currentXIndex = xStartIndex;
                var currentXStart = start;
                for (int i = start; i <= end; i++)
                {
                    var index = i/_cellResolution;
                    if (currentXIndex != index || i == end)
                    {
                        // map grid indices to cell ones
                        var offset = currentXIndex*_cellResolution;
                        var currentStart = currentXStart - offset;
                        var currentEnd = i - offset;

                        _cells[yIndex, currentXIndex].Fill(gradient, subTileLine, currentStart, currentEnd);
                        currentXIndex = index;
                        currentXStart = i;
                    }
                }
            }
            // one tile is affected
            else
                _cells[yIndex, xStartIndex].Fill(gradient, subTileLine, start % _cellResolution, end % _cellResolution);
        }

        #region ScanLine algorithm modification

        /// <summary> Custom version of ScanLine algorithm to process terrain data. </summary>
        private void ScanAndFill(List<Segment2D> segments, int size, GradientWrapper gradient)
        {
            // TODO use object pool
            var pointsBuffer = new List<int>();
            for (int y = 0; y < size; y++)
            {
                foreach (var segment in segments)
                {
                    if ((segment.Start.y > y && segment.End.y > y) || // above
                        (segment.Start.y < y && segment.End.y < y)) // below
                        continue;

                    var start = segment.Start.x < segment.End.x ? segment.Start : segment.End;
                    var end = segment.Start.x < segment.End.x ? segment.End : segment.Start;

                    var x1 = start.x;
                    var y1 = start.y;
                    var x2 = end.x;
                    var y2 = end.y;

                    var d = Math.Abs(y2 - y1);

                    if (Math.Abs(d) < float.Epsilon)
                        continue;

                    // algorithm is based on fact that scan line is parallel to x-axis 
                    // so we calculate tangens of Beta angle, length of b-cathetus and 
                    // use length to get x of intersection point

                    float tanBeta = Math.Abs(x1 - x2)/d;

                    var b = Math.Abs(y1 - y);
                    var length = b*tanBeta;

                    var x = (int) (x1 + Math.Floor(length));

                    if (x >= size) x = size - 1;
                    if (x < 0) x = 0;

                    pointsBuffer.Add(x);
                }

                if (pointsBuffer.Count > 1)
                {
                    // TODO use optimized data structure
                    pointsBuffer.Sort();
                    //_pointsBuffer = _pointsBuffer.Distinct().ToList();

                    // merge connected ranges
                    for (int i = pointsBuffer.Count - 1; i > 0; i--)
                    {
                        if (i != 0 && pointsBuffer[i] == pointsBuffer[i - 1])
                        {
                            pointsBuffer.RemoveAt(i);
                            if (pointsBuffer.Count%2 != 0)
                                pointsBuffer.RemoveAt(--i);
                        }
                    }
                }

                // ignore single point
                if (pointsBuffer.Count == 1) continue;

                if (pointsBuffer.Count%2 != 0)
                    throw new InvalidOperationException(
                        "Bug in algorithm! We're expecting to have even number of intersection _pointsBuffer: (_pointsBuffer.Count % 2 != 0)");

                for (int i = 0; i < pointsBuffer.Count; i += 2)
                    Fill(gradient, y, pointsBuffer[i], pointsBuffer[i + 1]);

                pointsBuffer.Clear();
            }
        }

        /// <summary> Represents segment in 2D space. </summary>
        private struct Segment2D
        {
            public Vector2 Start;
            public Vector2 End;

            public Segment2D(Vector2 start, Vector2 end)
            {
                Start = start;
                End = end;
            }
        }

        #endregion
    }
}