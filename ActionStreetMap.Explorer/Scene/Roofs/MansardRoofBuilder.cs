using System;
using System.Collections.Generic;
using System.Linq;
using ActionStreetMap.Core;
using ActionStreetMap.Core.Scene;
using ActionStreetMap.Explorer.Geometry;
using ActionStreetMap.Explorer.Geometry.Primitives;
using ActionStreetMap.Explorer.Geometry.Utils;
using ActionStreetMap.Explorer.Utils;
using ActionStreetMap.Unity.Wrappers;
using UnityEngine;

namespace ActionStreetMap.Explorer.Scene.Roofs
{
    /// <summary> Builds mansard roof. </summary>
    public class MansardRoofBuilder : RoofBuilder
    {
        /// <inheritdoc />
        public override string Name { get { return "mansard"; } }

        /// <inheritdoc />
        public override bool CanBuild(Building building)
        {
            // TODO improve checking of non standard buildings which 
            // cannot be used with mansard roof building

            // left condition: forced to use this builder from mapcss
            // right condition: in random scenario, prevent mansard to be used for buildings with many points in footprint
            return building.RoofType == Name || building.Footprint.Count < 8;
        }

        /// <inheritdoc />
        public override MeshData Build(Building building)
        {
            var polygon = new Polygon(building.Footprint);
            var offset = 2f; // TODO

            if (Math.Abs(building.RoofHeight) < 0.01f)
            {
                var random = new System.Random((int) building.Id);
                building.RoofHeight = (float) random.NextDouble(0.5f, 3);
            }

            var meshData = new MeshData();
            var gradient = ResourceProvider.GetGradient(building.RoofColor);

            SetMeshData(building, polygon, meshData, gradient, offset);
            return meshData;
        }

        private void SetMeshData(Building building, Polygon polygon, MeshData meshData, GradientWrapper gradient,
            float offset)
        {
            var roofOffset = building.Elevation + building.MinHeight + building.Height;

            var vertices = new List<Vector3>(polygon.Verticies.Length*2);
            var colors = new List<Color>(polygon.Verticies.Length * 2);

            // TODO something wrong with this collection
            var topVertices = new List<Vector3>(polygon.Verticies.Length);
            var roofTop = roofOffset + building.RoofHeight;

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

                var v0 = new Vector3(segment1.End.x, roofOffset, segment1.End.z);
                vertices.Add(v0);
                colors.Add(GradientUtils.GetColor(gradient, v0, 0.2f));

                var v1 = new Vector3(ip1.x, roofTop, ip1.z);
                vertices.Add(v1);
                colors.Add(GradientUtils.GetColor(gradient, v1, 0.2f));

                var v2 = new Vector3(segment2.End.x, roofOffset, segment2.End.z);
                vertices.Add(v2);
                colors.Add(GradientUtils.GetColor(gradient, v2, 0.2f));

                var v3 = new Vector3(ip2.x, roofTop, ip2.z);
                vertices.Add(v3);
                colors.Add(GradientUtils.GetColor(gradient, v3, 0.2f));

                topVertices.Add(new Vector3(ip1.x, roofTop, ip1.z));

            }
            vertices.AddRange(topVertices);
            foreach (var topVertex in topVertices)
                colors.Add(GradientUtils.GetColor(gradient, topVertex, 0.2f));

            meshData.Vertices = vertices;
            meshData.Triangles = GetTriangles(building.Footprint);
            meshData.Colors = colors;
            meshData.MaterialKey = building.RoofMaterial;
        }

        private List<int> GetTriangles(List<MapPoint> footprint)
        {
            var triangles = new List<int>();
            for (int i = 0; i < footprint.Count; i++)
            {
                var offset = i*4;
                triangles.AddRange(new[]
                {
                    0 + offset, 2 + offset, 1 + offset,
                    3 + offset, 1 + offset, 2 + offset
                });
            }

            var topPartIndecies = ObjectPool.NewList<int>();
            Triangulator.Triangulate(footprint, topPartIndecies);

            var vertCount = footprint.Count*4;
            triangles.AddRange(topPartIndecies.Select(i => i + vertCount));

            return triangles;
        }
    }
}