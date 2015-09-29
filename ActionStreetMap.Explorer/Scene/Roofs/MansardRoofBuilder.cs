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
            var random = new System.Random((int) building.Id);
            var footprint = building.Footprint;
            var roofOffset = building.Elevation + building.MinHeight + building.Height;
            var roofHeight = roofOffset + building.RoofHeight;

            var offset = new ClipperOffset();
            offset.AddPath(footprint.Select(p => new IntPoint(p.X*Scale, p.Y*Scale)).ToList(),
                JoinType.jtMiter, EndType.etClosedPolygon);

            var result = new List<List<IntPoint>>();
            offset.Execute(ref result, random.NextFloat(1, 3)*-Scale);

            if (result.Count != 1 || result[0].Count != footprint.Count)
            {
                Trace.Warn(LogCategory, Strings.RoofGenFailed, Name, building.Id.ToString());
                return base.Build(building);
            }

            var topVertices = ObjectPool.NewList<Vector2d>(footprint.Count);
            double scale = Scale;

            foreach (var intPoint in result[0])
                topVertices.Add(new Vector2d(intPoint.X/scale, intPoint.Y/scale));
            // NOTE need reverse vertices
            topVertices.Reverse();

            var floorCount = building.Levels;
            var topMesh = CreateMesh(topVertices);
            var floorMesh = CreateMesh(footprint);

            var roofVertexCount = topMesh.Triangles.Count*3*2 + footprint.Count*2*12;
            var floorVertexCount = floorMesh.Triangles.Count*3*2*floorCount;

            var vertexCount = roofVertexCount + floorVertexCount;

            var planeCount = footprint.Count + floorCount + 1;

            bool limitIsReached = false;
            if (vertexCount*2 > Consts.MaxMeshSize)
            {
                vertexCount = roofVertexCount;
                planeCount = building.Footprint.Count + 1;
                limitIsReached = true;
            }

            var meshIndex = new MultiPlaneMeshIndex(planeCount, vertexCount);
            var meshData = new MeshData(meshIndex, vertexCount);

            Vector2 uv0, uv1, uv2;
            GetUV(building, out uv0, out uv1, out uv2);

            var roofGradient = CustomizationService.GetGradient(building.RoofColor);
            int index = FindStartIndex(topVertices[0], footprint);
            for (int i = 0; i < topVertices.Count; i++)
            {
                var top = topVertices[i];
                var bottom = footprint[(index + i)%footprint.Count];
                var nextTop = topVertices[(i + 1)%topVertices.Count];
                var nextBottom = footprint[(index + i + 1)%footprint.Count];

                var v0 = new Vector3((float) bottom.X, roofOffset, (float) bottom.Y);
                var v1 = new Vector3((float) nextBottom.X, roofOffset, (float) nextBottom.Y);
                var v2 = new Vector3((float) nextTop.X, roofHeight, (float) nextTop.Y);
                var v3 = new Vector3((float) top.X, roofHeight, (float) top.Y);

                meshIndex.AddPlane(v0, v1, v2, meshData.NextIndex);
                AddTriangle(meshData, v0, v2, v3, roofGradient, uv0, uv1, uv2);
                AddTriangle(meshData, v2, v0, v1, roofGradient, uv0, uv1, uv2);
            }
            ObjectPool.StoreList(topVertices);

            // Attach top reusing roof context object
            var context = new RoofContext()
            {
                Mesh = topMesh,
                MeshData = meshData,
                MeshIndex = meshIndex,

                Bottom = roofHeight,
                FloorCount = 1,
                FloorHeight = building.Height/floorCount,
                FloorFrontGradient = CustomizationService.GetGradient(building.FloorFrontColor),
                FloorBackGradient = CustomizationService.GetGradient(building.FloorBackColor),

                IsLastRoof = true,
                RoofFrontGradient = roofGradient,
                RoofBackGradient = roofGradient
            };
            AttachFloors(building, context);

            if (!limitIsReached)
            {
                context.Mesh = floorMesh;
                context.MeshData = meshData;
                context.Bottom = building.Elevation + building.MinHeight;
                context.FloorCount = floorCount;
                context.IsLastRoof = false;
                AttachFloors(building, context);
                return new List<MeshData>(1) {meshData};
            }

            var meshDataList = BuildFloors(building, building.Levels, false);
            meshDataList.Add(meshData);
            return meshDataList;
        }

        private int FindStartIndex(Vector2d firstPoint, List<Vector2d> footprint)
        {
            int index = 0;
            double minDistance = int.MaxValue;
            for (int i = 0; i < footprint.Count; i++)
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