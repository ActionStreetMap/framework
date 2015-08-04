using System;
using System.Collections.Generic;
using System.Linq;
using ActionStreetMap.Core.Geometry;
using ActionStreetMap.Core.Geometry.Clipping;
using ActionStreetMap.Core.Scene;

namespace ActionStreetMap.Explorer.Scene.Roofs
{
    /// <summary> Builds mansard roof. </summary>
    internal class MansardRoofBuilder : FlatRoofBuilder
    {
        private const int Scale = 1000;
        private const float Offset = 2 * Scale;

        /// <inheritdoc />
        public override string Name { get { return "mansard"; } }

        /// <inheritdoc />
        public override bool CanBuild(Building building)
        {
            return building.RoofType == Name;
        }

        /// <inheritdoc />
        public override List<MeshData> Build(Building building)
        {
            var offset = new ClipperOffset();
            offset.AddPath(building.Footprint.Select(p =>
                new IntPoint(p.X*Scale, p.Y*Scale)).ToList(),
                JoinType.jtMiter, EndType.etClosedPolygon);

            var result = new List<List<IntPoint>>();
            offset.Execute(ref result, Offset);

            if (result.Count != 1 || result[0].Count != building.Footprint.Count)
            {
                Trace.Warn("building.roof", Strings.MansardRoofGenFailed, building.Id.ToString());
                return base.Build(building);
            }

            var topVertices = ObjectPool.NewList<Vector2d>(building.Footprint.Count);

            double scale = Scale;
            foreach (var intPoint in result[0])
                topVertices.Add(new Vector2d(intPoint.X / scale, intPoint.Y / scale));


            throw new NotImplementedException();
        }
    }
}