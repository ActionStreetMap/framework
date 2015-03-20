using System.Collections.Generic;
using ActionStreetMap.Core.MapCss.Domain;
using ActionStreetMap.Core.Polygons.Topology;
using ActionStreetMap.Core.Terrain;
using ActionStreetMap.Core.Tiling.Models;
using ActionStreetMap.Explorer.Helpers;
using ActionStreetMap.Explorer.Interactions;
using ActionStreetMap.Explorer.Scene;
using ActionStreetMap.Explorer.Scene.Builders;
using ActionStreetMap.Infrastructure.Dependencies;
using ActionStreetMap.Infrastructure.Reactive;
using UnityEngine;

namespace ActionStreetMap.Explorer.Terrain.Layers
{
    internal class WaterLayerBuilder : LayerBuilder
    {
        private readonly IResourceProvider _resourceProvider;
        private const string LogTag = "layer.water";
        private const float BottomLevelOffset = 5f;
        private const float WaterLevelOffset = 2.5f;

        public override string Name { get { return "water"; } }

        [Dependency]
        public WaterLayerBuilder(IResourceProvider resourceProvider)
        {
            _resourceProvider = resourceProvider;
        }

        public override void Build(MeshContext context, MeshRegion meshRegion)
        {
            const float colorNoiseFreq = 0.2f;
            var meshVertices = context.Data.Vertices;
            var hashMap = context.TriangleMap;

            // TODO allocate from pool with some size
            var vertices = new List<Vector3>();
            var triangles = new List<int>();
            var colors = new List<Color>();

            var gradient = ResourceProvider.GetGradient("water.simple");

            foreach (var region in meshRegion.FillRegions)
            {
                var point = region.Anchor;
                var start = (Triangle) context.Tree.Query(point.X, point.Y);
                if (start == null)
                {
                    Trace.Warn(LogTag, "Broken water region");
                    continue;
                }
                int count = 0;

                context.Iterator.Process(start, triangle =>
                {
                    var index = hashMap[triangle.GetHashCode()];

                    var p0 = meshVertices[index];
                    meshVertices[index] = new Vector3(p0.x, p0.y - BottomLevelOffset, p0.z);
                    vertices.Add(new Vector3(p0.x, p0.y - WaterLevelOffset, p0.z));

                    var p1 = meshVertices[index + 1];
                    meshVertices[index + 1] = new Vector3(p1.x, p1.y - BottomLevelOffset, p1.z);
                    vertices.Add(new Vector3(p1.x, p1.y - WaterLevelOffset, p1.z));

                    var p2 = meshVertices[index + 2];
                    meshVertices[index + 2] = new Vector3(p2.x, p2.y - BottomLevelOffset, p2.z);
                    vertices.Add(new Vector3(p2.x, p2.y - WaterLevelOffset, p2.z));

                    var color = GetColor(gradient, new Vector3(p0.x, p0.y, p0.y), colorNoiseFreq);
                    colors.Add(color);
                    colors.Add(color);
                    colors.Add(color);

                    triangles.Add(count);
                    triangles.Add(count + 2);
                    triangles.Add(count + 1);
                    count += 3;
                });
                Trace.Debug(LogTag, "Water region processed triangles: {0}", count /3);
            }

            BuildOffsetShape(context, meshRegion, ResourceProvider.GetGradient("canvas"), BottomLevelOffset);
            Scheduler.MainThread.Schedule(() => BuildWaterObject(context, vertices, triangles, colors));
        }

        private void BuildWaterObject(MeshContext context, List<Vector3> vertices, List<int> triangles, List<Color> colors)
        {
            var gameObject = new GameObject("water");
            // TODO attach to tile
            gameObject.transform.parent = context.Data.GameObject.GetComponent<GameObject>().transform;
            var meshData = new Mesh();
            meshData.vertices = vertices.ToArray();
            meshData.triangles = triangles.ToArray();
            meshData.colors = colors.ToArray();
            meshData.RecalculateNormals();

            // NOTE this script is too expensive to run!
            //gameObject.AddComponent<NoiseWaveBehavior>();
            gameObject.AddComponent<MeshRenderer>().material = context.Rule.GetMaterial("water", _resourceProvider);
            gameObject.AddComponent<MeshFilter>().mesh = meshData;
        }
    }
}