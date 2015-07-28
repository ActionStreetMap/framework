using System.Collections.Generic;
using ActionStreetMap.Core.Scene;
using ActionStreetMap.Explorer.Infrastructure;
using ActionStreetMap.Explorer.Utils;
using ActionStreetMap.Infrastructure.Dependencies;
using UnityEngine;

namespace ActionStreetMap.Explorer.Scene.Facades
{
    /// <summary> Creates facade builder for simple facade. </summary>
    internal class EmptyFacadeBuilder: IFacadeBuilder
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
                .SetStepHeight(random.NextFloat(2.9f, 4.2f))
                .SetStepWidth(random.NextFloat(3f, 5f))
                .SetHeight(building.Height);

            var meshDataList = new List<MeshData>(footprint.Count);
            for (int i = 0; i < footprint.Count; i++)
            {
                var nextIndex = i == (footprint.Count - 1) ? 0 : i + 1;
                var start = footprint[i];
                var end = footprint[nextIndex];

                var startVector = new Vector3((float) start.X, elevation, (float) start.Y);
                var endVector = new Vector3((float) end.X, elevation, (float) end.Y);

                var vertCount = emptyWallBuilder.CalculateVertexCount(startVector, endVector);
                var meshData = new MeshData()
                {
                    Vertices = new Vector3[vertCount],
                    Triangles = new int[vertCount*2],
                    Colors = new Color[vertCount],
                };
                emptyWallBuilder
                    .SetMeshData(meshData)
                    .Build(startVector, endVector);

                meshDataList.Add(meshData);
            }

            return meshDataList;
        }
    }
}
