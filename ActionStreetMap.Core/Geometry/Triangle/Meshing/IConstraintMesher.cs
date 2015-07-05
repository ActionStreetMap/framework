using ActionStreetMap.Core.Geometry.Triangle.Geometry;

namespace ActionStreetMap.Core.Geometry.Triangle.Meshing
{
    /// <summary> Interface for polygon triangulation. </summary>
    public interface IConstraintMesher
    {
        /// <summary> Triangulates a polygon. </summary>
        Mesh Triangulate(Polygon polygon);

        /// <summary> Triangulates a polygon, applying constraint options. </summary>
        Mesh Triangulate(Polygon polygon, ConstraintOptions options);
    }
}