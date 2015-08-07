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

            var elevation = building.Elevation + building.MinHeight + building.Height + offset;
            var gradient = ResourceProvider.GetGradient(building.RoofColor);

            var center3d = new Vector3((float)center.X, elevation, (float)center.Y);

            var sphereGen = new IcoSphereGenerator()
                .SetCenter(center3d)
                .SetRadius((float)radius)
                .SetRecursionLevel(2)
                .SetGradient(gradient);

            var meshData = new MeshData(new SphereMeshIndex((float)radius, center3d), 
                sphereGen.CalculateVertexCount());

            sphereGen.Build(meshData);

            return new List<MeshData>()
            {
                meshData,
                BuildFloor(gradient, building.Footprint, building.Elevation + building.MinHeight)
            };
        }
    }
}