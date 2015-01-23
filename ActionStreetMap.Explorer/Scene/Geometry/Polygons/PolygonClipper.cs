using System;
using System.Collections.Generic;
using System.Linq;
using ActionStreetMap.Core;

namespace ActionStreetMap.Explorer.Scene.Geometry.Polygons
{
    /// <summary>
    ///     Sutherland-Hodgman algorithm implementation which provides the way to clip the subject polygon against the clip
    ///     polygon (gets the intersection of the two polygons)
    ///     http://rosettacode.org/wiki/Sutherland-Hodgman_polygon_clipping#C.23
    /// </summary>
    internal static class PolygonClipper
    {
        #region Class: Edge

        /// <summary>
        ///     This represents a line segment
        /// </summary>
        private class Edge
        {
            public Edge(MapPoint from, MapPoint to)
            {
                From = from;
                To = to;
            }

            public readonly MapPoint From;
            public readonly MapPoint To;
        }

        #endregion

        /// <summary>
        ///     This clips the subject polygon against the clip polygon (gets the intersection of the two polygons)
        /// </summary>
        /// <remarks>
        ///     Based on the psuedocode from:
        ///     http://en.wikipedia.org/wiki/Sutherland%E2%80%93Hodgman
        /// </remarks>
        /// <param name="subjectPoly">Can be concave or convex</param>
        /// <param name="clipPoly">Must be convex</param>
        /// <returns>The intersection of the two polygons (or null).</returns>
        public static List<MapPoint> GetIntersectedPolygon(List<MapPoint> subjectPoly, List<MapPoint> clipPoly)
        {
            if (subjectPoly.Count < 3 || clipPoly.Count < 3)
                throw new AlgorithmException(string.Format(Strings.CannotClipPolygon, subjectPoly.Count, clipPoly.Count));

            List<MapPoint> outputList = subjectPoly.ToList();

            //	Make sure it's clockwise
            if (!IsClockwise(subjectPoly))
                outputList.Reverse();

            //	Walk around the clip polygon clockwise
            foreach (Edge clipEdge in IterateEdgesClockwise(clipPoly))
            {
                List<MapPoint> inputList = outputList.ToList(); //	clone it
                outputList.Clear();

                if (inputList.Count == 0)
                {
                    //	Sometimes when the polygons don't intersect, this list goes to zero.  
                    //  Jump out to avoid an index out of range exception
                    break;
                }

                MapPoint s = inputList[inputList.Count - 1];

                foreach (MapPoint e in inputList)
                {
                    if (IsInside(clipEdge, e))
                    {
                        if (!IsInside(clipEdge, s))
                        {
                            MapPoint? point = GetIntersect(s, e, clipEdge.From, clipEdge.To);
                            if (point == null)
                            {
                                throw new ApplicationException("Line segments don't intersect");
                                //	may be colinear, or may be a bug
                            }
                            outputList.Add(point.Value);
                        }

                        outputList.Add(e);
                    }
                    else if (IsInside(clipEdge, s))
                    {
                        MapPoint? point = GetIntersect(s, e, clipEdge.From, clipEdge.To);
                        if (point == null)
                        {
                            throw new ApplicationException("Line segments don't intersect");
                            //	may be colinear, or may be a bug
                        }
                        outputList.Add(point.Value);
                    }

                    s = e;
                }
            }

            //	Exit Function
            return outputList;
        }

        #region Private Methods

        /// <summary>
        ///     This iterates through the edges of the polygon, always clockwise
        /// </summary>
        private static IEnumerable<Edge> IterateEdgesClockwise(List<MapPoint> polygon)
        {
            if (IsClockwise(polygon))
            {
                #region Already clockwise

                for (int cntr = 0; cntr < polygon.Count - 1; cntr++)
                {
                    yield return new Edge(polygon[cntr], polygon[cntr + 1]);
                }

                yield return new Edge(polygon[polygon.Count - 1], polygon[0]);

                #endregion
            }
            else
            {
                #region Reverse

                for (int cntr = polygon.Count - 1; cntr > 0; cntr--)
                {
                    yield return new Edge(polygon[cntr], polygon[cntr - 1]);
                }

                yield return new Edge(polygon[0], polygon[polygon.Count - 1]);

                #endregion
            }
        }

        /// <summary>
        ///     Returns the intersection of the two lines (line segments are passed in, but they are treated like infinite lines)
        /// </summary>
        /// <remarks>
        ///     Got this here:
        ///     http://stackoverflow.com/questions/14480124/how-do-i-detect-triangle-and-rectangle-intersection
        /// </remarks>
        private static MapPoint? GetIntersect(MapPoint line1From, MapPoint line1To, MapPoint line2From, MapPoint line2To)
        {
            MapPoint direction1 = new MapPoint(line1To.X - line1From.X, line1To.Y - line1From.Y);
            MapPoint direction2 = new MapPoint(line2To.X - line2From.X, line2To.Y - line2From.Y);
            double dotPerp = (direction1.X*direction2.Y) - (direction1.Y*direction2.X);

            // If it's 0, it means the lines are parallel so have infinite intersection points
            if (IsNearZero(dotPerp))
            {
                return null;
            }

            MapPoint c = new MapPoint(line2From.X - line1From.X, line2From.Y - line1From.Y);
            float t = (float) ((c.X*direction2.Y - c.Y*direction2.X)/dotPerp);
            //if (t < 0 || t > 1)
            //{
            //    return null;		//	lies outside the line segment
            //}

            //double u = (c.X * direction1.Y - c.Y * direction1.X) / dotPerp;
            //if (u < 0 || u > 1)
            //{
            //    return null;		//	lies outside the line segment
            //}

            //	Return the intersection point
            return new MapPoint(line1From.X + (t*direction1.X), line1From.Y + (t*direction1.Y));
        }

        private static bool IsInside(Edge edge, MapPoint test)
        {
            bool? isLeft = IsLeftOf(edge, test);
            if (isLeft == null)
            {
                //	Colinear points should be considered inside
                return true;
            }

            return !isLeft.Value;
        }

        private static bool IsClockwise(List<MapPoint> polygon)
        {
            for (int cntr = 2; cntr < polygon.Count; cntr++)
            {
                bool? isLeft = IsLeftOf(new Edge(polygon[0], polygon[1]), polygon[cntr]);
                if (isLeft != null)
                    //	some of the points may be colinear.  That's ok as long as the overall is a polygon
                {
                    return !isLeft.Value;
                }
            }

            throw new ArgumentException("All the points in the polygon are colinear");
        }

        /// <summary>
        ///     Tells if the test point lies on the left side of the edge line
        /// </summary>
        private static bool? IsLeftOf(Edge edge, MapPoint test)
        {
            MapPoint tmp1 = new MapPoint(edge.To.X - edge.From.X, edge.To.Y - edge.From.Y);
            MapPoint tmp2 = new MapPoint(test.X - edge.To.X, test.Y - edge.To.Y);

            double x = (tmp1.X*tmp2.Y) - (tmp1.Y*tmp2.X); //	dot product of perpendicular?

            if (x < 0)
            {
                return false;
            }
            if (x > 0)
            {
                return true;
            }
            //	Colinear points;
            return null;
        }

        private static bool IsNearZero(double testValue)
        {
            return Math.Abs(testValue) <= .000000001d;
        }

        #endregion
    }
}