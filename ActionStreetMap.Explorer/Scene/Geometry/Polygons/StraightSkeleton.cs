using System.Collections.Generic;
using ActionStreetMap.Core;
using ActionStreetMap.Infrastructure.Primitives;
using UnityEngine;

namespace ActionStreetMap.Explorer.Scene.Geometry.Polygons
{
    /// <summary>
    ///     Compute skeleton. Reused algorithm found in Internet.
    ///     TODO: optimize memory allocations.
    /// </summary>
    internal class StraightSkeleton
    {
        public static MutableTuple<List<Vector2>, List<Vector2>> Calculate(List<MapPoint> polygon)
        {
            var context = new SkeletonContext();
            context.OriginalPoints = polygon;
            context.Count = polygon.Count;
            for (int i = polygon.Count - 1; i >= 0; i--)
                context.UsePoints.Add(new Vector2(polygon[i].X, polygon[i].Y));

            for (int i = 0; i < context.Count; i++)
            {
                //prepopulate the sodding lists
                context.PointAngles.Add(0);
                context.HypsForward.Add(float.PositiveInfinity);
                context.HypsBackward.Add(float.PositiveInfinity);
            }

            for (int i = 1; i < context.Count + 1; i++)
                CalculateAngles(context, i);


            for (int i = 0; i < context.Count; i++)
                CalculateHypotenuse(context, i);

            CalculateTriangles(context);

            return new MutableTuple<List<Vector2>, List<Vector2>>(context.ResultPoints, context.InteriorPoints);
        }

        private static void CalculateTriangles(SkeletonContext context)
        {
            int it = 0;
            int numberOfSkeletalPoints = 0;
            int totalOfSkeletalPoints = context.Count;
            while (numberOfSkeletalPoints < totalOfSkeletalPoints - 3)
            {
                //select shortest point
                int pointIndex = SmallestHypIndex(context);
                if (pointIndex == -1)
                    break;
                int lastPointIndex = pointIndex > 0 ? pointIndex - 1 : context.Count - 1;
                int lastPointIndexB = pointIndex > 1 ? pointIndex - 2 : pointIndex + (context.Count - 2);
                int nextPointIndex = (pointIndex + 1)%context.Count;
                int nextPointIndexB = (pointIndex + 2)%context.Count;

                Vector2 point = context.UsePoints[pointIndex];
                float pointAngle = context.PointAngles[pointIndex];
                float hypLengthF = context.HypsForward[pointIndex];
                float hypLengthB = context.HypsBackward[pointIndex];
                bool forward = hypLengthF < hypLengthB; //if the next point has a smaller size - delete forward
                float useHypLength = forward ? context.HypsForward[pointIndex] : context.HypsBackward[pointIndex];

                Vector2 pointDir = new Vector2(Mathf.Sin(pointAngle), Mathf.Cos(pointAngle))*useHypLength;
                Vector2 newPoint = point + pointDir;

                int indexA = forward ? lastPointIndex : lastPointIndexB;
                int indexB = forward ? pointIndex : lastPointIndex;
                int indexC = forward ? nextPointIndex : pointIndex;
                int indexD = forward ? nextPointIndexB : nextPointIndex;

                Vector2 pointA = context.UsePoints[indexA];
                Vector2 pointB = context.UsePoints[indexB];
                Vector2 pointC = context.UsePoints[indexC];
                Vector2 pointD = context.UsePoints[indexD];

                context.PointAngles[indexB] = Mathf.LerpAngle(context.PointAngles[indexB], context.PointAngles[indexC],
                    0.5f);

                if (indexB < indexC)
                {
                    context.UsePoints.RemoveAt(indexB);
                    context.UsePoints.RemoveAt(indexB);
                    context.HypsForward.RemoveAt(indexB);
                    context.HypsForward.RemoveAt(indexB);
                    context.HypsBackward.RemoveAt(indexB);
                    context.HypsBackward.RemoveAt(indexB);
                    context.PointAngles.RemoveAt(indexB);
                    context.PointAngles.RemoveAt(indexB);
                }
                else
                {
                    context.UsePoints.RemoveAt(indexC);
                    context.HypsForward.RemoveAt(indexC);
                    context.HypsBackward.RemoveAt(indexC);
                    context.PointAngles.RemoveAt(indexC);
                    indexB--;
                    context.UsePoints.RemoveAt(indexB);
                    context.HypsForward.RemoveAt(indexB);
                    context.HypsBackward.RemoveAt(indexB);
                    context.PointAngles.RemoveAt(indexB);
                }

                context.UsePoints.Insert(indexB, newPoint);
                context.InteriorPoints.Add(newPoint);
                context.HypsForward.Insert(indexB, float.PositiveInfinity);
                context.HypsBackward.Insert(indexB, float.PositiveInfinity);
                context.PointAngles.Insert(indexB, 0);
                context.Count--;

                totalOfSkeletalPoints++;
                numberOfSkeletalPoints += 2;

                if (indexB < indexA) indexA--;
                if (indexB < indexC) indexC--;
                if (indexB < indexD) indexD--;


                //add new point created
                if (context.Count > 3)
                {
                    CalculateAngles(context, indexB);
                    CalculateHypotenuse(context, indexA);
                    CalculateHypotenuse(context, indexB);
                    CalculateHypotenuse(context, indexC);
                    CalculateHypotenuse(context, indexD);
                }
                //re calculate points affected

                context.ResultPoints.Add(pointA);
                context.ResultPoints.Add(pointB);
                context.ResultPoints.Add(newPoint);

                context.ResultPoints.Add(pointB);
                context.ResultPoints.Add(pointC);
                context.ResultPoints.Add(newPoint);

                context.ResultPoints.Add(pointC);
                context.ResultPoints.Add(pointD);
                context.ResultPoints.Add(newPoint);

                it++;
                if (it > 32000)
                {
                    break;
                }
            }

            context.ResultPoints.Add(context.UsePoints[0]);
            context.ResultPoints.Add(context.UsePoints[1]);
            context.ResultPoints.Add(context.UsePoints[2]);
        }

