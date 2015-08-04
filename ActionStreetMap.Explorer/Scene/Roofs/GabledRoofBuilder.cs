using System;
using System.Collections.Generic;
using ActionStreetMap.Core;
using ActionStreetMap.Core.Geometry;
using ActionStreetMap.Core.Geometry.Utils;
using ActionStreetMap.Core.Scene;
using ActionStreetMap.Explorer.Utils;
using ActionStreetMap.Unity.Wrappers;
using UnityEngine;

namespace ActionStreetMap.Explorer.Scene.Roofs
{
    /// <summary>
    ///     Builds gabled roof.
    ///     See http://wiki.openstreetmap.org/wiki/Key:roof:shape#Roof
    /// </summary>
    internal class GabledRoofBuilder : RoofBuilder
    {
        /// <inheritdoc />
        public override string Name { get { return "gabled"; } }

        /// <inheritdoc />
        public override bool CanBuild(Building building) { return true; }

        public override List<MeshData> Build(Building building)
        {
            var gradient = ResourceProvider.GetGradient(building.RoofColor);
            var roofOffset = building.Elevation + building.MinHeight + building.Height;
            var roofHeight = roofOffset + building.RoofHeight;

            // 1. detect the longest segment
            float length;
            Vector2d longestStart;
            Vector2d longestEnd;
            GetLongestSegment(building.Footprint, out length, out longestStart, out longestEnd);

            // 2. get direction vector
            var ridgeDirection = (new Vector3((float) longestEnd.X, roofOffset, (float) longestEnd.Y) -
                                  new Vector3((float) longestStart.X, roofOffset, (float) longestStart.Y)).normalized;

            // 3. get centroid
            var centroidPoint = PolygonUtils.GetCentroid(building.Footprint);
            var centroidVector = new Vector3((float) centroidPoint.X, roofHeight, (float) centroidPoint.Y);

            // 4. get something like center line
            Vector3 p1 = centroidVector + length*length*ridgeDirection;
            Vector3 p2 = centroidVector - length*length*ridgeDirection;

            // 5. detect segments which have intesection with center line
            Vector2d first, second;
            int firstIndex, secondIndex;
            DetectIntersectSegments(building.Footprint, new Vector2d(p1.x, p1.z), new Vector2d(p2.x, p2.z),
                out first, out firstIndex, out second, out secondIndex);
            if (firstIndex == -1 || secondIndex == -1)
                throw new AlgorithmException(String.Format(Strings.GabledRoofGenFailed, building.Id));

            var meshData = new MeshData();
            meshData.Initialize((building.Footprint.Count - 1) * 6, true);

            // 6. process all segments and create vertices
            FillMeshData(meshData, gradient, roofOffset, roofHeight, building.Footprint,
                first, firstIndex, second, secondIndex);

            return new List<MeshData>()
            {
                meshData,
                BuildFloor(gradient, building.Footprint, building.Elevation + building.MinHeight),
            };
        }

        private void GetLongestSegment(List<Vector2d> footprint, out float maxLength,
            out Vector2d start, out Vector2d end)
        {
            maxLength = 0;
            start = default(Vector2d);
            end = default(Vector2d);
            for (int i = 0; i < footprint.Count; i++)
            {
                var s = footprint[i];
                var e = footprint[i == footprint.Count - 1 ? 0 : i + 1];

                var distance = s.DistanceTo(e);
                if (distance > maxLength)
                {
                    start = s;
                    end = e;
                    maxLength = (float) distance;
                }
            }
        }

        private void DetectIntersectSegments(List<Vector2d> footprint, Vector2d start, Vector2d end,
            out Vector2d first, out int firstIndex, out Vector2d second, out int secondIndex)
        {
            firstIndex = -1;
            secondIndex = -1;
            first = default(Vector2d);
            second = default(Vector2d);
            for (int i = 0; i < footprint.Count; i++)
            {
                var p1 = footprint[i];
                var p2 = footprint[i == footprint.Count - 1 ? 0 : i + 1];

                double r;
                if (Vector2dUtils.LineIntersects(start, end, p1, p2, out r))
                {
                    var intersectionPoint = Vector2dUtils.GetPointAlongLine(p1, p2, r);
                    if (firstIndex == -1)
                    {
                        firstIndex = i;
                        first = intersectionPoint;
                    }
                    else
                    {
                        secondIndex = i;
                        second = intersectionPoint;
                        break;
                    }
                }
            }
        }

        private void FillMeshData(MeshData meshData, GradientWrapper gradient, float roofOffset, float roofHeight,
            List<Vector2d> footprint, Vector2d first, int firstIndex, Vector2d second, int secondIndex)
        {
            var count = footprint.Count;
            int i = secondIndex;
            Vector2d startRidgePoint = default(Vector2d);
            do
            {
                var p1 = footprint[i];
                var p2 = footprint[i == footprint.Count - 1 ? 0 : i + 1];

                var nextIndex = i == count - 1 ? 0 : i + 1;
                // front faces
                if (i == firstIndex || i == secondIndex)
                {
                    startRidgePoint = i == firstIndex ? first : second;
                    var v0 = new Vector3((float) p1.X, roofOffset, (float) p1.Y);
                    var v1 = new Vector3((float)startRidgePoint.X, roofHeight, (float)startRidgePoint.Y);
                    var v2 = new Vector3((float)p2.X, roofOffset, (float)p2.Y);
                    var color = GradientUtils.GetColor(gradient, v0, 0.2f);
                    meshData.AddTriangle(v0, v1, v2, color);
                    i = nextIndex;
                    continue;
                }
                // side faces
                Vector2d endRidgePoint;
                if (nextIndex == firstIndex || nextIndex == secondIndex)
                    endRidgePoint = nextIndex == firstIndex ? first : second;
                else
                    endRidgePoint = Vector2dUtils.GetPointOnLine(first, second, p2);

                // add trapezoid
                {
                    var v0 = new Vector3((float)p1.X, roofOffset, (float)p1.Y);
                    var v1 = new Vector3((float)p2.X, roofOffset, (float)p2.Y);
                    var v2 = new Vector3((float)endRidgePoint.X, roofOffset, (float)endRidgePoint.Y);
                    var v3 = new Vector3((float)startRidgePoint.X, roofOffset, (float)startRidgePoint.Y);

                    var color1 = GradientUtils.GetColor(gradient, v0, 0.2f);
                    var color2 = GradientUtils.GetColor(gradient, v2, 0.2f);

                    meshData.AddTriangle(v0, v2, v1, color1);
                    meshData.AddTriangle(v2, v0, v3, color2);
                }
                startRidgePoint = endRidgePoint;
                i = nextIndex;
            } while (i != secondIndex);
        }
    }
}
