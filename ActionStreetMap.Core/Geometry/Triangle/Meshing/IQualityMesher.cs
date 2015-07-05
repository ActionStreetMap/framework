using ActionStreetMap.Core.Geometry.Triangle.Geometry;

namespace ActionStreetMap.Core.Geometry.Triangle.Meshing
{
    /// <summary> Interface for polygon triangulation with quality constraints. </summary>
    public interface IQualityMesher
    {
        /// <summary> Triangulates a polygon, applying quality options. </summary>
        Mesh Triangulate(Polygon polygon, QualityOptions quality);

        /// <summary> Triangulates a polygon, applying quality and constraint options. </summary>
        Mesh Triangulate(Polygon polygon, ConstraintOptions options, QualityOptions quality);
    }
}