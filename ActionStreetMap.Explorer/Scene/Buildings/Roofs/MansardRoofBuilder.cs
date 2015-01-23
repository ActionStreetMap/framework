using System;
using System.Collections.Generic;
using System.Linq;
using ActionStreetMap.Core;
using ActionStreetMap.Core.Scene.Buildings;
using ActionStreetMap.Infrastructure.Dependencies;
using ActionStreetMap.Infrastructure.Utilities;
using ActionStreetMap.Explorer.Scene.Geometry;
using ActionStreetMap.Explorer.Scene.Geometry.Primitives;
using ActionStreetMap.Explorer.Scene.Geometry.Polygons;
using ActionStreetMap.Explorer.Scene.Utils;
using UnityEngine;

namespace ActionStreetMap.Explorer.Scene.Buildings.Roofs
{
    /// <summary>
    ///     Builds mansard roof.
    /// </summary>
    public class MansardRoofBuilder : IRoofBuilder
    {
        /// <inheritdoc />
        public string Name { get { return "mansard"; } }

        /// <inheritdoc />
        [Dependency]
        public IObjectPool ObjectPool { get; set; }

        /// <inheritdoc />
        public bool CanBuild(Building building)
        {
            // TODO improve checking of non standard buildings which 
            // cannot be used with mansard roof building
            
            // left condition: forced to use this builder from mapcss
            // right condition: in random scenario, prevent mansard to be used for buildings with many points in footprint
            return building.RoofType == Name ||  building.Footprint.Count < 8;
        }

        /// <inheritdoc />
        public MeshData Build(Building building, BuildingStyle style)
        {
            var polygon = new Polygon(building.Footprint);
            var offset = 2f; // TODO

            var roofHeight = building.RoofHeight > 0 ? building.RoofHeight : style.Roof.Height;
            var roofOffset = building.Elevation + building.MinHeight + building.Height;

            if (Math.Abs(roofHeight) < 0.01f)
            {
                var random = new System.Random((int)building.Id);
                roofHeight = (float) random.NextDouble(0.5f, 3);
            }

            var verticies3D = GetVerticies3D(polygon, offset, roofOffset, roofHeight);

            return new MeshData
            {
                Vertices = verticies3D,
                Triangles = GetTriangles(building.Footprint),
                UV = GetUV(building.Footprint, style),
                MaterialKey = style.Roof.Path
            };
        }

        private Vector3[] GetVerticies3D(Polygon polygon, float offset,
            float roofOffset, float roofHeight)
        {
            var verticies = new List<Vector3>(polygon.Verticies.Length * 2);
            var topVerticies = new List<Vector3>(polygon.Verticies.Length);
            var roofTop = roofOffset + roofHeight;

            for (int i = 0; i < polygon.Segments.Length; i++)
            {
                var previous = i == 0 ? polygon.Segments.Length - 1 : i - 1;
                var nextIndex = i == polygon.Segments.Length - 1 ? 0 : i + 1;

                var segment1 = polygon.Segments[previous];
                var segment2 = polygon.Segments[i];
                var segment3 = polygon.Segments[nextIndex];

                var parallel1 = SegmentUtils.GetParallel(segment1, offset);
                var parallel2 = SegmentUtils.GetParallel(segment2, offset);
                var parallel3 = SegmentUtils.GetParallel(segment3, offset);

                Vector3 ip1 = SegmentUtils.IntersectionPoint(parallel1, parallel2);
                Vector3 ip2 = SegmentUtils.IntersectionPoint(parallel2, parallel3);

                // TODO check whether offset is correct for intersection

                verticies.Add(new Vector3(segment1.End.x, roofOffset, segment1.End.z));
                verticies.Add(new Vector3(ip1.x, roofTop, ip1.z));

                verticies.Add(new Vector3(segment2.End.x, roofOffset, segment2.End.z));
                verticies.Add(new Vector3(ip2.x, roofTop, ip2.z));

                topVerticies.Add(new Vector3(ip1.x, roofTop, ip1.z));
            }
            verticies.AddRange(topVerticies);
            return verticies.ToArray();
        }

        private int[] GetTriangles(List<MapPoint> footprint)
        {
            var triangles = new List<int>();
            for (int i = 0; i < footprint.Count; i++)
            {
                var offset = i * 4;
                triangles.AddRange(new int[]
                {
                    0 + offset, 2 + offset, 1 + offset,
                    3 + offset, 1 + offset, 2 + offset
                });
            }

            var buffer = ObjectPool.NewList<int>();
            var topPartIndecies = Triangulator.Triangulate(footprint, buffer);
            ObjectPool.StoreList(buffer);

            var vertCount = footprint.Count * 4;
            triangles.AddRange(topPartIndecies.Select(i => i + vertCount));

            return triangles.ToArray();
        }

        private Vector2[] GetUV(List<MapPoint> footprint, BuildingStyle style)
        {
            var count = footprint.Count;
            var uv = new Vector2[count * 5];

            for (int i = 0; i < uv.Length; i++)
                uv[i] = style.Roof.FrontUvMap.RightUpper;

            return uv;
        }
    }
}
