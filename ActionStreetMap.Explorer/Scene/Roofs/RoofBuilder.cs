using System.Collections.Generic;
using System.Linq;
using ActionStreetMap.Core.Geometry;
using ActionStreetMap.Core.Geometry.Triangle;
using ActionStreetMap.Core.Geometry.Triangle.Geometry;
using ActionStreetMap.Core.Geometry.Triangle.Meshing;
using ActionStreetMap.Core.Scene;
using ActionStreetMap.Core.Unity;
using ActionStreetMap.Explorer.Infrastructure;
using ActionStreetMap.Explorer.Scene.Indices;
using ActionStreetMap.Explorer.Scene.Utils;
using ActionStreetMap.Explorer.Utils;
using ActionStreetMap.Infrastructure.Dependencies;
using ActionStreetMap.Infrastructure.Utilities;
using ActionStreetMap.Unity.Wrappers;
using UnityEngine;

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
        List<MeshData> Build(Building building);
    }

    internal abstract class RoofBuilder : IRoofBuilder
    {
        /// <inheritdoc />
        public abstract string Name { get; }

        /// <inheritdoc />
        public abstract bool CanBuild(Building building);

        /// <inheritdoc />
        public abstract List<MeshData> Build(Building building);

        [Dependency]
        public IObjectPool ObjectPool { get; set; }

        [Dependency]
        public IResourceProvider ResourceProvider { get; set; }

        [Dependency]
        public IGameObjectFactory GameObjectFactory { get; set; }

        protected Core.Geometry.Triangle.Mesh CreateMesh(List<Vector2d> footprint)
        {
            using (var polygon = new Polygon(footprint.Count, ObjectPool))
            {
                var list = ObjectPool.NewList<Point>(footprint.Count);
                list.AddRange(footprint.Select(point => new Point(point.X, point.Y)));
                polygon.AddContour(list);

                return polygon.Triangulate(
                    new ConstraintOptions
                    {
                        ConformingDelaunay = false,
                        SegmentSplitting = 0
                    },
                    new QualityOptions { MaximumArea = 9 });
            }
        }

        /// <summary> Builds floor </summary>
        protected void AttachFloors(RoofContext context)
        {
            var triCount = context.Mesh.Triangles.Count;
            var vertPerFloor = triCount*3;
            var halfVertCount = context.MeshData.Vertices.Length / 2;

            var vertices = context.MeshData.Vertices;
            var triangles = context.MeshData.Triangles;
            var colors = context.MeshData.Colors;

            var startIndex = context.MeshData.NextIndex;
            int index = 0;
            foreach (var triangle in context.Mesh.Triangles)
            {
                var backSideIndex = index;
                for (int i = 0; i < 3; i++)
                {
                    var p = triangle.GetVertex(2 - i);
                    var v = new Vector3((float) p.X, 0, (float) p.Y);

                    var eleNoise = p.Type == VertexType.FreeVertex ? Noise.Perlin3D(v, 0.1f) : 0f;

                    var frontColor = GetColor(context.FloorFrontGradient, v);
                    var backColor = GetColor(context.FloorBackGradient, v);
                    // iterate over floors and set up the corresponding vertex
                    for (int k = 0; k < context.FloorCount; k++)
                    {
                        var floorOffsetIndex = startIndex + vertPerFloor*k;
                        var currentIndex = floorOffsetIndex + index;
                        v = new Vector3(v.x, context.Bottom + context.FloorHeight * k + eleNoise, v.z);

                        vertices[currentIndex] = v;
                        triangles[currentIndex] = currentIndex;
                        colors[currentIndex] = frontColor;

                        vertices[halfVertCount + currentIndex] = v;
                        triangles[halfVertCount + currentIndex] = halfVertCount + floorOffsetIndex + backSideIndex + 2 - i;
                        colors[halfVertCount + currentIndex] = backColor;
                    }
                    index++;
                }
            }

            // setup mesh index
            for (int i = 0; i < context.FloorCount; i++)
            {
                var triIndex = startIndex + vertPerFloor * i;
                var v0 = vertices[triIndex];
                var v1 = vertices[triIndex + 1];
                var v2 = vertices[triIndex + 2];
                context.MeshIndex.AddPlane(v0, v1, v2, triIndex);
            }

            context.MeshData.NextIndex += vertPerFloor * context.FloorCount * 2;
        }

        protected void AddTriangle(MeshData meshData, GradientWrapper gradient,
            Vector3 v0, Vector3 v1, Vector3 v2)
        {
            var v01 = Vector3Utils.GetIntermediatePoint(v0, v1);
            var v12 = Vector3Utils.GetIntermediatePoint(v1, v2);
            var v02 = Vector3Utils.GetIntermediatePoint(v0, v2);

            var color = GetColor(gradient, v0);
            meshData.AddTriangle(v0, v01, v02, color, color);

            color = GetColor(gradient, v01);
            meshData.AddTriangle(v02, v01, v12, color, color);

            color = GetColor(gradient, v02);
            meshData.AddTriangle(v2, v02, v12, color, color);

            color = GetColor(gradient, v01);
            meshData.AddTriangle(v01, v1, v12, color, color);
        }

        protected Color GetColor(GradientWrapper gradient, Vector3 point)
        {
            var value = (Noise.Perlin3D(point, .3f) + 1f) / 2f;
            return gradient.Evaluate(value);
        }

        #region Nested classes

        /// <summary> Context for roof builer. </summary>
        public class RoofContext
        {
            // General mesh specific
            public Core.Geometry.Triangle.Mesh Mesh;
            public MeshData MeshData;
            public MultiPlaneMeshIndex MeshIndex;

            // Floor specific
            public int FloorCount;
            public float FloorHeight;
            public float Bottom;
            public GradientWrapper FloorFrontGradient;
            public GradientWrapper FloorBackGradient;

            // Consider last floor as specific
            public bool IsLastRoof;
            public GradientWrapper RoofFrontGradient;
            public GradientWrapper RoofBackGradient;
        }

        #endregion
    }
}