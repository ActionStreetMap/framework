using System.Collections.Generic;
using ActionStreetMap.Core;
using ActionStreetMap.Core.Scene;
using ActionStreetMap.Core.Unity;
using ActionStreetMap.Explorer.Geometry;
using ActionStreetMap.Explorer.Geometry.Utils;
using ActionStreetMap.Explorer.Infrastructure;
using ActionStreetMap.Explorer.Utils;
using ActionStreetMap.Infrastructure.Dependencies;
using ActionStreetMap.Infrastructure.Utilities;
using ActionStreetMap.Unity.Wrappers;

namespace ActionStreetMap.Explorer.Scene.Roofs
{
    /// <summary> Defines roof builder logic. </summary>
    public interface IRoofBuilder
    {
        /// <summary> Gets name of roof builder. </summary>
        string Name { get; }

        /// <summary> Checks whether this builder can build roof of given building. </summary>
        /// <param name="building"> Building.</param>
        /// <returns> True if can build.</returns>
        bool CanBuild(Building building);

        /// <summary> Builds MeshData which contains information how to construct roof. </summary>
        /// <param name="building"> Building.</param>
        /// <returns> MeshData.</returns>
        MeshData Build(Building building);
    }

    public abstract class RoofBuilder : IRoofBuilder
    {
        /// <inheritdoc />
        public abstract string Name { get; }

        /// <inheritdoc />
        public abstract bool CanBuild(Building building);

        /// <inheritdoc />
        public abstract MeshData Build(Building building);

        [Dependency]
        public IObjectPool ObjectPool { get; set; }

        [Dependency]
        public IResourceProvider ResourceProvider { get; set; }

        [Dependency]
        public IGameObjectFactory GameObjectFactory { get; set; }

        /// <summary> Builds flat roof from footprint. </summary>
        protected void BuildFootprint(MeshData meshData, GradientWrapper gradient, List<MapPoint> footprint,
            float roofOffset, bool reversed = false)
        {
            var triangles = ObjectPool.NewList<int>();
            Triangulator.Triangulate(footprint, triangles);
            BuildFootprint(meshData, gradient, footprint, triangles, roofOffset, reversed);
        }

        /// <summary> Builds flat roof from footprint using provided triangles. </summary>
        protected void BuildFootprint(MeshData meshData, GradientWrapper gradient, List<MapPoint> footprint,
           List<int> triangles, float roofOffset, bool reversed = false)
        {
            if(reversed) triangles.Reverse();

            for (int i = 0; i < triangles.Count; i += 3)
            {
                var p0 = footprint[triangles[i]];
                var v0 = new MapPoint(p0.X, p0.Y, roofOffset);

                var p1 = footprint[triangles[i + 2]];
                var v1 = new MapPoint(p1.X, p1.Y, roofOffset);

                var p2 = footprint[triangles[i + 1]];
                var v2 = new MapPoint(p2.X, p2.Y, roofOffset);

                meshData.AddTriangle(v0, v1, v2, GradientUtils.GetColor(gradient, v0, 0.2f));
            }
        }
    }
}