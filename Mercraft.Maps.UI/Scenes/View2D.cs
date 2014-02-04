using System;
using Mercraft.Math.Primitives;
using Mercraft.Math.Units.Angle;

namespace Mercraft.Maps.UI.Rendering
{
	/// <summary>
	/// Represents a view on a 2D scene.
	/// </summary>
	public class View2D
	{
		/// <summary>
		/// Holds the rectangle in scene-coordinates of what the zoom represents.
		/// </summary>
		private RectangleF2D _rectangle;

		/// <summary>
		/// Holds the invert X flag.
		/// </summary>
		private bool _invertX;

		/// <summary>
		/// Holds the invert Y flag.
		/// </summary>
		private bool _invertY;

		/// <summary>
		/// Initializes a new instance of the <see cref="View2D"/> class.
		/// </summary>
		private View2D(RectangleF2D rectangle, bool invertX, bool invertY)
		{
			_invertX = invertX;
			_invertY = invertY;

			_rectangle = rectangle;
		}

        /// <summary>
        /// Returns the center of this view.
        /// </summary>
        public PointF2D Center {
            get
            {
                return _rectangle.Center;
            }
        }


		/// <summary>
		/// Gets the width.
		/// </summary>
		/// <value>The width.</value>
		public double Width{
			get{
				return _rectangle.Width;
			}
		}

		/// <summary>
		/// Gets the height.
		/// </summary>
		/// <value>The height.</value>
		public double Height{
			get{
				return _rectangle.Height;
			}
		}

		/// <summary>
		/// Gets the left top.
		/// </summary>
		/// <value>The left top.</value>
		public PointF2D LeftTop{
			get{
				if (_invertX && _invertY) {
					return _rectangle.BottomRight;
				} else if (_invertX) {
					return _rectangle.TopRight;
				} else if (_invertY) {
					return _rectangle.BottomLeft;
				}
				return _rectangle.TopLeft;
			}
		}

		/// <summary>
		/// Gets the right top.
		/// </summary>
		/// <value>The right top.</value>
		public PointF2D RightTop{
			get{
				if (_invertX && _invertY) {
					return _rectangle.BottomLeft;
				} else if (_invertX) {
					return _rectangle.TopLeft;
				} else if (_invertY) {
					return _rectangle.BottomRight;
				}
				return _rectangle.TopRight;
			}
		}

		/// <summary>
		/// Gets the left bottom.
		/// </summary>
		/// <value>The left bottom.</value>
		public PointF2D LeftBottom{
			get{
				if (_invertX && _invertY) {
					return _rectangle.TopRight;
				} else if (_invertX) {
					return _rectangle.BottomRight;
				} else if (_invertY) {
					return _rectangle.TopLeft;
				}
				return _rectangle.BottomLeft;
			}
		}

		/// <summary>
		/// Gets the right bottom.
		/// </summary>
		/// <value>The right bottom.</value>
		public PointF2D RightBottom{
			get{
				if (_invertX && _invertY) {
					return _rectangle.TopLeft;
				} else if (_invertX) {
					return _rectangle.BottomLeft;
				} else if (_invertY) {
					return _rectangle.TopRight;
				}
				return _rectangle.BottomRight;
			}
		}

		/// <summary>
		/// Gets the angle.
		/// </summary>
		/// <value>The angle.</value>
		public Degree Angle {
			get{
				return _rectangle.Angle;
			}
		}

		/// <summary>
		/// Gets the rectangle.
		/// </summary>
		/// <value>The rectangle.</value>
		public RectangleF2D Rectangle {
			get {
				return _rectangle;
			}
		}

		#region Create From

        /// <summary>
        /// Creates a new instance of the <see cref="OsmSharp.UI.Renderer.View2D"/> class.
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="centerX"></param>
        /// <param name="centerY"></param>
        /// <param name="directionX"></param>
        /// <param name="directionY"></param>
        /// <param name="angleY"></param>
        /// <returns></returns>
        public static View2D CreateFromCenterAndSize(double width, double height, double centerX, double centerY,
            bool directionX, bool directionY)
        {
            return View2D.CreateFromCenterAndSize(width, height, centerX, centerY, directionX, directionY, 0);
        }