        private static int SmallestHypIndex(SkeletonContext context)
        {
            float smallestHyp = float.PositiveInfinity;
            int smallestHypIndex = -1;
            for (int i = 0; i < context.Count; i++)
            {
                float hypA = context.HypsForward[i];
                float hypB = context.HypsBackward[i];
                float hypOtherA = context.HypsBackward[(i + 1)%context.Count];
                float hypOtherB = context.HypsForward[(i > 0) ? i - 1 : context.Count - 1];

                if (hypA < smallestHyp && !float.IsPositiveInfinity(hypOtherA))
                {
                    smallestHyp = hypA;
                    smallestHypIndex = i;
                }
                if (hypB < smallestHyp && !float.IsPositiveInfinity(hypOtherB))
                {
                    smallestHyp = hypB;
                    smallestHypIndex = i;
                }
            }
            return smallestHypIndex;
        }

        private static void CalculateAngles(SkeletonContext context, int pointIndex)
        {
            int ia = (pointIndex > 0) ? pointIndex - 1 : context.Count - 1;
            int ib = pointIndex%context.Count;
            int ic = (pointIndex + 1)%context.Count;

            Vector2 a = context.UsePoints[ia];
            Vector2 b = context.UsePoints[ib];
            Vector2 c = context.UsePoints[ic];

            Vector2 dirA = a - b;
            Vector2 dirB = c - b;

            float tarad = Vector2.Angle(Vector2.up, dirA);
            if (tarad < 0) tarad += 360;
            tarad = tarad*Mathf.Deg2Rad*Mathf.Sign(Vector2.Dot(Vector2.right, dirA));

            Vector2 aDir90 = new Vector2(Mathf.Sin(tarad + Mathf.PI/2), Mathf.Cos(tarad + Mathf.PI/2));
            //for use to determine reflex angle using Dot

            float tbrad = Vector2.Angle(Vector2.up, dirB);
            if (tbrad < 0) tbrad += 360;
            tbrad = tbrad*Mathf.Deg2Rad*Mathf.Sign(Vector2.Dot(Vector2.right, dirB));
            Vector2 bDir = new Vector2(Mathf.Sin(tbrad), Mathf.Cos(tbrad));

            float reflex = Vector2.Dot(aDir90, bDir) > 0 ? 0 : -1;
            float drad = (Mathf.LerpAngle(tarad*Mathf.Rad2Deg, tbrad*Mathf.Rad2Deg, 0.5f) + (reflex*180))*
                         (Mathf.Deg2Rad);

            context.PointAngles[ib] = drad;
        }

