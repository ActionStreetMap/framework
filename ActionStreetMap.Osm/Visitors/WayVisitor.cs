using System;
using System.Collections.Generic;
using System.Linq;
using ActionStreetMap.Core;
using ActionStreetMap.Core.Tiling;
using ActionStreetMap.Core.Tiling.Models;
using ActionStreetMap.Infrastructure.Utilities;
using ActionStreetMap.Osm.Extensions;

namespace ActionStreetMap.Osm.Visitors
{
    /// <inheritdoc />
    public class WayVisitor : ElementVisitor
    {
        /// <summary>
        ///     Contains keys of osm tags which are markers of closed polygons ("area")
        /// </summary>
        private static readonly HashSet<string> AreaKeys = new HashSet<string>
        {
            "building",
            "building:part",
            "landuse",
            "amenity",
            "harbour",
            "historic",
            "leisure",
            "man_made",
            "military",
            "natural",
            "office",
            "place",
            "power",
            "public_transport",
            "shop",
            "sport",
            "tourism",
            "waterway",
            "wetland",
            "water",
            "aeroway",
            "addr:housenumber",
            "addr:housename"
        };

        /// <inheritdoc />
        public WayVisitor(IModelVisitor modelVisitor, IObjectPool objectPool)
            : base(modelVisitor, objectPool)
        {
        }

        /// <inheritdoc />
        public override void VisitWay(Entities.Way way)
        {
            if (!IsArea(way.Tags))
            {
                var points = ObjectPool.NewList<GeoCoordinate>();
                way.FillPoints(points);
                ModelVisitor.VisitWay(new Core.Tiling.Models.Way
                {
                    Id = way.Id,
                    Points = points,
                    Tags = way.Tags
                });

                return;
            }

            if (!way.IsPolygon)
                return;
            {
                var points = ObjectPool.NewList<GeoCoordinate>();
                way.FillPoints(points);
                ModelVisitor.VisitArea(new Area
                {
                    Id = way.Id,
                    Points = points,
                    Tags = way.Tags
                });
            }
        }

        private bool IsArea(Dictionary<string, string> tags)
        {
            return tags != null && tags.Any(tag => AreaKeys.Contains(tag.Key) && !tags.IsFalse(tag.Key));
        }
    }
}