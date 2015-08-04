using System;
using System.Collections.Generic;
using System.Linq;
using ActionStreetMap.Core;
using ActionStreetMap.Core.Geometry.Clipping;
using ActionStreetMap.Core.Scene;
using ActionStreetMap.Explorer.Utils;
using ActionStreetMap.Unity.Wrappers;
using UnityEngine;

namespace ActionStreetMap.Explorer.Scene.Roofs
{
    /// <summary> Builds mansard roof. </summary>
    internal class MansardRoofBuilder : RoofBuilder
    {
        private const int Scale = 1000;
        private const int AttemptCount = 2;
        private const float Offset = 2;

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
                new IntPoint(p.X * Scale, p.Y * Scale)).ToList(),
                JoinType.jtMiter, EndType.etClosedLine);

            var result = new List<List<IntPoint>>();
            double OffsetSize = Offset * Scale;
            int attempts = 0;
            for (; attempts < AttemptCount; attempts++)
            {
                offset.Execute(ref result, OffsetSize);
                if (result.Count != 1 && result[0].Count != building.Footprint.Count)
                    OffsetSize -= (Offset/AttemptCount)*Scale;
                else
                    break;

            }

            if (attempts > AttemptCount)
                throw new AlgorithmException("Unable to do offset for mansard");


            /*var polygon = new Polygon(building.Footprint);
            var offset = 2f; // TODO

            if (Math.Abs(building.RoofHeight) < 0.01f)
            {
                var random = new System.Random((int) building.Id);
                building.RoofHeight = (float) random.NextDouble(0.5f, 3);
            }
            var gradient = ResourceProvider.GetGradient(building.RoofColor);*/

            //return CreateMeshDataList(building, polygon, gradient, offset);
            throw new NotImplementedException();
        }

        //private List<MeshData> CreateMeshDataList(Building building, Polygon polygon, 
        //    GradientWrapper gradient, float offset)
        //{
        //    var roofOffset = building.Elevation + building.MinHeight + building.Height;
        //    var roofTop = roofOffset + building.RoofHeight;

        //    var topFootprint = ObjectPool.NewList<MapPoint>(building.Footprint.Count);

        //    for (int i = 0; i < polygon.Segments.Length; i++)
        //    {
        //        var previous = i == 0 ? polygon.Segments.Length - 1 : i - 1;
        //        var nextIndex = i == polygon.Segments.Length - 1 ? 0 : i + 1;

        //        var segment1 = polygon.Segments[previous];
        //        var segment2 = polygon.Segments[i];
        //        var segment3 = polygon.Segments[nextIndex];

        //        var parallel1 = SegmentUtils.GetParallel(segment1, offset);
        //        var parallel2 = SegmentUtils.GetParallel(segment2, offset);
        //        var parallel3 = SegmentUtils.GetParallel(segment3, offset);

        //        var ip1 = SegmentUtils.IntersectionPoint(parallel1, parallel2);
        //        var ip2 = SegmentUtils.IntersectionPoint(parallel2, parallel3);

        //        // TODO check whether offset is correct for intersection

        //        var v0 = new Vector3(segment1.End.x, roofOffset, segment1.End.z);
        //        var v1 = new Vector3(ip1.x, roofTop, ip1.z);
        //        var v2 = new Vector3(segment2.End.x, roofOffset, segment2.End.z);
        //        var v3 = new Vector3(ip2.x, roofTop, ip2.z);

        //        meshData.AddTriangle(v0, v1, v2, GradientUtils.GetColor(gradient, v0, 0.2f));
        //        meshData.AddTriangle(v3, v2, v1, GradientUtils.GetColor(gradient, v3, 0.2f));

        //        topFootprint.Add(v1);
        //    }

        //    var topMeshData =  BuildFloor(gradient, topFootprint, roofTop);
        //    ObjectPool.StoreList(topFootprint);
        //}

        //private void AttachTopPart(MeshData meshData, GradientWrapper gradient, List<MapPoint> footprint, float roofTop)
        //{
        //    //var topPartIndecies = ObjectPool.NewList<int>();
        //   // Triangulator.Triangulate(footprint, topPartIndecies);

        //    for (int i = 0; i < topPartIndecies.Count; i+=3)
        //    {
        //        var p0 = footprint[topPartIndecies[i]];
        //        var v0 = new Vector3(p0.X, roofTop, p0.Y);

        //        var p1 = footprint[topPartIndecies[i+1]];
        //        var v1 = new Vector3(p1.X, roofTop, p1.Y);

        //        var p2 = footprint[topPartIndecies[i+2]];
        //        var v2 = new Vector3(p2.X, roofTop, p2.Y);

        //        meshData.AddTriangle(v0, v2, v1, GradientUtils.GetColor(gradient, v0, 0.2f));
        //    }

        //    //ObjectPool.StoreList(topPartIndecies);
        //}
    }
}