        private static void CalculateHypotenuse(SkeletonContext context, int pointIndex)
        {
            //        bool drawLine = numberOfPoints == 4;
            int pointIndexB = (pointIndex + 1)%context.Count;
            Vector2 pa = context.UsePoints[pointIndex];
            Vector2 pb = context.UsePoints[pointIndexB];
            Vector2 baseDir = pa - pb;
            float baseLength = Vector2.Distance(pa, pb);
            float aa = context.PointAngles[pointIndex];
            float ab = context.PointAngles[pointIndexB];

            float baseAngle = Vector2.Angle(Vector2.up, baseDir);
            if (baseAngle < 0) baseAngle += 360;
            baseAngle = baseAngle*Mathf.Deg2Rad*Mathf.Sign(Vector2.Dot(Vector2.right, baseDir));

            Vector2 aDir = new Vector2(Mathf.Sin(aa + Mathf.PI*0.5f), Mathf.Cos(aa + Mathf.PI*0.5f));
            Vector2 bDir = new Vector2(Mathf.Sin(ab), Mathf.Cos(ab));
            float hypDot = Vector2.Dot(aDir, bDir);
            if (hypDot > -0.01f) //reflex - it'll never intersect laddey
                return;

            float relAngA = aa - baseAngle;
            float relAngB = ab - baseAngle;

            //triangulate the adjacent length
            float adjactentLength = (baseLength*Mathf.Sin(relAngA)*Mathf.Sin(relAngB))/Mathf.Sin(relAngA - relAngB);

            float hypA = adjactentLength/Mathf.Sin(relAngA); //trig get the hypot
            float hypB = adjactentLength/Mathf.Sin(relAngB);

            //Fast line intersection HERE
            int numberOfOriginalPoints = context.OriginalPoints.Count;
            Vector2 pB0A = pa;
            Vector2 pB1ha = pa + new Vector2(Mathf.Sin(aa), Mathf.Cos(aa))*hypA;
            Vector2 pB0B = pb;
            Vector2 pB1hb = pb + new Vector2(Mathf.Sin(ab), Mathf.Cos(ab))*hypB;
            bool calculateForward = true;
            bool calculateBackward = true;
            for (int i = 0; i < numberOfOriginalPoints; i++)
            {
                if (i == pointIndex)
                    continue;
                bool skipForard = false, skipBackward = false;
                if (i == ((pointIndex > 0) ? pointIndex - 1 : context.Count - 1))
                    skipForard = true;
                if (i == (pointIndex + 1)%context.Count)
                    skipBackward = true;
                Vector2 pA0 = new Vector2(context.OriginalPoints[i].X, context.OriginalPoints[i].Y);
                var nextOriginalPoint = context.OriginalPoints[(i + 1)%numberOfOriginalPoints];
                Vector2 pA1 = new Vector2(nextOriginalPoint.X, nextOriginalPoint.Y);

                if (pA0 == pB0A || pA1 == pB0A)
                    skipForard = true;
                if (pA0 == pB0B || pA1 == pB0B)
                    skipBackward = true;

                if (calculateForward && !skipForard)
                {
                    if (FastLineIntersection(pA0, pA1, pB0A, pB1ha))
                    {
                        //hypontenuse intersects building plan
                        context.HypsForward[pointIndex] = float.PositiveInfinity;
                        calculateForward = false;
                    }
                }
                if (calculateBackward && !skipBackward)
                {
                    if (FastLineIntersection(pA0, pA1, pB0B, pB1hb))
                    {
                        //hypontenuse intersects building plan
                        context.HypsBackward[pointIndexB] = float.PositiveInfinity;
                        calculateBackward = false;
                    }
                }
            }
            if (calculateForward)
            {
                context.HypsForward[pointIndex] = hypA;
            }
            if (calculateBackward)
            {
                context.HypsBackward[pointIndexB] = hypB;
            }
        }

        public static bool FastLineIntersection(Vector2 a1, Vector2 a2, Vector2 b1, Vector2 b2)
        {
            return (CCW(a1, b1, b2) != CCW(a2, b1, b2)) && (CCW(a1, a2, b1) != CCW(a1, a2, b2));
        }

        private static bool CCW(Vector2 p1, Vector2 p2, Vector2 p3)
        {
            return ((p2.x - p1.x)*(p3.y - p1.y) > (p2.y - p1.y)*(p3.x - p1.x));
        }

        internal class SkeletonContext
        {
            public int Count;

            /// <summary>
            ///     Result as a set of Vector2 in triangle sets of three.
            /// </summary>
            public List<Vector2> ResultPoints = new List<Vector2>();

            public List<MapPoint> OriginalPoints;

            public List<Vector2> UsePoints = new List<Vector2>();

            /// <summary>
            ///     Points that make up the ridge.
            /// </summary>
            public List<Vector2> InteriorPoints = new List<Vector2>();

            public List<float> PointAngles = new List<float>();

            /// <summary>
            ///     List of hypontenuse for ordering the points
            /// </summary>
            public List<float> HypsForward = new List<float>();

            /// <summary>
            ///     list of hypontenuse for ordering the points
            /// </summary>
            public List<float> HypsBackward = new List<float>();
        }
    }
}