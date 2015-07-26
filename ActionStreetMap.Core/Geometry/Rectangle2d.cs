namespace ActionStreetMap.Core.Geometry
{
    /// <summary> Represents rectangle in 2D space. </summary>
    public struct Rectangle2d
    {
        private readonly double _xmin;
        private readonly double _ymin;
        private readonly double _xmax;
        private readonly double _ymax;

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
        }

        /// <summary> Initializes a new instance of the <see cref="Rectangle2d"/> class with predefined bounds. </summary>
        /// <param name="leftBottom">Left bottom corner.</param>
        /// <param name="rightUpper">Right upper corner.</param>
        public Rectangle2d(Vector2d leftBottom, Vector2d rightUpper)
        {
            _xmin = leftBottom.X;
            _ymin = leftBottom.Y;
            _xmax = rightUpper.X;
            _ymax = rightUpper.Y;
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
    }
}