	    /// <summary>
	    /// Creates a new instance of the <see cref="View2D"/> class.
	    /// </summary>
	    /// <param name="width">The width.</param>
	    /// <param name="height">The height.</param>
	    /// <param name="centerX">The center x.</param>
	    /// <param name="centerY">The center y.</param>
	    /// <param name="xInverted">When true x increases from left to right, when false otherwise.</param>
        /// <param name="yInverted">When true y increases from bottom to top, when false otherwise.</param>
        /// <param name="angleY"></param>
        public static View2D CreateFromCenterAndSize(double width, double height, double centerX, double centerY,
            bool xInverted, bool yInverted, Degree angleY)
		{
			if(width <= 0)
			{
				throw new ArgumentOutOfRangeException("width", "width has to be larger and not equal to zero.");
			}
			if(height <= 0)
			{
				throw new ArgumentOutOfRangeException("height", "height has to be larger and not equal to zero.");
			}

			return new View2D(RectangleF2D.FromBoundsAndCenter(width, height,
                centerX, centerY, angleY), xInverted, yInverted);
		}

		/// <summary>
		/// Creates a new instance of the <see cref="View2D"/> class.
		/// </summary>
		/// <returns>The from bounds.</returns>
		/// <param name="top">Top.</param>
		/// <param name="left">Left.</param>
		/// <param name="bottom">Bottom.</param>
		/// <param name="right">Right.</param>
        public static View2D CreateFromBounds(double top, double left, double bottom, double right)
		{
			double width;
			bool xInverted;
			double centerX = (left + right) / 2.0;
			if (left > right) {
				xInverted = true;
				width = left - right;
			} else {			
				width = right - left;
				xInverted = false;
			}

			double height;
			bool yInverted;
			double centerY = (top + bottom) / 2.0;
			if(bottom > top){
				yInverted = true;
				height = bottom - top;
			}
			else {
				yInverted = false;
				height = top - bottom;
			}
			return View2D.CreateFromCenterAndSize(width, height, centerX, centerY,
			                                      xInverted, yInverted);
		}

	    /// <summary>
	    /// Creates a view based on a center location a zoomfactor and the size of the current viewport.
	    /// </summary>
	    /// <param name="centerX"></param>
	    /// <param name="centerY"></param>
	    /// <param name="pixelsWidth"></param>
	    /// <param name="pixelsHeight"></param>
	    /// <param name="zoomFactor"></param>
	    /// <param name="xInverted"></param>
        /// <param name="yInverted"></param>
        /// <param name="angleY"></param>
	    /// <returns></returns>
        public static View2D CreateFrom(double centerX, double centerY, double pixelsWidth, double pixelsHeight,
            double zoomFactor, bool xInverted, bool yInverted)
        {
            return View2D.CreateFrom(centerX, centerY, pixelsWidth, pixelsHeight, zoomFactor, xInverted, yInverted, 0);
        }

	    /// <summary>
	    /// Creates a view based on a center location a zoomfactor and the size of the current viewport.
	    /// </summary>
	    /// <param name="centerX"></param>
	    /// <param name="centerY"></param>
	    /// <param name="pixelsWidth"></param>
	    /// <param name="pixelsHeight"></param>
	    /// <param name="zoomFactor"></param>
	    /// <param name="xInverted"></param>
        /// <param name="yInverted"></param>
        /// <param name="angleY"></param>
	    /// <returns></returns>
        public static View2D CreateFrom(double centerX, double centerY, double pixelsWidth, double pixelsHeight,
            double zoomFactor, bool xInverted, bool yInverted, Degree angleY)
        {
            double realZoom = zoomFactor;

            double width = pixelsWidth / realZoom;
            double height = pixelsHeight / realZoom;

            return View2D.CreateFromCenterAndSize(width, height, centerX, centerY, xInverted, yInverted, angleY);
        }

