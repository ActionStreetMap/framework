using System.Collections.Generic;
using ActionStreetMap.Core.Tiling;
using ActionStreetMap.Core.Tiling.Models;
using ActionStreetMap.Infrastructure.Utilities;

namespace ActionStreetMap.Maps.Visitors
{
    internal class WayVisitor : ElementVisitor
    {
        // NOTE these hashsets contains all case sensitive strings as we don't want to convert case of matching string every time
        private static readonly HashSet<string> BooleanTrueValues = new HashSet<string> { "yes", "Yes", "YES", "true", "True", "TRUE", "1" };
        private static readonly HashSet<string> BooleanFalseValues = new HashSet<string> { "no", "No", "NO", "false", "False", "FALSE", "0" };

        /// <summary> Contains keys of osm tags which are markers of closed polygons ("area"). </summary>
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
        public WayVisitor(Tile tile, IModelLoader modelLoader, IObjectPool objectPool)
            : base(tile, modelLoader, objectPool)
        {
        }

        /// <inheritdoc />
        public override void VisitWay(Entities.Way way)
        {
            if (!IsArea(way.Tags))
            {
                ModelLoader.LoadWay(Tile, new Way
                {
                    Id = way.Id,
                    Points = way.Coordinates,
                    Tags = way.Tags
                });

                return;
            }

            if (!way.IsPolygon)
                return;
            {
                ModelLoader.LoadArea(Tile, new Area
                {
                    Id = way.Id,
                    Points = way.Coordinates,
                    Tags = way.Tags
                });
            }
        }

        private bool IsArea(TagCollection tags)
        {
            //return tags != null && tags.Any(tag => AreaKeys.Contains(tag.Key) && !tags.IsFalse(tag.Key));
            if (tags == null)
                return false;

            for (int i = 0; i < tags.Count; i++)
                if (AreaKeys.Contains(tags.KeyAt(i)) && !BooleanFalseValues.Contains(tags.ValueAt(i)))
                    return true;

            return false;
        }
    }
}