using System.Collections.Generic;
using ActionStreetMap.Core.Geometry;

namespace ActionStreetMap.Core.Scene
{
    /// <summary> Represents terrain surface. </summary>
    public class Surface
    {
        /// <summary> Gradient key. </summary>
        public string GradientKey;

        /// <summary> Texture atlas. </summary>
        public string TextureAtlas;

        /// <summary> Texture key in atlas. </summary>
        public string TextureKey;

        /// <summary> Map points for this surcafe. </summary>
        public List<Vector2d> Points;

        /// <summary> Points for holes inside this surcafe. </summary>
        public List<List<Vector2d>> Holes;

        /// <summary> Elevation noise. </summary>
        internal float ElevationNoise;

        /// <summary> Color noise.  </summary>
        internal float ColorNoise;
    }
}
