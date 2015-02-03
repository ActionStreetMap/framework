using System;
using System.Collections.Generic;
using ActionStreetMap.Core;
using ActionStreetMap.Core.Elevation;
using ActionStreetMap.Explorer.Scene.Geometry;
using ActionStreetMap.Explorer.Scene.Geometry.Polygons;
using ActionStreetMap.Infrastructure.Utilities;
using ActionStreetMap.Infrastructure.Dependencies;

namespace ActionStreetMap.Explorer.Scene.Utils
{
    /// <summary>
    ///     Provides logic to adjust height of terrain.
    ///     NOTE this class has performance critical impact on rendering time if elevation is enabled. 
    /// </summary>
    public class HeightMapProcessor
    {
        private readonly IObjectPool _objectPool;

        /// <summary>
        ///     Creates instace of <see cref="HeightMapProcessor" />.
        /// </summary>
        /// <param name="objectPool"></param>
        [Dependency]
        public HeightMapProcessor(IObjectPool objectPool)
        {
            _objectPool = objectPool;
        }

        /// <summary> Adjust height of line. </summary>
        /// <param name="heightMap">Height map.</param>
        /// <param name="start">Start point.</param>
        /// <param name="end">End point.</param>
        /// <param name="width">Width.</param>
        public void AdjustLine(HeightMap heightMap, MapPoint start, MapPoint end, float width)
        {
            var mapPointBuffer = _objectPool.NewList<MapPoint>(4);
            var size = heightMap.Resolution;
            var data = heightMap.Data;
            var lastIndex = size - 1;

            SetOffsetPoints(heightMap, start, end, width, mapPointBuffer);

            var elevation = start.Elevation < end.Elevation ? start.Elevation : end.Elevation;

            SimpleScanLine.Fill(mapPointBuffer, size, (scanline, s, e) =>
               Fill(data, lastIndex, size, scanline, s, e, elevation), _objectPool);

            _objectPool.StoreList(mapPointBuffer);
        }

        /// <summary> Adjusts height of polygon. </summary>
        /// <param name="heightMap">Height map;</param>
        /// <param name="points">Polygon points.</param>
        /// <param name="elevation">Elevation.</param>
        public void AdjustPolygon(HeightMap heightMap, List<MapPoint> points, float elevation)
        {
            var polygonMapPointBuffer = _objectPool.NewList<MapPoint>();
            var size = heightMap.Resolution;
            var data = heightMap.Data;
            var lastIndex = size - 1;
            var ratio = heightMap.Size / heightMap.Resolution;

            for (int i = 0; i < points.Count; i++)
            {
                var point = points[i];
                polygonMapPointBuffer.Add(GetHeightMapPoint(heightMap, ratio, point.X, point.Y));
            }

            // NOTE: this is experimental - simple scan line is faster, but it was designed
            // to work with short road elements which are just rectangles
            if (PointUtils.IsConvex(polygonMapPointBuffer))
            {
                var scanLineBuffer = _objectPool.NewList<int>();
                SimpleScanLine.Fill(polygonMapPointBuffer, size, (scanline, s, e) =>
                    Fill(data, lastIndex, size, scanline, s, e, elevation), _objectPool);
                _objectPool.StoreList(scanLineBuffer);
            }
            else
            {
                ScanLine.FillPolygon(polygonMapPointBuffer, (scanline, s, e) =>
                    Fill(data, lastIndex, size, scanline, s, e, elevation), _objectPool);
            }
            _objectPool.StoreList(polygonMapPointBuffer);
        }

        private void Fill(float[,] _data, int _lastIndex, int _size, int line, int start, int end, float elevation)
        {
            if ((start > _lastIndex) || (end < 0) || line < 0 || line > _lastIndex)
                return;

            var s = start > _lastIndex ? _lastIndex : start;
            s = s < 0 ? 0 : s;

            var e = end > _lastIndex ? _lastIndex : end;

            for (int i = s; i <= e && i < _size; i++)
                _data[line, i] = elevation;
        }

        private MapPoint GetHeightMapPoint(HeightMap heightMap, float ratio, float x, float y)
        {
            return new MapPoint(
            (int)Math.Ceiling((x - heightMap.LeftBottomCorner.X) / ratio),
            (int)Math.Ceiling(((y - heightMap.LeftBottomCorner.Y) / ratio)));
        }

        private void SetOffsetPoints(HeightMap heightMap, MapPoint point1, MapPoint point2, float offset, List<MapPoint> mapPointBuffer)
        {
            float x1 = point1.X, x2 = point2.X, z1 = point1.Y, z2 = point2.Y;
            float l = (float)Math.Sqrt((x1 - x2) * (x1 - x2) + (z1 - z2) * (z1 - z2));

            var zOffset = (z2 - z1) / l;
            var xOffset = (x1 - x2) / l;

            float ratio = heightMap.Size / heightMap.Resolution;

            mapPointBuffer.Add(GetHeightMapPoint(heightMap, ratio, x1 - offset * zOffset, z1 - offset * xOffset));
            mapPointBuffer.Add(GetHeightMapPoint(heightMap, ratio, x2 - offset * zOffset, z2 - offset * xOffset));
            mapPointBuffer.Add(GetHeightMapPoint(heightMap, ratio, x2 + offset * zOffset, z2 + offset * xOffset));
            mapPointBuffer.Add(GetHeightMapPoint(heightMap, ratio, x1 + offset * zOffset, z1 + offset * xOffset));
        }
    }
}
