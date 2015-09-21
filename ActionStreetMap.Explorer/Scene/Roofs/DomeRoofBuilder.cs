using System.Collections.Generic;
using ActionStreetMap.Core.Geometry;
using ActionStreetMap.Core.Geometry.Utils;
using ActionStreetMap.Core.Scene;
using ActionStreetMap.Explorer.Scene.Generators;
using ActionStreetMap.Explorer.Scene.Indices;
using UnityEngine;

namespace ActionStreetMap.Explorer.Scene.Roofs
{
    /// <summary> Builds dome roof. </summary>
    internal class DomeRoofBuilder : RoofBuilder
    {
        /// <inheritdoc />
        public override string Name { get { return "dome"; } }

        /// <inheritdoc />
        public override bool CanBuild(Building building)
        {
            // we should use this builder only in case of dome type defined explicitly
            // cause we expect that footprint of building has the coresponding shape (circle)
            return building.RoofType == Name;
        }

        /// <inheritdoc />
        public override List<MeshData> Build(Building building)
        {
            Vector2d center;
            double radius;
            CircleUtils.GetCircle(building.Footprint, out radius, out center);

            var center3d = new Vector3((float)center.X,
                building.Elevation + building.MinHeight + building.Height, 
                (float)center.Y);

            var sphereGen = new IcoSphereGenerator()
                .SetCenter(center3d)
                .SetRadius((float)radius)
                .SetRecursionLevel(2)
                .IsSemiphere(true)
                .SetGradient(CustomizationService.GetGradient(building.RoofColor));

            var mesh = CreateMesh(building.Footprint);

            var floorCount = building.Levels;
            var floorVertexCount = mesh.Triangles.Count*3*2*floorCount;
            IMeshIndex floorMeshIndex = new MultiPlaneMeshIndex(building.Levels, floorVertexCount);

            var roofVertexCount = sphereGen.CalculateVertexCount();

            var vertexCount = roofVertexCount + floorVertexCount;

            bool limitIsReached = false;
            if (vertexCount * 2 > Consts.MaxMeshSize)
            {
                vertexCount = roofVertexCount;
                limitIsReached = true;
                floorMeshIndex = DummyMeshIndex.Default;
            }

            var meshIndex = new CompositeMeshIndex(2)
                .AddMeshIndex(new SphereMeshIndex((float) radius, center3d))
                .AddMeshIndex(floorMeshIndex);
            var meshData = new MeshData(meshIndex, vertexCount);

            // attach roof
            sphereGen.Build(meshData);

            if (!limitIsReached)
            {
                // attach floors
                AttachFloors(new RoofContext()
                {
                    Mesh = mesh,
                    MeshData = meshData,
                    MeshIndex = (MultiPlaneMeshIndex) floorMeshIndex,

                    Bottom = building.Elevation + building.MinHeight,
                    FloorCount = floorCount,
                    FloorHeight = building.Height/floorCount,
                    FloorFrontGradient = CustomizationService.GetGradient(building.FloorFrontColor),
                    FloorBackGradient = CustomizationService.GetGradient(building.FloorBackColor),

                    IsLastRoof = false
                });

                return new List<MeshData>(1) {meshData};
            }

            var meshDataList = BuildFloors(building, building.Levels, false);
            meshDataList.Add(meshData);
            return meshDataList;
        }
    }
}