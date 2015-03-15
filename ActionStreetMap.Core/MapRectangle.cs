namespace ActionStreetMap.Core
{
    /// <summary> Represents map rectangle. </summary>
    public struct MapRectangle
    {
        private readonly float _xmin;
        private readonly float _ymin;
        private readonly float _xmax;
        private readonly float _ymax;

        /// <summary> Initializes a new instance of the <see cref="MapRectangle"/> class with predefined bounds. </summary>
        /// <param name="x"> Minimum x value (left). </param>
        /// <param name="y"> Minimum y value (bottom). </param>
        /// <param name="width"> Width of the rectangle. </param>
        /// <param name="height"> Height of the rectangle. </param>
        public MapRectangle(float x, float y, float width, float height)
        {
            _xmin = x;
            _ymin = y;
            _xmax = x + width;
            _ymax = y + height;
        }

        /// <summary> Gets left bottom point. </summary>
        public MapPoint BottomLeft { get { return new MapPoint(_xmin, _ymin); } }

        /// <summary> Gets right top point. </summary>
        public MapPoint TopRight { get { return new MapPoint(_xmax, _ymax); } }

        /// <summary> Gets left top point. </summary>
        public MapPoint TopLeft { get { return new MapPoint(_xmin, _ymax); } }

        /// <summary> Gets right bottom point. </summary>
        public MapPoint BottomRight { get { return new MapPoint(_xmax, _ymin); } }

        /// <summary> Gets the width of the bounding box. </summary>
        public float Width { get { return _xmax - _xmin; } }

        /// <summary> Gets the height of the bounding box. </summary>
        public float Height { get { return _ymax - _ymin; } }
    }
}
