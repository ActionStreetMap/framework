using System.Collections.Generic;
using ActionStreetMap.Core.Scene;
using ActionStreetMap.Explorer.Geometry;
using ActionStreetMap.Explorer.Geometry.Generators;
using ActionStreetMap.Explorer.Utils;
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

        public override List<MeshData> Build(Building building)
        {
            throw new System.NotImplementedException();
        }

        ///// <inheritdoc />
        //public override MeshData Build(Building building)
        //{
        //    var tuple = CircleUtils.GetCircle(building.Footprint);

        //    var radius = tuple.Item1 / 2;
        //    var center = tuple.Item2;

        //    // if offset is zero, than we will use hemisphere
        //    float offset = 0;
        //    if (building.RoofHeight > 0)
        //        offset = building.RoofHeight - radius;

        //    center.SetElevation(building.Elevation + building.MinHeight + building.Height + offset);

        //    var gradient = ResourceProvider.GetGradient(building.RoofColor);

        //    var meshData = ObjectPool.CreateMeshData();

        //    new IcoSphereGenerator(meshData)
        //        .SetCenter(new Vector3(center.X, center.Elevation, center.Y))
        //        .SetRadius(radius)
        //        .SetRecursionLevel(2)
        //        .SetGradient(gradient)
        //        .Build();

        //    meshData.MaterialKey = building.RoofMaterial;

        //    return meshData;
        //}
    }
}
