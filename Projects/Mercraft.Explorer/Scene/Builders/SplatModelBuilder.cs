﻿using System;
using System.Collections.Generic;
using Mercraft.Core;
using Mercraft.Core.Elevation;
using Mercraft.Core.MapCss.Domain;
using Mercraft.Core.Scene.Models;
using Mercraft.Core.Unity;
using Mercraft.Explorer.Helpers;
using Mercraft.Infrastructure.Dependencies;
using Mercraft.Models.Details;
using Mercraft.Models.Geometry;
using Mercraft.Models.Terrain;

namespace Mercraft.Explorer.Scene.Builders
{
    /// <summary>
    ///     Provides the way to process splat areas.
    /// </summary>
    public class SplatModelBuilder : ModelBuilder
    {
        private readonly ITerrainBuilder _terrainBuilder;

        /// <inheritdoc />
        public override string Name
        {
            get { return "splat"; }
        }

        /// <summary>
        ///     Creates TreeModelBuilder.
        /// </summary>
        [Dependency]
        public SplatModelBuilder(ITerrainBuilder terrainBuilder)
        {
            _terrainBuilder = terrainBuilder;
        }

        /// <inheritdoc />
        public override IGameObject BuildArea(Tile tile, Rule rule, Area area)
        {
            var points = ObjectPool.NewList<MapPoint>();
            PointUtils.GetPolygonPoints(tile.RelativeNullPoint, area.Points, points);
            _terrainBuilder.AddArea(new AreaSettings
            {
                ZIndex = rule.GetZIndex(),
                SplatIndex = rule.GetSplatIndex(),
                DetailIndex = rule.GetTerrainDetailIndex(),
                Points = points
            });

            if (rule.IsForest())
                GenerateTrees(points, (int) area.Id);

            return null;
        }

        private void GenerateTrees(List<MapPoint> points, int seed)
        {
            // triangulate polygon
            var triangles = PolygonUtils.Triangulate(points);
            
            var rnd = new Random(seed);
            // this cycle generate points inside each triangle
            // count of points is based on triangle area
            for (int i = 0; i < triangles.Length;)
            {
                // get triangle vertices
                var p1 = points[triangles[i++]];
                var p2 = points[triangles[i++]];
                var p3 = points[triangles[i++]];

                var area = TriangleUtils.GetTriangleArea(p1, p2, p3);
                var count = area / 200;
                for (int j = 0; j < count; j++)
                {
                    var point = TriangleUtils.GetRandomPoint(p1, p2, p3, rnd.NextDouble(), rnd.NextDouble());
                    _terrainBuilder.AddTree(new TreeDetail()
                    {
                        Point = point
                    });
                }
            }
        }
    }
}
