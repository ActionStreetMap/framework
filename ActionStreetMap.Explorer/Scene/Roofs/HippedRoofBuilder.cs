using System.Collections.Generic;
using System.Linq;
using ActionStreetMap.Core.Geometry;
using ActionStreetMap.Core.Geometry.StraightSkeleton;
using ActionStreetMap.Core.Geometry.Triangle.Geometry;
using ActionStreetMap.Core.Scene;
using ActionStreetMap.Explorer.Scene.Indices;
using ActionStreetMap.Unity.Wrappers;
using UnityEngine;

namespace ActionStreetMap.Explorer.Scene.Roofs
{
    /// <summary> Builds hipped roof. </summary>
    internal class HippedRoofBuilder : FlatRoofBuilder
    {
        /// <inheritdoc />
        public override string Name { get { return "hipped"; } }

        /// <inheritdoc />
        public override bool CanBuild(Building building) { return true; }

        /// <inheritdoc />
        public override List<MeshData> Build(Building building)
        {
            var roofHeight = building.RoofHeight;
            var roofOffset = building.Elevation + building.MinHeight + building.Height;

            var skeleton = SkeletonBuilder.Build(building.Footprint);
            var roofVertexCount = 0;
            foreach (var edgeResult in skeleton.Edges)
                roofVertexCount += (edgeResult.Polygon.Count - 2) * 12;

            var mesh = CreateMesh(building.Footprint);
            var floorCount = building.Levels;
            var floorVertexCount = mesh.Triangles.Count * 3 * 2 * floorCount;

            var vertexCount = roofVertexCount + floorVertexCount;
            var planeCount = skeleton.Edges.Count + floorCount;

            bool limitIsReached = false;
            if (vertexCount * 2 > Consts.MaxMeshSize)
            {
                vertexCount = roofVertexCount;
                planeCount = building.Footprint.Count;
                limitIsReached = true;
            }

            var meshIndex = new MultiPlaneMeshIndex(planeCount + floorCount, vertexCount);
            MeshData meshData = new MeshData(meshIndex, vertexCount);
            try
            {
                var roofGradient = ResourceProvider.GetGradient(building.RoofColor);
                foreach (var edge in skeleton.Edges)
                {
                    if (edge.Polygon.Count < 5)
                        HandleSimpleCase(meshData, meshIndex, roofGradient, skeleton, edge, roofOffset, roofHeight);
                    else
                        HandleComplexCase(meshData, meshIndex, roofGradient, skeleton, edge, roofOffset, roofHeight);
                }

                if (!limitIsReached)
                {
                    // attach floors
                    AttachFloors(new RoofContext()
                    {
                        Mesh = mesh,
                        MeshData = meshData,
                        MeshIndex = meshIndex,

                        Bottom = building.Elevation + building.MinHeight,
                        FloorCount = floorCount,
                        FloorHeight = building.Height/floorCount,
                        FloorFrontGradient = ResourceProvider.GetGradient(building.FloorFrontColor),
                        FloorBackGradient = ResourceProvider.GetGradient(building.FloorBackColor),

                        IsLastRoof = false
                    });
                    return new List<MeshData>(1) { meshData };
                }
                var meshDataList = BuildFloors(building, building.Levels, false);
                meshDataList.Add(meshData);
                return meshDataList;
            }
            catch
            {
                // NOTE straight skeleton may fail on some footprints.
                Trace.Warn("building.roof", Strings.RoofGenFailed, Name, building.Id.ToString());
                return base.Build(building);
            }
        }

        private void HandleSimpleCase(MeshData meshData, MultiPlaneMeshIndex meshIndex, GradientWrapper gradient,
            Skeleton skeleton, EdgeResult edge, float roofOffset, float roofHeight)
        {
            var polygon = edge.Polygon;
            var distances = skeleton.Distances;
            for (int i = 0; i <= polygon.Count - 2; i += 2)
            {
                var p0 = polygon[i];
                var p1 = polygon[i + 1];
                var p2 = polygon[i + 2 == polygon.Count ? 0 : i + 2];

                var v0 = new Vector3((float)p0.X, distances[p0] > 0 ? roofHeight + roofOffset : roofOffset,
                    (float)p0.Y);
                var v1 = new Vector3((float)p1.X, distances[p1] > 0 ? roofHeight + roofOffset : roofOffset,
                    (float)p1.Y);
                var v2 = new Vector3((float)p2.X, distances[p2] > 0 ? roofHeight + roofOffset : roofOffset,
                    (float)p2.Y);

                if (i == 0)
                    meshIndex.AddPlane(v0, v1, v2, meshData.NextIndex);
                AddTriangle(meshData, gradient, v0, v1, v2);
            }
        }

        private void HandleComplexCase(MeshData meshData, MultiPlaneMeshIndex meshIndex, GradientWrapper gradient,
            Skeleton skeleton, EdgeResult edge, float roofOffset, float roofHeight)
        {
            var polygon = edge.Polygon;
            var distances = skeleton.Distances;
            using (var trianglePolygon = new Polygon(polygon.Count, ObjectPool))
            {
                trianglePolygon.AddContour(polygon.Select(p => new Point(p.X, p.Y)).ToList());
                var mesh = trianglePolygon.Triangulate();
                bool planeIsAdded = false;
                foreach (var triangle in mesh.Triangles)
                {
                    var p0 = new Vector2d(triangle.vertices[0].X, triangle.vertices[0].Y);
                    var p1 = new Vector2d(triangle.vertices[1].X, triangle.vertices[1].Y);
                    var p2 = new Vector2d(triangle.vertices[2].X, triangle.vertices[2].Y);

                    double y;
                    if (distances.TryGetValue(p0, out y) && y > 0) y = roofHeight + roofOffset;
                    else y = roofOffset;
                    var v0 = new Vector3((float) p0.X, (float) y, (float) p0.Y);

                    if (distances.TryGetValue(p1, out y) && y > 0) y = roofHeight + roofOffset;
                    else y = roofOffset;
                    var v1 = new Vector3((float) p1.X, (float) y, (float) p1.Y);

                    if (distances.TryGetValue(p2, out y) && y > 0) y = roofHeight + roofOffset;
                    else y = roofOffset;
                    var v2 = new Vector3((float) p2.X, (float) y, (float) p2.Y);

                    if (!planeIsAdded)
                    {
                        meshIndex.AddPlane(v0, v1, v2, meshData.NextIndex);
                        planeIsAdded = true;
                    }
                    AddTriangle(meshData, gradient, v0, v1, v2);
                }
            }
        }
    }
}
