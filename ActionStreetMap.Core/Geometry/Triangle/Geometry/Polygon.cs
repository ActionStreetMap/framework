using System;
using System.Collections.Generic;
using System.Linq;

namespace ActionStreetMap.Core.Geometry.Triangle.Geometry
{
    /// <summary> A polygon represented as a planar straight line graph. </summary>
    public class Polygon
    {
        private List<Vertex> points;
        private List<Point> holes;
        private List<Edge> segments;

        /// <summary> Gets the vertices of the polygon. </summary>
        public List<Vertex> Points { get { return points; } }

        /// <summary> Gets a list of points defining the holes of the polygon. </summary>
        public List<Point> Holes { get { return holes; } }

        /// <summary> Gets the segments of the polygon. </summary>
        public List<Edge> Segments { get { return segments; } }

        /// <summary> Gets point count. </summary>
        public int Count { get { return points.Count; } }

        /// <summary> Initializes a new instance of the <see cref="Polygon" /> class. </summary>
        public Polygon(): this(3, false)
        {
        }

        /// <summary> Initializes a new instance of the <see cref="Polygon" /> class. </summary>
        /// <param name="capacity">The default capacity for the points list.</param>
        public Polygon(int capacity): this(3, false)
        {
        }

        /// <summary> Initializes a new instance of the <see cref="Polygon" /> class. </summary>
        /// <param name="capacity">The default capacity for the points list.</param>
        /// <param name="markers">Use point and segment markers.</param>
        public Polygon(int capacity, bool markers)
        {
            points = new List<Vertex>(capacity);
            holes = new List<Point>();
            segments = new List<Edge>();
        }

        public void AddContour(List<Point> pointList, bool hole = false, bool convex = false)
        {
            if (!pointList.Any())
                return;

            var contour = new List<Vertex>(pointList.Count);
            foreach (var point in pointList)
                contour.Add(new Vertex(point.X, point.Y));
            AddContour(contour, hole, convex);
        }

        /// <summary> Adds a contour to the polygon. </summary>
        public void AddContour(List<Vertex> contour, bool hole = false, bool convex = false)
        {
            int offset = this.points.Count;
            int count = contour.Count;

            // Check if first vertex equals last vertex.
            if (contour[0] == contour[count - 1])
            {
                count--;
                contour.RemoveAt(count);
            }

            // Add points to polygon.
            this.points.AddRange(contour);

            var centroid = new Point(0.0, 0.0);

            for (int i = 0; i < count; i++)
            {
                centroid.x += contour[i].x;
                centroid.y += contour[i].y;

                // Add segments to polygon.
                this.segments.Add(new Edge(offset + i, offset + ((i + 1) % count)));
            }

            if (hole)
            {
                if (convex)
                {
                    // If the hole is convex, use its centroid.
                    centroid.x /= count;
                    centroid.y /= count;

                    this.holes.Add(centroid);
                }
                else
                {
                    Point point;
                    if (FindPointInPolygon(contour, out point))
                        holes.Add(point);
                }
            }
        }

        /// <summary> Adds a contour to the polygon. </summary>
        public void AddContour(IEnumerable<Vertex> points, Point hole)
        {
            // Copy input to list.
            var contour = new List<Vertex>(points);

            int offset = this.points.Count;
            int count = contour.Count;

            // Check if first vertex equals last vertex.
            if (contour[0] == contour[count - 1])
            {
                count--;
                contour.RemoveAt(count);
            }

            // Add points to polygon.
            this.points.AddRange(contour);

            for (int i = 0; i < count; i++)
            {
                // Add segments to polygon.
                this.segments.Add(new Edge(offset + i, offset + ((i + 1) % count)));
            }

            // TODO: check if hole is actually inside contour?
            this.holes.Add(hole);
        }

        /// <summary> Compute the bounds of the polygon. </summary>
        public Rectangle Bounds()
        {
            var bounds = new Rectangle();
            bounds.Expand(points);

            return bounds;
        }

        /// <summary> Add a vertex to the polygon. </summary>
        public void Add(Vertex vertex)
        {
            points.Add(vertex);
        }

        /// <summary> Add a vertex to the polygon. </summary>
        public void Add(Vertex vertex, double[] attributes)
        {
            // TODO: check attibutes

            vertex.attributes = attributes;

            this.points.Add(vertex);
        }

        /// <summary> Add a segment to the polygon. </summary>
        public void Add(Edge edge)
        {
            this.segments.Add(edge);
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
                point = new Point((contour[0].x + contour[1].x + contour[2].x) / 3,
                    (contour[0].y + contour[1].y + contour[2].y) / 3);
                return true;
            }


            for (int i = 0; i < length; i++)
            {
                a = contour[i];
                b = contour[(i + 1) % length];

                cx = (a.x + b.x) / 2;
                cy = (a.y + b.y) / 2;

                dx = (b.y - a.y) / 1.374;
                dy = (a.x - b.x) / 1.374;

                for (int j = 1; j <= limit; j++)
                {
                    // Search to the right of the segment.
                    point.x = cx + dx / j;
                    point.y = cy + dy / j;

                    if (bounds.Contains(point) && IsPointInPolygon(point, contour))
                        return true;

                    // Search on the other side of the segment.
                    point.x = cx - dx / j;
                    point.y = cy - dy / j;

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

            double x = point.x;
            double y = point.y;

            int count = poly.Count;

            for (int i = 0, j = count - 1; i < count; i++)
            {
                if (((poly[i].y < y && poly[j].y >= y) || (poly[j].y < y && poly[i].y >= y))
                    && (poly[i].x <= x || poly[j].x <= x))
                {
                    inside ^= (poly[i].x + (y - poly[i].y) / (poly[j].y - poly[i].y) * (poly[j].x - poly[i].x) < x);
                }

                j = i;
            }

            return inside;
        }
    }
}