		#endregion

		/// <summary>
		/// Returns true if the given coordinates are inside this view.
		/// </summary>
		/// <returns><c>true</c> if this instance is in the specified x y; otherwise, <c>false</c>.</returns>
		/// <param name="x">The x coordinate.</param>
		/// <param name="y">The y coordinate.</param>
		public bool Contains (double x, double y)
		{
			if (_rectangle.BoundingBox.Contains (x, y)) {
				return _rectangle.Contains (x, y);
			}
			return false;
		}

        /// <summary>
        /// Returns true if an object with the given coordinates is visible with this view.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="closed"></param>
        /// <returns></returns>
        public bool IsVisible(double[] x, double[] y, bool closed)
        {
            double MinX = double.MaxValue;
            double MaxX = double.MinValue;
            for (int idx = 0; idx < x.Length; idx++)
            {
                if (x[idx] > MaxX)
                {
                    MaxX = x[idx];
                }
                if (x[idx] < MinX)
                {
                    MinX = x[idx];
                }
            }
            double MinY = double.MaxValue;
            double MaxY = double.MinValue;
            for (int idx = 0; idx < y.Length; idx++)
            {
                if (y[idx] > MaxY)
                {
                    MaxY = y[idx];
                }
                if (y[idx] < MinY)
                {
                    MinY = y[idx];
                }
            }
            return this.OverlapsWithBox(MinX, MinY, MaxX, MaxY);
        }

		/// <summary>
		/// Returns true if the given rectangle overlaps with this view.
		/// </summary>
		/// <returns><c>true</c>, if with rectangle overlaps, <c>false</c> otherwise.</returns>
		/// <param name="left">Left.</param>
		/// <param name="top">Top.</param>
		/// <param name="right">Right.</param>
		/// <param name="bottom">Bottom.</param>
		public bool OverlapsWithBox(double left, double top, double right, double bottom)
		{
			BoxF2D box = new BoxF2D (left, top, right, bottom);
			if (box.Overlaps (_rectangle.BoundingBox)) {
				return _rectangle.Overlaps (box);
			}
			return false;
		}

        /// <summary>
        /// Returns the coordinates represented by the given pixel in the given viewport.
        /// </summary>
        /// <param name="pixelX"></param>
        /// <param name="pixelY"></param>
        /// <param name="pixelsWidth"></param>
        /// <param name="pixelsHeight"></param>
        /// <returns></returns>
        public double[] FromViewPort(double pixelsWidth, double pixelsHeight, double pixelX, double pixelY)
        { // assumed that the coordinate system of the viewport starts at (0,0) in the topleft corner and increases to 
			return _rectangle.TransformFrom (pixelsWidth, pixelsHeight, _invertX, _invertY, pixelX, pixelY);
        }

		/// <summary>
		/// Froms the view port.
		/// </summary>
		/// <returns>The view port.</returns>
		/// <param name="pixelsWidth">Pixels width.</param>
		/// <param name="pixelsHeight">Pixels height.</param>
		/// <param name="pixelsX">Pixels x.</param>
		/// <param name="pixelsY">Pixels y.</param>
		public double[][] FromViewPort(double pixelsWidth, double pixelsHeight, double[] pixelsX, double[] pixelsY) {
			return _rectangle.TransformFrom (pixelsWidth, pixelsHeight, _invertX, _invertY, pixelsX, pixelsY);
		}

        /// <summary>
        /// Returns the viewport coordinates in the given viewport that corresponds with the given scene coordinates.
        /// </summary>
        /// <param name="pixelsWidth"></param>
        /// <param name="pixelsHeight"></param>
        /// <param name="sceneX"></param>
        /// <param name="sceneY"></param>
        /// <returns></returns>
        public double[][] ToViewPort(double pixelsWidth, double pixelsHeight, double[] sceneX, double[] sceneY)
        { // the right and going down.
			return _rectangle.TransformTo (pixelsWidth, pixelsHeight, _invertX, _invertY, sceneX, sceneY);
		}

