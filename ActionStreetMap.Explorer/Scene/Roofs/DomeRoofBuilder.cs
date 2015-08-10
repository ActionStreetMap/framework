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

            // if offset is zero, than we will use hemisphere
            float offset = 0;
            if (building.RoofHeight > 0)
                offset = building.RoofHeight - (float)radius;

            var center3d = new Vector3((float)center.X,
                building.Elevation + building.MinHeight + building.Height + offset,
                (float)center.Y);

            var sphereGen = new IcoSphereGenerator()
                .SetCenter(center3d)
                .SetRadius((float)radius)
                .SetRecursionLevel(2)
                .SetGradient(ResourceProvider.GetGradient(building.RoofColor));

            var mesh = CreateMesh(building.Footprint);

            var floorCount = building.Levels;
            var floorVertexCount = mesh.Triangles.Count*3*2*floorCount;
            var floorMeshIndex = new MultiPlaneMeshIndex(building.Levels, floorVertexCount);

            var vertexCount = sphereGen.CalculateVertexCount() + floorVertexCount;

            var meshIndex = new CompositeMeshIndex(2)
                .AddMeshIndex(new SphereMeshIndex((float) radius, center3d))
                .AddMeshIndex(floorMeshIndex);
            var meshData = new MeshData(meshIndex, vertexCount);

            // attach roof
            sphereGen.Build(meshData);
            // attach floors
            AttachFloors(new RoofContext()
            {
                Mesh = mesh,
                MeshData = meshData,
                MeshIndex = floorMeshIndex,

                Bottom = building.Elevation + building.MinHeight,
                FloorCount = floorCount,
                FloorHeight = building.Height / floorCount,
                FloorFrontGradient = ResourceProvider.GetGradient(building.FloorFrontColor),
                FloorBackGradient = ResourceProvider.GetGradient(building.FloorBackColor),

                IsLastRoof = false
            });

            return new List<MeshData>(1) { meshData };
        }
    }
}