using System;
using System.Collections.Generic;
using ActionStreetMap.Core;
using ActionStreetMap.Core.Elevation;
using ActionStreetMap.Models.Geometry;
using ActionStreetMap.Models.Geometry.Polygons;

namespace ActionStreetMap.Models.Utils
{
    /// <summary>
    ///     Provides logic to adjust height of terrain.
    ///     NOTE this class has performance critical impact on rendering time if elevation is enabled. 
    ///     So readability was sacrified in favor of performance
    ///     NOT thread safe! 
    /// </summary>
    public class HeightMapProcessor
    {
        private HeightMap _heightMap;
        private int _size;
        private int _lastIndex;
        private float[,] _data;
        private float _ratio;

        // NOTE reusable buffers: they are made static to prevent allocations in multiply instances
        private static readonly List<MapPoint> MapPointBuffer = new List<MapPoint>(4);
        private static readonly List<MapPoint> PolygonMapPointBuffer = new List<MapPoint>(256);

        /// <summary>
        ///     Recycles heightmap instance to decrease memory allocation count.
        /// </summary>
        /// <param name="heightMap">Heightmap</param>
        public void Recycle(HeightMap heightMap)
        {
            _heightMap = heightMap;
            _data = _heightMap.Data;
            _ratio = heightMap.Size / heightMap.Resolution;
            _size = heightMap.Resolution;
            _lastIndex = _size - 1;
        }

        /// <summary>
        ///     Clear state.
        /// </summary>
        public void Clear()
        {
            _heightMap = null;
            _data = null;
        }

        /// <summary>
        ///     Adjust height of line
        /// </summary>
        /// <param name="start">Start point.</param>
        /// <param name="end">End point.</param>
        /// <param name="width">Width.</param>
        public void AdjustLine(MapPoint start, MapPoint end, float width)
        {
            SetOffsetPoints(start, end, width);

            var elevation = start.Elevation < end.Elevation ? start.Elevation : end.Elevation;

            // TODO this is not good  idea to create instance of anonymous class for every small road chunk
            SimpleScanLine.Fill(MapPointBuffer, _size, (scanline, s, e) =>
               Fill(scanline, s, e, elevation));
        }

        /// <summary>
        ///     Adjusts height of polygon
        /// </summary>
        /// <param name="points">Polygon points.</param>
        /// <param name="elevation">Elevation.</param>
        public void AdjustPolygon(List<MapPoint> points, float elevation)
        {
            PolygonMapPointBuffer.Clear();

            for (int i = 0; i < points.Count; i++)
            {
                var point = points[i];
                PolygonMapPointBuffer.Add(GetHeightMapPoint(point.X, point.Y));
            }

            // NOTE: this is experimental - simple scan line is faster, but it was designed
            // to work with short road elements which are just rectangles
            if (PointUtils.IsConvex(PolygonMapPointBuffer))
            {
                SimpleScanLine.Fill(PolygonMapPointBuffer, _size, (scanline, s, e) =>
                    Fill(scanline, s, e, elevation));
            }
            else
            {
                ScanLine.FillPolygon(PolygonMapPointBuffer, (scanline, s, e) =>
                    Fill(scanline, s, e, elevation));
            }
        }

        private void Fill(int line, int start, int end, float elevation)
        {
            if ((start > _lastIndex) || (end < 0) || line < 0 || line > _lastIndex)
                return;

            var s = start > _lastIndex ? _lastIndex : start;
            s = s < 0 ? 0 : s;

            var e = end > _lastIndex ? _lastIndex : end;

            for (int i = s; i <= e && i < _size; i++)
            {
                _data[line, i] = elevation;
            }

        }

        private MapPoint GetHeightMapPoint(float x, float y)
        {
            return new MapPoint(
            (int)Math.Ceiling((x - _heightMap.LeftBottomCorner.X) / _ratio),
            (int)Math.Ceiling(((y - _heightMap.LeftBottomCorner.Y) / _ratio)));
        }

        private void SetOffsetPoints(MapPoint point1, MapPoint point2, float offset)
        {
            float x1 = point1.X, x2 = point2.X, z1 = point1.Y, z2 = point2.Y;
            float l = (float)Math.Sqrt((x1 - x2) * (x1 - x2) + (z1 - z2) * (z1 - z2));

            var zOffset = (z2 - z1) / l;
            var xOffset = (x1 - x2) / l;
            
            MapPointBuffer.Clear();

            MapPointBuffer.Add(GetHeightMapPoint(x1 - offset * zOffset, z1 - offset * xOffset));
            MapPointBuffer.Add(GetHeightMapPoint(x2 - offset * zOffset, z2 - offset * xOffset));
            MapPointBuffer.Add(GetHeightMapPoint(x2 + offset * zOffset, z2 + offset * xOffset));
            MapPointBuffer.Add(GetHeightMapPoint(x1 + offset * zOffset, z1 + offset * xOffset));           
        }
    }
}