		/// <summary>
		/// Returns the viewport coordinates in the given viewport that corresponds with the given scene coordinates.
		/// </summary>
		/// <param name="pixelsWidth"></param>
		/// <param name="pixelsHeight"></param>
		/// <param name="sceneX"></param>
		/// <param name="sceneY"></param>
		/// <returns></returns>
		public double[] ToViewPort(double pixelsWidth, double pixelsHeight, double sceneX, double sceneY)
		{ // the right and going down.
            return _rectangle.TransformTo(pixelsWidth, pixelsHeight, _invertX, _invertY, sceneX, sceneY);
		}

		/// <summary>
		/// Returns the viewport coordinates in the given viewport that corresponds with the given scene coordinates.
		/// </summary>
		/// <param name="pixelsWidth"></param>
		/// <param name="pixelsHeight"></param>
		/// <param name="sceneX"></param>
		/// <param name="sceneY"></param>
		/// <returns></returns>
		public void ToViewPort(double pixelsWidth, double pixelsHeight, double sceneX, double sceneY, double[] transformed)
		{ // the right and going down.
			_rectangle.TransformTo(pixelsWidth, pixelsHeight, _invertX, _invertY, sceneX, sceneY, transformed);
		}

        /// <summary>
        /// Calculates the zoom factor for the given view when at the given resolution.
        /// </summary>
        /// <param name="pixelsWidth"></param>
        /// <param name="pixelsHeight"></param>
        /// <returns></returns>
        public double CalculateZoom(double pixelsWidth, double pixelsHeight)
        {
            double realZoom = pixelsWidth / _rectangle.Width;
            return realZoom;
        }

		/// <summary>
		/// Rotates this view around it's center with a given angle and returns the modified version.
		/// </summary>
		/// <returns>The around center.</returns>
		/// <param name="angle">Angle.</param>
		public View2D RotateAroundCenter(Radian angle) {
			RectangleF2D rotated = this.Rectangle.RotateAroundCenter (angle);
			return new View2D (rotated, _invertX, _invertY);
		}

        /// <summary>
        /// Fites this view around the given points but keeps aspect ratio and 
        /// </summary>
        /// <param name="points"></param>
        /// <returns></returns>
        public View2D Fit(PointF2D[] points)
        {
            return this.Fit(points, 0);
        }

        /// <summary>
        /// Fites this view around the given points but keeps aspect ratio and 
        /// </summary>
        /// <param name="points"></param>
        /// <returns></returns>
        public View2D Fit(PointF2D[] points, double percentage)
        {
            RectangleF2D rotated = this.Rectangle.FitAndKeepAspectRatio(points, percentage);
            return new View2D(rotated, _invertX, _invertY);
        }

		/// <summary>
		/// Returns the smallest rectangular box containing the entire view. Will be larger when turned in a non-zero direction.
		/// </summary>
		/// <value>The outer box.</value>
		public BoxF2D OuterBox
		{
			get{
				return _rectangle.BoundingBox;
			}
		}

        /// <summary>
        /// Determines whether the specified <see cref="System.Object"/> is equal to the current <see cref="View2D"/>.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object"/> to compare with the current <see cref="View2D"/>.</param>
        /// <returns><c>true</c> if the specified <see cref="System.Object"/> is equal to the current
        /// <see cref="View2D"/>; otherwise, <c>false</c>.</returns>
        public override bool Equals(object obj)
        {
            View2D view = obj as View2D;
            if (view != null)
            {
                return view.Rectangle.Equals(
                    this.Rectangle);
            }
            return false;
        }

        /// <summary>
        /// Serves as a hash function for a <see cref="View2D"/> object.
        /// </summary>
        /// <returns>A hash code for this instance that is suitable for use in hashing algorithms and data structures such as a
        /// hash table.</returns>
        public override int GetHashCode()
        {
            return "View2D".GetHashCode() ^
                this.Rectangle.GetHashCode();
        }
    }
}