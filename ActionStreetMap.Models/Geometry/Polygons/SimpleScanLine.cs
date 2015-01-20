using System;
using System.Collections.Generic;
using ActionStreetMap.Core;
using ActionStreetMap.Infrastructure.Utilities;

namespace ActionStreetMap.Models.Geometry.Polygons
{
    /// <summary>
    ///     Provides implementation of simplified scan line algorithm to process polygons.
    ///     This is optimized version for particular case of algorithm for road segments
    /// </summary>
    public class SimpleScanLine
    {
        /// <summary>
        ///     Fills polygon using fill action provided.
        /// </summary>
        /// <param name="mapPointBuffer">Polygon points.</param>
        /// <param name="size"></param>
        /// <param name="fillAction">Fill action.</param>
        public static void Fill(List<MapPoint> mapPointBuffer, int size, Action<int,int,int> fillAction, IObjectPool objectPool)
        {
            var scanLineStart = int.MaxValue;
            var scanLineEnd = int.MinValue;
            var scanListBuffer = objectPool.NewList<int>();
            // search start and end values
            for (int i = 0; i < mapPointBuffer.Count; i++)
            {
                var point = mapPointBuffer[i];

                if (point.Y <= 0 || point.Y >= size)
                    continue;

                if (scanLineEnd < point.Y)
                    scanLineEnd = (int)point.Y;
                if (scanLineStart > point.Y)
                    scanLineStart = (int)point.Y;
            }

            for (int z = scanLineStart; z <= scanLineEnd; z++)
            {
                for (int i = 0; i < mapPointBuffer.Count; i++)
                {
                    var currentIndex = i;
                    var nextIndex = i == mapPointBuffer.Count - 1 ? 0 : i + 1;

                    if ((mapPointBuffer[currentIndex].Y > z && mapPointBuffer[nextIndex].Y > z) || // above
                      (mapPointBuffer[currentIndex].Y < z && mapPointBuffer[nextIndex].Y < z)) // below
                        continue;

                    var start = mapPointBuffer[currentIndex].X < mapPointBuffer[nextIndex].X ?
                          mapPointBuffer[currentIndex]
                        : mapPointBuffer[nextIndex];

                    var end = mapPointBuffer[currentIndex].X < mapPointBuffer[nextIndex].X
                        ? mapPointBuffer[nextIndex]
                        : mapPointBuffer[currentIndex];

                    var x1 = start.X;
                    var y1 = start.Y;
                    var x2 = end.X;
                    var y2 = end.Y;

                    var d = Math.Abs(y2 - y1);

                    if (Math.Abs(d) < float.Epsilon)
                        continue;

                    float tanBeta = Math.Abs(x1 - x2) / d;

                    var b = Math.Abs(y1 - z);
                    var length = b * tanBeta;

                    var x = (int)(x1 + Math.Floor(length));

                    if (x >= size) x = size - 1;
                    if (x < 0) x = 0;

                    scanListBuffer.Add(x);
                }

                if (scanListBuffer.Count > 1)
                {
                    scanListBuffer.Sort();
                    fillAction(z, scanListBuffer[0], scanListBuffer[scanListBuffer.Count - 1]);
                }

                scanListBuffer.Clear();
            }
            objectPool.Store(scanListBuffer);
        }
    }
}
