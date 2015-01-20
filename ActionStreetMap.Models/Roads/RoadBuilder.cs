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
using ActionStreetMap.Infrastructure.Utilities;

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
        private readonly IObjectPool _objectPool;
        private readonly HeightMapProcessor _heightMapProcessor;

        /// <summary>
        ///     Creates RoadBuilder.
        /// </summary>
        /// <param name="resourceProvider">Resource provider.</param>
        /// /// <param name="objectPool"Object pool.</param>
        [Dependency]
        public RoadBuilder(IResourceProvider resourceProvider, IObjectPool objectPool, HeightMapProcessor heightMapProcessor)
        {
            _resourceProvider = resourceProvider;
            _objectPool = objectPool;
            _heightMapProcessor = heightMapProcessor;
        }

        /// <inheritdoc />
        public void Build(HeightMap heightMap, Road road, RoadStyle style)
        {
            var lineElements = road.Elements.Select(e => new LineElement(e.Points, e.Width)).ToList();

            using(var lineBuilder = new ThickLineBuilder(_objectPool, _heightMapProcessor))
                lineBuilder.Build(heightMap, lineElements, (p, t, u) => CreateRoadMesh(road, style, p, t, u));
        }

        /// <inheritdoc />
        public void Build(HeightMap heightMap, RoadJunction junction, RoadStyle style)
        {
            _heightMapProcessor.AdjustPolygon(junction.Polygon, junction.Center.Elevation);

            var buffer = _objectPool.NewList<int>();
            var polygonTriangles = Triangulator.Triangulate(junction.Polygon, buffer);
            _objectPool.Store(buffer);

            CreateJunctionMesh(junction, style, polygonTriangles);
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
        protected virtual void CreateJunctionMesh(RoadJunction junction, RoadStyle style, int[] polygonTriangles)
        {
            Mesh mesh = new Mesh();
            mesh.vertices = junction.Polygon.Select(p => new Vector3(p.X, p.Elevation, p.Y)).ToArray();
            mesh.triangles = polygonTriangles;
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
