using System;
using System.Collections.Generic;
using System.Linq;
using ActionStreetMap.Infrastructure.Utilities;

namespace ActionStreetMap.Core.Geometry.Triangle.Geometry
{
    /// <summary> A polygon represented as a planar straight line graph. </summary>
    public sealed class Polygon: IDisposable
    {
        private readonly IObjectPool _objectPool;

        /// <summary> Points of polygon. </summary>
        public List<Vertex> Points;

        /// <summary> Holes of polygon. </summary>
        public List<Point> Holes;

        /// <summary> Segments of polygon. </summary>
        public List<Edge> Segments;

        /// <summary> Initializes a new instance of the <see cref="Polygon" /> class. </summary>
        /// <param name="capacity">The default capacity for the points list.</param>
        /// <param name="objectPool">Object pool.</param>
        public Polygon(int capacity, IObjectPool objectPool)
        {
            _objectPool = objectPool;

            Points = _objectPool.NewList<Vertex>(capacity);
            Holes = _objectPool.NewList<Point>();
            Segments = _objectPool.NewList<Edge>();
        }

        public void AddContour(List<Point> pointList, bool hole = false, bool convex = false)
        {
            if (!pointList.Any())
                return;
            
            // TODO list allocation!
            var contour = new List<Vertex>(pointList.Count);
            foreach (var point in pointList)
                contour.Add(new Vertex(point.X, point.Y));
            AddContour(contour, hole, convex);
        }

        /// <summary> Adds a contour to the polygon. </summary>
        public void AddContour(List<Vertex> contour, bool hole = false, bool convex = false)
        {
            ushort offset = (ushort) Points.Count;
            ushort count = (ushort) contour.Count;

            // Check if first vertex equals last vertex.
            if (contour[0] == contour[count - 1])
            {
                count--;
                contour.RemoveAt(count);
            }

            // Add points to polygon.
            Points.AddRange(contour);

            var centroid = new Point(0.0, 0.0);

            for (ushort i = 0; i < count; i++)
            {
                centroid.X += contour[i].X;
                centroid.Y += contour[i].Y;

                // Add segments to polygon.
                Segments.Add(new Edge((ushort)(offset + i), (ushort)(offset + ((i + 1) % count))));
            }

            if (hole)
            {
                if (convex)
                {
                    // If the hole is convex, use its centroid.
                    centroid.X /= count;
                    centroid.Y /= count;

                    Holes.Add(centroid);
                }
                else
                {
                    Point point;
                    if (FindPointInPolygon(contour, out point))
                        Holes.Add(point);
                }
            }
        }

        private bool FindPointInPolygon(List<Vertex> contour, out Point point)
        {
            var bounds = new Rectangle();
            bounds.Expand(contour);

            int length = contour.Count;
            int limit = 8;

            point = new Point();

            Point a, b; // Current edge.
            double cx, cy; // Center of current edge.
            double dx, dy; // Direction perpendicular to edge.

            if (contour.Count == 3)
            {
                point = new Point((contour[0].X + contour[1].X + contour[2].X) / 3,
                    (contour[0].Y + contour[1].Y + contour[2].Y) / 3);
                return true;
            }


            for (int i = 0; i < length; i++)
            {
                a = contour[i];
                b = contour[(i + 1) % length];

                cx = (a.X + b.X) / 2;
                cy = (a.Y + b.Y) / 2;

                dx = (b.Y - a.Y) / 1.374;
                dy = (a.X - b.X) / 1.374;

                for (int j = 1; j <= limit; j++)
                {
                    // Search to the right of the segment.
                    point.X = cx + dx / j;
                    point.Y = cy + dy / j;

                    if (bounds.Contains(point) && IsPointInPolygon(point, contour))
                        return true;

                    // Search on the other side of the segment.
                    point.X = cx - dx / j;
                    point.Y = cy - dy / j;

                    if (bounds.Contains(point) && IsPointInPolygon(point, contour))
                        return true;
                }
            }

            return false;
        }

        /// <summary> Return true if the given point is inside the polygon, or false if it is not. </summary>
        /// <param name="point">The point to check.</param>
        /// <param name="poly">The polygon (list of contour points).</param>
        /// <returns></returns>
        /// <remarks>
        /// WARNING: If the point is exactly on the edge of the polygon, then the function
        /// may return true or false.
        /// 
        /// See http://alienryderflex.com/polygon/
        /// </remarks>
        private bool IsPointInPolygon(Point point, List<Vertex> poly)
        {
            bool inside = false;

            double x = point.X;
            double y = point.Y;

            int count = poly.Count;

            for (int i = 0, j = count - 1; i < count; i++)
            {
                if (((poly[i].Y < y && poly[j].Y >= y) || (poly[j].Y < y && poly[i].Y >= y))
                    && (poly[i].X <= x || poly[j].X <= x))
                {
                    inside ^= (poly[i].X + (y - poly[i].Y) / (poly[j].Y - poly[i].Y) * (poly[j].X - poly[i].X) < x);
                }

                j = i;
            }

            return inside;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _objectPool.StoreList(Points);
            _objectPool.StoreList(Holes);
            _objectPool.StoreList(Segments);
        }
    }
}
