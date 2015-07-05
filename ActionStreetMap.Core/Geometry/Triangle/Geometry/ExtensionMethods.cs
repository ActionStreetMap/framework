using ActionStreetMap.Core.Geometry.Triangle.Meshing;
using ActionStreetMap.Core.Geometry.Triangle.Meshing.Algorithm;

namespace ActionStreetMap.Core.Geometry.Triangle.Geometry
{
    internal static class ExtensionMethods
    {
        #region IPolygon extensions

        /// <summary> Triangulates a polygon. </summary>
        public static Mesh Triangulate(this Polygon polygon)
        {
            return new Dwyer()
                .Triangulate(polygon.Points)
                .ApplyConstraints(polygon, null, null);
        }

        /// <summary> Triangulates a polygon, applying constraint options. </summary>
        /// <param name="polygon">Polygon.</param>
        /// <param name="options">Constraint options.</param>
        public static Mesh Triangulate(this Polygon polygon, ConstraintOptions options)
        {
            return new Dwyer()
              .Triangulate(polygon.Points)
              .ApplyConstraints(polygon, options, null);
        }

        /// <summary> Triangulates a polygon, applying quality options. </summary>
        /// <param name="polygon">Polygon.</param>
        /// <param name="quality">Quality options.</param>
        public static Mesh Triangulate(this Polygon polygon, QualityOptions quality)
        {
            return new Dwyer()
                .Triangulate(polygon.Points)
                .ApplyConstraints(polygon, null, quality);
        }

        /// <summary> Triangulates a polygon, applying quality and constraint options. </summary>
        /// <param name="polygon">Polygon.</param>
        /// <param name="options">Constraint options.</param>
        /// <param name="quality">Quality options.</param>
        public static Mesh Triangulate(this Polygon polygon, ConstraintOptions options, QualityOptions quality)
        {
            return new Dwyer()
             .Triangulate(polygon.Points)
             .ApplyConstraints(polygon, options, quality);
        }

        /// <summary> Triangulates a polygon, applying quality and constraint options. </summary>
        /// <param name="polygon">Polygon.</param>
        /// <param name="options">Constraint options.</param>
        /// <param name="quality">Quality options.</param>
        /// <param name="triangulator">The triangulation algorithm.</param>
        public static Mesh Triangulate(this Polygon polygon, ConstraintOptions options, QualityOptions quality,
            ITriangulator triangulator)
        {
            return triangulator
                .Triangulate(polygon.Points)
                .ApplyConstraints(polygon, options, quality);
        }

        #endregion
    }
}
