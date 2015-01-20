using System;
using System.Collections.Generic;
using ActionStreetMap.Core;
using ActionStreetMap.Core.Elevation;
using ActionStreetMap.Models.Geometry;
using ActionStreetMap.Models.Geometry.Polygons;
using ActionStreetMap.Infrastructure.Utilities;
using ActionStreetMap.Infrastructure.Dependencies;

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
        private readonly IObjectPool _objectPool;
        private HeightMap _heightMap;
        private int _size;
        private int _lastIndex;
        private float[,] _data;
        private float _ratio;

        /// <summary>
        ///     Creates instace of <see cref="HeightMapProcessor" />.
        /// </summary>
        /// <param name="objectPool"></param>
        [Dependency]
        public HeightMapProcessor(IObjectPool objectPool)
        {
            _objectPool = objectPool;
        }

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
        ///     Adjust height of line.
        /// </summary>
        /// <param name="start">Start point.</param>
        /// <param name="end">End point.</param>
        /// <param name="width">Width.</param>
        public void AdjustLine(MapPoint start, MapPoint end, float width)
        {
            var mapPointBuffer = _objectPool.NewList<MapPoint>();
            SetOffsetPoints(start, end, width, mapPointBuffer);

            var elevation = start.Elevation < end.Elevation ? start.Elevation : end.Elevation;

            SimpleScanLine.Fill(mapPointBuffer, _size, (scanline, s, e) =>
               Fill(scanline, s, e, elevation), _objectPool);

            _objectPool.Store(mapPointBuffer);
        }

        /// <summary>
        ///     Adjusts height of polygon.
        /// </summary>
        /// <param name="points">Polygon points.</param>
        /// <param name="elevation">Elevation.</param>
        public void AdjustPolygon(List<MapPoint> points, float elevation)
        {
            var polygonMapPointBuffer = _objectPool.NewList<MapPoint>();

            for (int i = 0; i < points.Count; i++)
            {
                var point = points[i];
                polygonMapPointBuffer.Add(GetHeightMapPoint(point.X, point.Y));
            }

            // NOTE: this is experimental - simple scan line is faster, but it was designed
            // to work with short road elements which are just rectangles
            if (PointUtils.IsConvex(polygonMapPointBuffer))
            {
                var scanLineBuffer = _objectPool.NewList<int>();
                SimpleScanLine.Fill(polygonMapPointBuffer, _size, (scanline, s, e) =>
                    Fill(scanline, s, e, elevation), _objectPool);
                _objectPool.Store(scanLineBuffer);
            }
            else
            {
                ScanLine.FillPolygon(polygonMapPointBuffer, (scanline, s, e) =>
                    Fill(scanline, s, e, elevation), _objectPool);
            }
            _objectPool.Store(polygonMapPointBuffer);
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

        private void SetOffsetPoints(MapPoint point1, MapPoint point2, float offset, List<MapPoint> mapPointBuffer)
        {
            float x1 = point1.X, x2 = point2.X, z1 = point1.Y, z2 = point2.Y;
            float l = (float)Math.Sqrt((x1 - x2) * (x1 - x2) + (z1 - z2) * (z1 - z2));

            var zOffset = (z2 - z1) / l;
            var xOffset = (x1 - x2) / l;
            
            mapPointBuffer.Add(GetHeightMapPoint(x1 - offset * zOffset, z1 - offset * xOffset));
            mapPointBuffer.Add(GetHeightMapPoint(x2 - offset * zOffset, z2 - offset * xOffset));
            mapPointBuffer.Add(GetHeightMapPoint(x2 + offset * zOffset, z2 + offset * xOffset));
            mapPointBuffer.Add(GetHeightMapPoint(x1 + offset * zOffset, z1 + offset * xOffset));           
        }
    }
}
