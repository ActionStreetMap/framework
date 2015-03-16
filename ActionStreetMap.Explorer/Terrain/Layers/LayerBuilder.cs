using ActionStreetMap.Core;
using ActionStreetMap.Core.Polygons.Meshing.Iterators;
using ActionStreetMap.Core.Polygons.Tools;
using ActionStreetMap.Core.Terrain;
using ActionStreetMap.Core.Utilities;
using ActionStreetMap.Infrastructure.Dependencies;
using ActionStreetMap.Infrastructure.Diagnostic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ActionStreetMap.Explorer.Terrain.Layers
{
    internal interface ILayerBuilder
    {
        string Name { get; }

        void Build(MeshContext context, MeshRegion meshRegion);
    }

    internal abstract class LayerBuilder
    {
        [Dependency]
        public ITrace Trace { get; set; }

        [Dependency]
        public IElevationProvider ElevationProvider { get; set; }

        protected void BuildOffsetShape(MeshContext context, MeshRegion region, float deepLevel)
        {
            var vertices = context.Vertices;
            var triangles = context.Triangles;
            var colors = context.Colors;
            foreach (var contour in region.Contours)
            {
                var length = contour.Count;
                var vertOffset = vertices.Count;
                // vertices
                for (int i = 0; i < length; i++)
                {
                    var v2DIndex = i == (length - 1) ? 0 : i + 1;

                    var ele1 = ElevationProvider.GetElevation(new MapPoint((float)contour[i].X, (float)contour[i].Y));
                    var ele2 = ElevationProvider.GetElevation(new MapPoint((float)contour[v2DIndex].X, (float)contour[v2DIndex].Y));

                    vertices.Add(new Vector3((float)contour[i].X, ele1, (float)contour[i].Y));
                    vertices.Add(new Vector3((float)contour[v2DIndex].X, ele2, (float)contour[v2DIndex].Y));
                    vertices.Add(new Vector3((float)contour[v2DIndex].X, ele2 - deepLevel, (float)contour[v2DIndex].Y));
                    vertices.Add(new Vector3((float)contour[i].X, ele1 - deepLevel, (float)contour[i].Y));

                    colors.Add(Color.magenta);
                    colors.Add(Color.magenta);
                    colors.Add(Color.magenta);
                    colors.Add(Color.magenta);
                }

                // triangles
                for (int i = 0; i < length; i++)
                {
                    var vIndex = vertOffset + i * 4;
                    triangles.Add(vIndex);
                    triangles.Add(vIndex + 2);
                    triangles.Add(vIndex + 1);

                    triangles.Add(vIndex + 3);
                    triangles.Add(vIndex + 2);
                    triangles.Add(vIndex + 0);
                }
            }
        }
    }
}
