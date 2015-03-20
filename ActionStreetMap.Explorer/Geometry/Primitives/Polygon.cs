using System.Collections.Generic;
using System.Linq;
using ActionStreetMap.Core;
using UnityEngine;

namespace ActionStreetMap.Explorer.Geometry.Primitives
{
    /// <summary> Represents polygon. </summary>
    public class Polygon
    {
        /// <summary> Gets polygon verticies. </summary>
        public Vector3[] Verticies { get; private set; }

        /// <summary> Gets polygon segments. </summary>
        public Segment[] Segments { get; private set; }

        /// <summary> Creates Polygon from MapPoints. </summary>
        /// <param name="verticies">Verticies.</param>
        public Polygon(IEnumerable<MapPoint> verticies)
        {
            Verticies = verticies.Select(v => new Vector3(v.X, v.Elevation, v.Y)).ToArray();
            ComputeSegments();
        }

        /// <summary> Creates Polygon from MapPoints and elevation. </summary>
        /// <param name="verticies">Verticies.</param>
        /// <param name="elevation">Elevation.</param>
        public Polygon(IEnumerable<MapPoint> verticies, float elevation)
        {
            Verticies = verticies.Select(v => new Vector3(v.X, elevation, v.Y)).ToArray();
            ComputeSegments();
        }

        /// <summary> Creates Polygon from Vector3. </summary>
        /// <param name="verticies">Verticies.</param>
        public Polygon(IEnumerable<Vector3> verticies)
        {
            Verticies = verticies.ToArray();
            ComputeSegments();
        }

        private void ComputeSegments()
        {
            // NOTE assume clock-wise order
            Segments = new Segment[Verticies.Length];
            for (int i = 0; i < Verticies.Length; i++)
            {
                var endIndex = i == Verticies.Length - 1 ? 0 : i + 1;
                Segments[i] = new Segment(Verticies[i], Verticies[endIndex]);
            }
        }
    }
}
