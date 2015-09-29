using System;
using System.Collections.Generic;
using System.Linq;
using ActionStreetMap.Core.Geometry;
using ActionStreetMap.Core.Geometry.Triangle;
using ActionStreetMap.Core.Geometry.Triangle.Geometry;
using ActionStreetMap.Core.Geometry.Triangle.Meshing;
using ActionStreetMap.Core.Scene;
using ActionStreetMap.Core.Unity;
using ActionStreetMap.Explorer.Customization;
using ActionStreetMap.Explorer.Scene.Indices;
using ActionStreetMap.Explorer.Scene.Utils;
using ActionStreetMap.Explorer.Utils;
using ActionStreetMap.Infrastructure.Dependencies;
using ActionStreetMap.Infrastructure.Diagnostic;
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
        /// <param name="building"> Building. </param>
        /// <returns> True if bulder can build roof. </returns>
        bool CanBuild(Building building);

        /// <summary> Builds MeshData which contains information how to construct roof. </summary>
        /// <param name="building"> Building. </param>
        List<MeshData> Build(Building building);
    }

    /// <summary> Provides useful methods for different types of roof builders. </summary>
    internal abstract class RoofBuilder : IRoofBuilder
    {
        protected const string LogCategory = "building.roof";

        /// <inheritdoc />
        public abstract string Name { get; }

        /// <inheritdoc />
        public abstract bool CanBuild(Building building);

        /// <inheritdoc />
        public abstract List<MeshData> Build(Building building);

        [global::System.Reflection.Obfuscation(Exclude = true, Feature = "renaming")]
        [Dependency]
        public IObjectPool ObjectPool { get; set; }

        [global::System.Reflection.Obfuscation(Exclude = true, Feature = "renaming")]
        [Dependency]
        public CustomizationService CustomizationService { get; set; }

        [global::System.Reflection.Obfuscation(Exclude = true, Feature = "renaming")]
        [Dependency]
        public IGameObjectFactory GameObjectFactory { get; set; }

        [global::System.Reflection.Obfuscation(Exclude = true, Feature = "renaming")]
        [Dependency]
        public ITrace Trace { get; set; }

        /// <summary> Creates a mesh from given footprint using delaunay triangulation. </summary>
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
                    new QualityOptions { MaximumArea = 20 });
            }
        }

        /// <summary> Builds floor. </summary>
        protected void AttachFloors(Building building, RoofContext context)
        {
            var triCount = context.Mesh.Triangles.Count;
            var vertPerFloor = triCount*3;
            var halfVertCount = context.MeshData.Vertices.Length / 2;

            Vector2 uv0, uv1, uv2;
            GetUV(building, out uv0, out uv1, out uv2);

            var vertices = context.MeshData.Vertices;
            var triangles = context.MeshData.Triangles;
            var colors = context.MeshData.Colors;
            var uvs = context.MeshData.UVs;

            int lastFloorIndex = context.IsLastRoof ? context.FloorCount - 1 : -1;
            var startIndex = context.MeshData.NextIndex;
            int index = 0;
            foreach (var triangle in context.Mesh.Triangles)
            {
                var backSideIndex = index;
                for (int i = 0; i < 3; i++)
                {
                    var p = triangle.GetVertex(2 - i);
                    var v = new Vector3((float) p.X, 0, (float) p.Y);

                    float eleNoise = p.Type == VertexType.FreeVertex ? Noise.Perlin3D(v, 0.1f) : 0f;

                    var frontColor = GetColor(context.FloorFrontGradient, v);
                    var backColor = GetColor(context.FloorBackGradient, v);
                    // iterate over floors and set up the corresponding vertex
                    for (int k = 0; k < context.FloorCount; k++)
                    {
                        var floorOffsetIndex = startIndex + vertPerFloor*k;
                        var currentIndex = floorOffsetIndex + index;
                        v = new Vector3(v.x, context.Bottom + context.FloorHeight * k + eleNoise, v.z);

                        if (k == lastFloorIndex)
                        {
                            frontColor = GetColor(context.RoofFrontGradient, v);
                            backColor = GetColor(context.RoofBackGradient, v);
                        }

                        vertices[currentIndex] = v;
                        triangles[currentIndex] = currentIndex;
                        colors[currentIndex] = frontColor;

                        vertices[halfVertCount + currentIndex] = v;
                        triangles[halfVertCount + currentIndex] = halfVertCount + floorOffsetIndex
                            + backSideIndex + 2 - i;
                        colors[halfVertCount + currentIndex] = backColor;

                        uvs[currentIndex] = uvs[currentIndex + halfVertCount] =
                            i == 0 ? uv0 : i == 1 ? uv1 : uv2;
                    }
                    index++;
                }
            }

            // setup mesh index
            for (int i=0; i < context.FloorCount; i++)
            {
                var triIndex = startIndex + vertPerFloor*i;
                var v0 = vertices[triIndex];
                var v1 = vertices[triIndex + 1];
                var v2 = vertices[triIndex + 2];
                context.MeshIndex.AddPlane(v0, v1, v2, triIndex);
            }

            context.MeshData.NextIndex += vertPerFloor * context.FloorCount * 2;
        }

        /// <summary> Builds flat floors. </summary>
        protected List<MeshData> BuildFloors(Building building, int floorCount,
            bool lastFloorIsRoof, int extraMeshCount = 0)
        {
            var mesh = CreateMesh(building.Footprint);

            var floorHeight = building.Height / building.Levels;
            var bottomOffset = building.Elevation + building.MinHeight;

            var vertexPerFloor = mesh.Triangles.Count * 3 * 2;
            int vertexCount = vertexPerFloor * floorCount;

            int meshCount = 1;
            int floorsPerIteration = floorCount;
            var twoSizedMeshCount = vertexCount * 2;
            if (twoSizedMeshCount > Consts.MaxMeshSize)
            {
                Trace.Warn(LogCategory, Strings.MeshHasMaxVertexLimit, building.Id.ToString(),
                    twoSizedMeshCount.ToString());
                meshCount = (int)Math.Ceiling((double)twoSizedMeshCount / Consts.MaxMeshSize);
                floorsPerIteration = floorCount / meshCount;
            }

            var meshDataList = new List<MeshData>(meshCount + extraMeshCount);

            for (int i = 0; i < meshCount; i++)
            {
                var stepFloorCount = (i != meshCount - 1 || meshCount == 1)
                    ? floorsPerIteration
                    : floorsPerIteration + floorCount % meshCount;

                var stepVertexCount = vertexPerFloor * stepFloorCount;
                var stepBottomOffset = bottomOffset + i * (floorsPerIteration * floorHeight);

                var meshIndex = new MultiPlaneMeshIndex(stepFloorCount, stepVertexCount);
                var meshData = new MeshData(meshIndex, stepVertexCount);

                AttachFloors(building, new RoofContext()
                {
                    Mesh = mesh,
                    MeshData = meshData,
                    MeshIndex = meshIndex,

                    Bottom = stepBottomOffset,
                    FloorCount = stepFloorCount,
                    FloorHeight = floorHeight,
                    FloorFrontGradient = CustomizationService.GetGradient(building.FloorFrontColor),
                    FloorBackGradient = CustomizationService.GetGradient(building.FloorBackColor),

                    IsLastRoof = i == meshCount - 1 && lastFloorIsRoof,
                    RoofFrontGradient = CustomizationService.GetGradient(building.RoofColor),
                    RoofBackGradient = CustomizationService.GetGradient(building.RoofColor),
                });

                meshDataList.Add(meshData);
            }

            return meshDataList;
        }

        protected void AddTriangle(MeshData meshData, Vector3 v0, Vector3 v1, Vector3 v2,
            GradientWrapper gradient, Vector2 uv0, Vector2 uv1, Vector2 uv2)
        {
            var v01 = Vector3Utils.GetIntermediatePoint(v0, v1);
            var v12 = Vector3Utils.GetIntermediatePoint(v1, v2);
            var v02 = Vector3Utils.GetIntermediatePoint(v0, v2);

            var color = GetColor(gradient, v0);
            meshData.AddTriangle(v0, v01, v02, color, color, uv0, uv1, uv2);

            color = GetColor(gradient, v01);
            meshData.AddTriangle(v02, v01, v12, color, color, uv0, uv1, uv2);

            color = GetColor(gradient, v02);
            meshData.AddTriangle(v2, v02, v12, color, color, uv0, uv1, uv2);

            color = GetColor(gradient, v01);
            meshData.AddTriangle(v01, v1, v12, color, color, uv0, uv1, uv2);
        }

        /// <summary> Gets color value from gradient for given point. </summary>
        protected Color GetColor(GradientWrapper gradient, Vector3 point)
        {
            var value = (Noise.Perlin3D(point, .3f) + 1f) / 2f;
            return gradient.Evaluate(value);
        }

        /// <summary> Gets texture coordinates for given building. </summary>
        protected void GetUV(Building building, out Vector2 uv0, out Vector2 uv1, out Vector2 uv2)
        {
            var texture = CustomizationService
                .GetAtlas(building.RoofMaterial)
                .Get(building.RoofTexture)
                .Get((int) building.Id);

            uv0 = texture.Map(new Vector2(0, 0));
            uv1 = texture.Map(new Vector2(1, 0));
            uv2 = texture.Map(new Vector2(.5f, .5f));
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