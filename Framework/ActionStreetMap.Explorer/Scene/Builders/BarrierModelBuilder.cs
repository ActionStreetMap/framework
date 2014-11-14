﻿using System;
using System.Collections.Generic;
using ActionStreetMap.Core;
using ActionStreetMap.Core.MapCss.Domain;
using ActionStreetMap.Core.Scene.Models;
using ActionStreetMap.Core.Unity;
using ActionStreetMap.Models.Geometry;
using ActionStreetMap.Models.Geometry.ThickLine;
using ActionStreetMap.Explorer.Helpers;
using UnityEngine;

namespace ActionStreetMap.Explorer.Scene.Builders
{
    /// <summary>
    ///     Provides logic to build various barriers.
    /// </summary>
    public class BarrierModelBuilder: ModelBuilder
    {
        private readonly DimenLineBuilder _dimenLineBuilder = new DimenLineBuilder(2);
        private readonly List<LineElement> _lines = new List<LineElement>(1);

        /// <inheritdoc />
        public override string Name
        {
            get { return "barrier"; }
        }

        /// <inheritdoc />
        public override IGameObject BuildWay(Tile tile, Rule rule, Way way)
        {
            if (way.Points.Count < 2)
            {
                Trace.Warn(Strings.InvalidPolyline);
                return null;
            }

            var gameObjectWrapper = GameObjectFactory.CreateNew(String.Format("{0} {1}", Name, way));

            var points = ObjectPool.NewList<MapPoint>();
            PointUtils.FillHeight(tile.HeightMap, tile.RelativeNullPoint, way.Points, points);
            
            // reuse lines
            _lines.Clear();
            _lines.Add(new LineElement(points, rule.GetWidth()));

            _dimenLineBuilder.Height = rule.GetHeight();
            _dimenLineBuilder.Build(tile.HeightMap, _lines, 
                (p, t, u) => BuildObject(gameObjectWrapper, rule, p, t, u));
            _lines.Clear();

            ObjectPool.Store(points);

            return gameObjectWrapper;
        }

        /// <summary>
        ///     Process unity specific data.
        /// </summary>
        protected virtual void BuildObject(IGameObject gameObjectWrapper, Rule rule,
            List<Vector3> p, List<int> t, List<Vector2> u)
        {
            var gameObject = gameObjectWrapper.GetComponent<GameObject>();

            Mesh mesh = new Mesh();
            mesh.vertices = p.ToArray();
            mesh.triangles = t.ToArray();
            mesh.uv = u.ToArray();
            mesh.RecalculateNormals();

            var meshFilter = gameObject.AddComponent<MeshFilter>();
            meshFilter.mesh = mesh;

            var renderer = gameObject.AddComponent<MeshRenderer>();
            renderer.material = rule.GetMaterial(ResourceProvider);
            renderer.material.mainTexture = rule.GetTexture(ResourceProvider);
        }
    }
}
