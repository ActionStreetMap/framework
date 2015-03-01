using System;
using System.Collections.Generic;
using ActionStreetMap.Core;
using ActionStreetMap.Core.Scene.Details;
using ActionStreetMap.Explorer.Scene.Geometry.Primitives;
using ActionStreetMap.Infrastructure.Utilities;

namespace ActionStreetMap.Explorer.Terrain
{
    /// <summary> Fills alphamap and detail maps of TerrainData using TerrainSettings provided. </summary>
    public class SurfaceBuilder
    {
        private readonly IObjectPool _objectPool;

        /// <summary> Creates instance of <see cref="SurfaceBuilder" />. </summary>
        /// <param name="objectPool"></param>
        public SurfaceBuilder(IObjectPool objectPool)
        {
            _objectPool = objectPool;
        }

        /// <summary> Builds surface. </summary>
        /// <param name="settings">Terrain settings.</param>
        /// <param name="elements">Terrain elements.</param>
        /// <param name="splatMap">Splat map.</param>
        /// <param name="detailMapList">Detail map list.</param>
        public void Build(TerrainSettings settings, TerrainElement[] elements, float[,,] splatMap,
            List<int[,]> detailMapList)
        {
            // TODO Performance optimization: do this during scanline logic?
            var resolution = settings.Resolution;
            splatMap.Parallel((start, end) =>
            {
                for (int x = start; x < end; x++)
                    for (int y = 0; y < resolution; y++)
                        splatMap[x, y, 0] = 1;
            });

            // NOTE experimental. Is it safe to do this async?
            elements.Parallel(index =>
            {
                var polygon = new Polygon(elements[index].Points);
                ScanAndFill(polygon, settings.Resolution, splatMap, detailMapList,
                    elements[index].SplatIndex, elements[index].DetailIndex, _objectPool);
            });
            // NOTE if not thread safe, than use this instead:
            /*var polygons = elements.Select(e => new Polygon(e.Points)).ToArray();
            for (int i = 0; i < polygons.Length; i++)
            {
                var index = i;
                ScanAndFill(polygon, settings.Resolution, splatMap, detailMapList, 
                    elements[index].SplatIndex, elements[index].DetailIndex, _objectPool);
            }*/
        }

        /// <summary> Custom version of Scanline algorithm to process terrain data. </summary>
        public static void ScanAndFill(Polygon polygon, int size, float[,,] map, List<int[,]> detailMapList,
            int splatIndex, int detailIndex, IObjectPool objectPool)
        {
            var pointsBuffer = objectPool.NewList<int>();
            for (int z = 0; z < size; z++)
            {
                foreach (var segment in polygon.Segments)
                {
                    if ((segment.Start.z > z && segment.End.z > z) || // above
                        (segment.Start.z < z && segment.End.z < z)) // below
                        continue;

                    var start = segment.Start.x < segment.End.x ? segment.Start : segment.End;
                    var end = segment.Start.x < segment.End.x ? segment.End : segment.Start;

                    var x1 = start.x;
                    var z1 = start.z;
                    var x2 = end.x;
                    var z2 = end.z;

                    var d = Math.Abs(z2 - z1);

                    if (Math.Abs(d) < float.Epsilon)
                        continue;

                    // algorithm is based on fact that scan line is parallel to x-axis 
                    // so we calculate tangens of Beta angle, length of b-cathetus and 
                    // use length to get x of intersection point

                    float tanBeta = Math.Abs(x1 - x2)/d;

                    var b = Math.Abs(z1 - z);
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
                    throw new AlgorithmException(Strings.TerrainScanLineAlgorithmBug);

                for (int i = 0; i < pointsBuffer.Count; i += 2)
                    Fill(map, detailMapList, splatIndex, detailIndex, z, pointsBuffer[i], pointsBuffer[i + 1]);

                pointsBuffer.Clear();
            }
            objectPool.StoreList(pointsBuffer);
        }

        private static void Fill(float[,,] map, List<int[,]> detailMapList, int splatIndex, int detailIndex,
            int line, int start, int end)
        {
            var detailMap = detailIndex != Surface.DefaultDetailIndex ? detailMapList[detailIndex] : null;
            for (int k = start; k <= end; k++)
            {
                // TODO improve fill logic: which value to use for splat?
                map[k, line, splatIndex] = 0.5f;

                if (detailMap != null) detailMap[k, line] = 1;
            }
        }
    }
}