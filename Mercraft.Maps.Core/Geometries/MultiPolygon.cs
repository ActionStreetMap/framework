﻿using System.Collections.Generic;
using System.Linq;

namespace Mercraft.Maps.Core.Geometries
{
    /// <summary>
    /// A multi polygon, a collection of zero or more polygons.
    /// </summary>
    public class MultiPolygon : GeometryCollection
    {
        /// <summary>
        /// Creates a new multipolygon string.
        /// </summary>
        public MultiPolygon()
        {

        }

        /// <summary>
        /// Creates a new multipolygon string.
        /// </summary>
        /// <param name="polygons"></param>
        public MultiPolygon(params Polygon[] polygons)
            : base(polygons.Cast<Geometry>())
        {

        }

        /// <summary>
        /// Creates a new multipolygon string.
        /// </summary>
        /// <param name="polygons"></param>
        public MultiPolygon(IEnumerable<Polygon> polygons)
            : base(polygons.Cast<Geometry>())
        {

        }
    }
}
