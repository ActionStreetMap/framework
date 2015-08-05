using System.Collections.Generic;
using System.Linq;
using ActionStreetMap.Core.Geometry;
using ActionStreetMap.Core.Geometry.Clipping;
using ActionStreetMap.Core.Scene;
using ActionStreetMap.Explorer.Scene.Indices;
using ActionStreetMap.Explorer.Utils;
using UnityEngine;

namespace ActionStreetMap.Explorer.Scene.Roofs
{
    /// <summary> Builds mansard roof. </summary>
    internal class MansardRoofBuilder : FlatRoofBuilder
    {
        private const int Scale = 1000;

        /// <inheritdoc />
        public override string Name { get { return "mansard"; } }

        /// <inheritdoc />
        public override bool CanBuild(Building building)
        {
            return building.RoofType == Name;
        }

        /// <inheritdoc />
        public override List<MeshData> Build(Building building)
        {
            var random = new System.Random((int)building.Id);
            var gradient = ResourceProvider.GetGradient(building.RoofColor);
            var footprint = building.Footprint;
            var roofOffset = building.Elevation + building.MinHeight + building.Height;
            var roofHeight = roofOffset + building.RoofHeight;

            var offset = new ClipperOffset();
            offset.AddPath(footprint.Select(p => new IntPoint(p.X*Scale, p.Y*Scale)).ToList(),
                JoinType.jtMiter, EndType.etClosedPolygon);

            var result = new List<List<IntPoint>>();
            offset.Execute(ref result, random.NextFloat(1, 3) * -Scale);

            if (result.Count != 1 || result[0].Count != footprint.Count)
            {
                Trace.Warn("building.roof", Strings.RoofGenFailed, Name, building.Id.ToString());
                return base.Build(building);
            }
            var topVertices = ObjectPool.NewList<Vector2d>(footprint.Count);
            double scale = Scale;
            foreach (var intPoint in result[0])
                topVertices.Add(new Vector2d(intPoint.X / scale, intPoint.Y / scale));

            var toppart = BuildFloor(gradient, topVertices, roofHeight);

            var vertexCount = footprint.Count * 2 * 12;
            var meshIndex = new MultiPlaneMeshIndex(footprint.Count, vertexCount);
            var meshData = new MeshData(meshIndex);
            meshData.Initialize(vertexCount, true);

            int index = FindStartIndex(topVertices[0],footprint);
            for (int i = 0; i < topVertices.Count; i++)
            {
                var top = topVertices[i];
                var bottom = footprint[(index + i) % footprint.Count];
                var nextTop = topVertices[(i + 1)%topVertices.Count];
                var nextBottom = footprint[(index + i + 1) % footprint.Count];

                var v0 = new Vector3((float)bottom.X, roofOffset, (float)bottom.Y);
                var v1 = new Vector3((float)nextBottom.X, roofOffset, (float)nextBottom.Y);
                var v2 = new Vector3((float)nextTop.X, roofHeight, (float)nextTop.Y);
                var v3 = new Vector3((float)top.X, roofHeight, (float)top.Y);

                meshIndex.AddPlane(v0, v1, v2, meshData.NextIndex);
                AddTriangle(meshData, gradient, v0, v2, v1);
                AddTriangle(meshData, gradient, v2, v0, v3);
            }

            ObjectPool.StoreList(topVertices);

            return new List<MeshData>()
            {
                meshData,
                toppart,
                BuildFloor(gradient, building.Footprint, building.Elevation + building.MinHeight)
            };
        }

        private int FindStartIndex(Vector2d firstPoint, List<Vector2d> footprint)
        {
            int index = 0;
            double minDistance = int.MaxValue;
            for (int i = 0; i < footprint.Count - 1; i++)
            {
                var point = footprint[i];
                var distance = firstPoint.DistanceTo(point);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    index = i;
                }
            }
            return index;
        }
    }
}