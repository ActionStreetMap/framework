using System.Collections.Generic;
using ActionStreetMap.Core.Scene.Details;
using ActionStreetMap.Explorer.Scene.Geometry.Primitives;
using ActionStreetMap.Infrastructure.Utilities;

namespace ActionStreetMap.Explorer.Scene.Terrain.Unity
{
    /// <summary>
    ///     Fills alphamap and detail maps of TerrainData using TerrainSettings provided.
    /// </summary>
    public class SurfaceBuilder
    {
        private readonly IObjectPool _objectPool;
        /// <summary>
        ///     Creates instance of <see cref="SurfaceBuilder"/>.
        /// </summary>
        /// <param name="objectPool"></param>
        public SurfaceBuilder(IObjectPool objectPool)
        {
            _objectPool = objectPool;
        }

        /// <summary>
        ///     Builds surface.
        /// </summary>
        /// <param name="settings">Terrain settings.</param>
        /// <param name="elements">Terrain elements.</param>
        /// <param name="splatMap">Splat map.</param>
        /// <param name="detailMapList">Detail map list.</param>
        public void Build(TerrainSettings settings, TerrainElement[] elements, float[, ,] splatMap, List<int[,]> detailMapList)
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
                TerrainScanLine.ScanAndFill(polygon, settings.Resolution, (line, start, end) =>
                    Fill(splatMap, detailMapList, line, start, end, elements[index].SplatIndex, elements[index].DetailIndex), 
                    _objectPool);
            });
            // NOTE if not thread safe, than use this instead:
            /*var polygons = elements.Select(e => new Polygon(e.Points)).ToArray();
            for (int i = 0; i < polygons.Length; i++)
            {
                var index = i;
                TerrainScanLine.ScanAndFill(polygons[index], settings.Resolution, (line, start, end) =>
                        Fill(splatMap, detailMapList, line, start, end, elements[index].SplatIndex, elements[index].DetailIndex));
            }*/
        }

        private static void Fill(float[, ,] map, List<int[,]> detailMapList, int line, int start, int end, 
            int splatIndex, int detailIndex)
        {
            var detailMap = detailIndex != Surface.DefaultDetailIndex ? detailMapList[detailIndex] : null;
            for (int i = start; i <= end; i++)
            {
                // TODO improve fill logic: which value to use for splat?
                map[i, line, splatIndex] = 0.5f;

                if (detailMap != null)
                    detailMap[i, line] = 1;
            }
        }
    }
}