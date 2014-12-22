using System;
using System.Collections.Generic;
using System.Linq;
using ActionStreetMap.Core;
using ActionStreetMap.Core.Scene;
using ActionStreetMap.Core.Scene.Models;
using ActionStreetMap.Infrastructure.Utilities;
using ActionStreetMap.Osm.Extensions;
using Way = ActionStreetMap.Osm.Entities.Way;

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
                ModelVisitor.VisitWay(new Core.Scene.Models.Way
                {
                    Id = way.Id,
                    Points = points,
                    Tags = GetMergedTags(way)
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
                    Tags = GetMergedTags(way)
                });
            }
        }

        /// <summary>
        ///     Returns merged tags. We cannot do this in place as Way can be reused
        ///     in case of cross tile processing logic is applied
        /// </summary>
        private Dictionary<string, string> GetMergedTags(Entities.Way way)
        {
           /* var tags = way.Tags == null
                ? new Dictionary<string, string>()
                : new Dictionary<string, string>(way.Tags);
            foreach (var node in way.Nodes)
            {
                if (node.Tags == null)
                    continue;
                foreach (var tag in node.Tags)
                {
                    if (IsMergeTag(tag) && !tags.ContainsKey(tag.Key))
                        tags.Add(tag.Key, tag.Value);
                }
            }
            return tags;*/
            throw new NotImplementedException();
        }

        private bool IsMergeTag(KeyValuePair<string, string> tag)
        {
            return tag.Key.StartsWith("addr:");
        }

        private bool IsArea(Dictionary<string, string> tags)
        {
            return tags != null && tags.Any(tag => AreaKeys.Contains(tag.Key) && !tags.IsFalse(tag.Key));
        }
    }
}