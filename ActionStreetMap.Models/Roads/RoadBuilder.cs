using System;
using System.Collections.Generic;
using System.Linq;
using ActionStreetMap.Core.Elevation;
using ActionStreetMap.Core.Scene.World.Roads;
using ActionStreetMap.Infrastructure.Dependencies;
using ActionStreetMap.Models.Geometry;
using ActionStreetMap.Models.Geometry.ThickLine;
using ActionStreetMap.Models.Utils;
using UnityEngine;

namespace ActionStreetMap.Models.Roads
{
    /// <summary>
    ///     Provides the way to build road using road model.
    /// </summary>
    public interface IRoadBuilder
    {
        /// <summary>
        ///     Builds road.
        /// </summary>
        /// <param name="heightMap">Height map.</param>
        /// <param name="road">Road.</param>
        /// <param name="style">Style.</param>
        void Build(HeightMap heightMap, Road road, RoadStyle style);

        /// <summary>
        ///     Builds road junction.
        /// </summary>
        /// <param name="heightMap">Height map.</param>
        /// <param name="junction">Road junction.</param>
        /// <param name="style">Style.</param>
        void Build(HeightMap heightMap, RoadJunction junction, RoadStyle style);
    }

    /// <summary>
    ///     Defaul road builder.
    /// </summary>
    public class RoadBuilder : IRoadBuilder
    {
        private const string OsmTag = "osm.road";

        private readonly IResourceProvider _resourceProvider;
        private readonly HeightMapProcessor _heightMapProcessor = new HeightMapProcessor();
        private readonly ThickLineBuilder _lineBuilder = new ThickLineBuilder();

        /// <summary>
        ///     Creates RoadBuilder.
        /// </summary>
        /// <param name="resourceProvider">Resource provider.</param>
        [Dependency]
        public RoadBuilder(IResourceProvider resourceProvider)
        {
            _resourceProvider = resourceProvider;
        }

        /// <inheritdoc />
        public void Build(HeightMap heightMap, Road road, RoadStyle style)
        {
            var lineElements = road.Elements.Select(e => new LineElement(e.Points, e.Width)).ToList();
            _lineBuilder.Build(heightMap, lineElements, (p, t, u) => CreateRoadMesh(road, style, p, t, u));
        }

        /// <inheritdoc />
        public void Build(HeightMap heightMap, RoadJunction junction, RoadStyle style)
        {
            _heightMapProcessor.Recycle(heightMap);
            _heightMapProcessor.AdjustPolygon(junction.Polygon, junction.Center.Elevation);
            _heightMapProcessor.Clear();

            CreateJunctionMesh(junction, style);
        }

        /// <summary>
        ///     Creates unity's mesh for road.
        /// </summary>
        /// <param name="road">Road.</param>
        /// <param name="style">Style.</param>
        /// <param name="points">Points.</param>
        /// <param name="triangles">Triangles.</param>
        /// <param name="uv">UV.</param>
        protected virtual void CreateRoadMesh(Road road, RoadStyle style,
            List<Vector3> points, List<int> triangles, List<Vector2> uv)
        {
            Mesh mesh = new Mesh();
            mesh.vertices = points.ToArray();
            mesh.triangles = triangles.ToArray();
            mesh.uv = uv.ToArray();
            mesh.RecalculateNormals();

            var gameObject = road.GameObject.GetComponent<GameObject>();
            gameObject.isStatic = true;
            var meshFilter = gameObject.AddComponent<MeshFilter>();
            meshFilter.mesh = mesh;

            gameObject.AddComponent<MeshCollider>();
            gameObject.AddComponent<RoadBehaviors>().Road = road;
            gameObject.tag = OsmTag;

            gameObject.AddComponent<MeshRenderer>()
                .sharedMaterial = _resourceProvider.GetMatertial(style.Path);
        }

        /// <summary>
        ///     Creates unity's mesh for road junction.
        /// </summary>
        /// <param name="junction">Road junction.</param>
        /// <param name="style">Road style.</param>
        protected virtual void CreateJunctionMesh(RoadJunction junction, RoadStyle style)
        {
            Mesh mesh = new Mesh();
            mesh.vertices = junction.Polygon.Select(p => new Vector3(p.X, p.Elevation, p.Y)).ToArray();
            mesh.triangles = Triangulator.Triangulate(junction.Polygon);
            // TODO
            mesh.uv = junction.Polygon.Select(p => new Vector2()).ToArray();
            mesh.RecalculateNormals();

            var gameObject = junction.GameObject.GetComponent<GameObject>();
            gameObject.isStatic = true;
            var meshFilter = gameObject.AddComponent<MeshFilter>();
            meshFilter.mesh = mesh;

            gameObject.AddComponent<MeshCollider>();
            gameObject.AddComponent<JunctionBehavior>().Junction = junction;
            gameObject.tag = OsmTag;

            gameObject.AddComponent<MeshRenderer>()
                .sharedMaterial = _resourceProvider.GetMatertial(style.Path);
        }
    }
}
