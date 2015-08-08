using System.Collections.Generic;
using ActionStreetMap.Core.Geometry;
using ActionStreetMap.Core.Scene;
using ActionStreetMap.Explorer.Infrastructure;
using ActionStreetMap.Explorer.Scene.Indices;
using ActionStreetMap.Explorer.Utils;
using ActionStreetMap.Infrastructure.Dependencies;
using UnityEngine;

namespace ActionStreetMap.Explorer.Scene.Facades
{
    /// <summary> Creates facade builder for simple facade. </summary>
    internal class EmptyFacadeBuilder : IFacadeBuilder
    {
        private readonly IResourceProvider _resourceProvider;

        /// <inheritdoc />
        public string Name { get { return "empty"; } }

        /// <summary> Creates instance of <see cref="EmptyFacadeBuilder"/>. </summary>
        [Dependency]
        public EmptyFacadeBuilder(IResourceProvider resourceProvider)
        {
            _resourceProvider = resourceProvider;
        }

        /// <inheritdoc />
        public List<MeshData> Build(Building building)
        {
            var random = new System.Random((int)building.Id);
            var footprint = building.Footprint;
            var elevation = building.MinHeight + building.Elevation;
            var gradient = _resourceProvider.GetGradient(building.FacadeColor);

            var emptyWallBuilder = new EmptyWallBuilder()
                .SetGradient(gradient)
                .SetMinHeight(elevation)
                .SetStepHeight(random.NextFloat(2f, 3f))
                .SetStepWidth(random.NextFloat(3f, 4f))
                .SetHeight(building.Height);

            var vertCount = CalculateVertexCount(emptyWallBuilder, building.Footprint, elevation);
            // TODO if vertCount * 2 exceedes Unity vertex limit, 
            // then produce split to multiply mesh data

            var meshIndex = new MultiPlaneMeshIndex(footprint.Count, vertCount);
            var meshData = new MeshData(meshIndex, vertCount);
            emptyWallBuilder.SetMeshData(meshData);

            for (int i = 0; i < footprint.Count; i++)
            {
                var nextIndex = i == (footprint.Count - 1) ? 0 : i + 1;
                var start = footprint[i];
                var end = footprint[nextIndex];

                var startVector = new Vector3((float)start.X, elevation, (float)start.Y);
                var endVector = new Vector3((float)end.X, elevation, (float)end.Y);
                var somePointOnPlane = new Vector3((float)end.X, elevation + 10, (float)end.Y);

                meshIndex.AddPlane(startVector, endVector, somePointOnPlane, meshData.NextIndex);
                emptyWallBuilder
                    .SetStartIndex(meshData.NextIndex)
                    .Build(startVector, endVector);
            }

            return new List<MeshData>(1) { meshData };
        }

        private int CalculateVertexCount(EmptyWallBuilder emptyWallBuilder,
            List<Vector2d> footprint, float elevation)
        {
            var count = 0;
            for (int i = 0; i < footprint.Count; i++)
            {
                var nextIndex = i == (footprint.Count - 1) ? 0 : i + 1;
                var start = footprint[i];
                var end = footprint[nextIndex];

                var startVector = new Vector3((float)start.X, elevation, (float)start.Y);
                var endVector = new Vector3((float)end.X, elevation, (float)end.Y);

                count += emptyWallBuilder.CalculateVertexCount(startVector, endVector);
            }

            return count;
        }
    }
}