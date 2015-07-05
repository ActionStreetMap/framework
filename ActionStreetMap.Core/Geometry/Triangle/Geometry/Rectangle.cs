using System;
using System.Collections.Generic;

namespace ActionStreetMap.Core.Geometry.Triangle.Geometry
{
    /// <summary> A simple bounding box class. </summary>
    public class Rectangle
    {
        private double _xmin, _ymin, _xmax, _ymax;

        /// <summary> Initializes a new instance of the <see cref="Rectangle" /> class. </summary>
        public Rectangle()
        {
            _xmin = _ymin = double.MaxValue;
            _xmax = _ymax = -double.MaxValue;
        }

        public Rectangle(Rectangle other)
            : this(other.Left, other.Bottom, other.Right, other.Top)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="Rectangle" /> class
        ///     with predefined bounds.
        /// </summary>
        /// <param name="x">Minimum x value (left).</param>
        /// <param name="y">Minimum y value (bottom).</param>
        /// <param name="width">Width of the rectangle.</param>
        /// <param name="height">Height of the rectangle.</param>
        public Rectangle(double x, double y, double width, double height)
        {
            _xmin = x;
            _ymin = y;
            _xmax = x + width;
            _ymax = y + height;
        }

        /// <summary> Gets the minimum x value (left boundary). </summary>
        public double Left { get { return _xmin; } }

        /// <summary> Gets the maximum x value (right boundary). </summary>
        public double Right { get { return _xmax; } }

        /// <summary> Gets the minimum y value (bottom boundary). </summary>
        public double Bottom { get { return _ymin; } }

        /// <summary> Gets the maximum y value (top boundary). </summary>
        public double Top { get { return _ymax; } }

        /// <summary> Gets the width of the bounding box. </summary>
        public double Width { get { return _xmax - _xmin; } }

        /// <summary> Gets the height of the bounding box. </summary>
        public double Height { get { return _ymax - _ymin; } }

        /// <summary> Scale bounds. </summary>
        /// <param name="dx">Add dx to left and right bounds.</param>
        /// <param name="dy">Add dy to top and bottom bounds.</param>
        public void Resize(double dx, double dy)
        {
            _xmin -= dx;
            _xmax += dx;
            _ymin -= dy;
            _ymax += dy;
        }

        /// <summary> Expand rectangle to include given point. </summary>
        public void Expand(Point p)
        {
            _xmin = Math.Min(_xmin, p.X);
            _ymin = Math.Min(_ymin, p.Y);
            _xmax = Math.Max(_xmax, p.X);
            _ymax = Math.Max(_ymax, p.Y);
        }

        /// <summary> Expand rectangle to include a list of points. </summary>
        public void Expand(IEnumerable<Point> points)
        {
            foreach (var p in points)
                Expand(p);
        }

        /// <summary> Expand rectangle to include a list of points. </summary>
        public void Expand(IEnumerable<Vertex> points)
        {
            foreach (var p in points)
                Expand(p);
        }

        /// <summary> Expand rectangle to include given rectangle. </summary>
        public void Expand(Rectangle other)
        {
            _xmin = Math.Min(_xmin, other._xmin);
            _ymin = Math.Min(_ymin, other._ymin);
            _xmax = Math.Max(_xmax, other._xmax);
            _ymax = Math.Max(_ymax, other._ymax);
        }

        /// <summary> Check if given point is inside bounding box. </summary>
        /// <param name="pt">Point to check.</param>
        /// <returns>Return true, if bounding box contains given point.</returns>
        public bool Contains(Point pt)
        {
            return ((pt.X >= _xmin) && (pt.X <= _xmax) && (pt.Y >= _ymin) && (pt.Y <= _ymax));
        }

        /// <summary> Check if given rectangle is inside bounding box. </summary>
        /// <param name="other">Rectangle to check.</param>
        /// <returns>Return true, if bounding box contains given rectangle.</returns>
        public bool Contains(Rectangle other)
        {
            return (_xmin <= other.Left && other.Right <= _xmax
                    && _ymin <= other.Bottom && other.Top <= _ymax);
        }

        /// <summary> Check if given rectangle intersects bounding box. </summary>
        /// <param name="other">Rectangle to check.</param>
        /// <returns>Return true, if given rectangle intersects bounding box.</returns>
        public bool Intersects(Rectangle other)
        {
            return (other.Left < _xmax && _xmin < other.Right
                    && other.Bottom < _ymax && _ymin < other.Top);
        }
    }
}