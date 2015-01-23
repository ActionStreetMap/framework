using System.Collections.Generic;
using System.Linq;
using ActionStreetMap.Models.Geometry.Primitives;
using ActionStreetMap.Models.Geometry.Polygons;
using ActionStreetMap.Core.Scene.Details;

namespace ActionStreetMap.Models.Terrain
{
    /// <summary>
    ///     Fills alphamap and detail maps of TerrainData using TerrainSettings provided.
    /// </summary>
    public class SurfaceBuilder
    {
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
            for (int x = 0; x < settings.Resolution; x++)
                for (int y = 0; y < settings.Resolution; y++)
                    splatMap[x, y, 0] = 1;

            var polygons = elements.Select(e => new Polygon(e.Points)).ToArray();
            for (int i = 0; i < polygons.Length; i++)
            {
                var index = i;
                TerrainScanLine.ScanAndFill(polygons[index], settings.Resolution, (line, start, end) =>
                        Fill(splatMap, detailMapList, line, start, end, elements[index].SplatIndex, elements[index].DetailIndex));
            }
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