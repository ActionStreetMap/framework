using System;

namespace ActionStreetMap.Core.Geometry
{
    /// <summary> Represents rectangle in 2D space. </summary>
    public struct Rectangle2d
    {
        private readonly double _xmin;
        private readonly double _ymin;
        private readonly double _xmax;
        private readonly double _ymax;

        private readonly LineLinear2d _left;
        private readonly LineLinear2d _right;
        private readonly LineLinear2d _bottom;
        private readonly LineLinear2d _top;

        /// <summary> Initializes a new instance of the <see cref="Rectangle2d"/> class with predefined bounds. </summary>
        /// <param name="x"> Minimum x value (left). </param>
        /// <param name="y"> Minimum y value (bottom). </param>
        /// <param name="width"> Width of the rectangle. </param>
        /// <param name="height"> Height of the rectangle. </param>
        public Rectangle2d(double x, double y, double width, double height)
        {
            _xmin = x;
            _ymin = y;
            _xmax = x + width;
            _ymax = y + height;

            _left = new LineLinear2d(new Vector2d(_xmin, _ymin), new Vector2d(_xmin, _ymax));
            _right = new LineLinear2d(new Vector2d(_xmax, _ymin), new Vector2d(_xmax, _ymax));
            _bottom = new LineLinear2d(new Vector2d(_xmin, _ymin), new Vector2d(_xmax, _ymin));
            _top = new LineLinear2d(new Vector2d(_xmin, _ymax), new Vector2d(_xmax, _ymax));
        }

        /// <summary> Initializes a new instance of the <see cref="Rectangle2d"/> class with predefined bounds. </summary>
        /// <param name="leftBottom">Left bottom corner.</param>
        /// <param name="rightUpper">Right upper corner.</param>
        public Rectangle2d(Vector2d leftBottom, Vector2d rightUpper)
            : this(leftBottom.X, leftBottom.Y, rightUpper.X - leftBottom.X, rightUpper.Y - leftBottom.Y)
        {
        }

        /// <summary> Gets left. </summary>
        public double Left { get { return _xmin; } }

        /// <summary> Gets right. </summary>
        public double Right { get { return _xmax; } }

        /// <summary> Gets bottom. </summary>
        public double Bottom { get { return _ymin; } }

        /// <summary> Gets top. </summary>
        public double Top { get { return _ymax; } }

        /// <summary> Gets left bottom point. </summary>
        public Vector2d BottomLeft { get { return new Vector2d(_xmin, _ymin); } }

        /// <summary> Gets right top point. </summary>
        public Vector2d TopRight { get { return new Vector2d(_xmax, _ymax); } }

        /// <summary> Gets left top point. </summary>
        public Vector2d TopLeft { get { return new Vector2d(_xmin, _ymax); } }

        /// <summary> Gets right bottom point. </summary>
        public Vector2d BottomRight { get { return new Vector2d(_xmax, _ymin); } }

        /// <summary> Gets the width of the bounding box. </summary>
        public double Width { get { return _xmax - _xmin; } }

        /// <summary> Gets the height of the bounding box. </summary>
        public double Height { get { return _ymax - _ymin; } }

        /// <summary> Checks whether point is on border of rectangle. </summary>
        public bool IsOnBorder(Vector2d point)
        {
            return _left.Contains(point) || _right.Contains(point) ||
                  _bottom.Contains(point) || _top.Contains(point);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return String.Format("[{0},{1}]", BottomLeft, TopRight);
        }
    }
}
