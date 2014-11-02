﻿using System;

namespace Mercraft.Core
{
    /// <summary>
    ///     Represents map point in two dimensional space. However contains associated elevation with it.
    /// </summary>
    public struct MapPoint
    {
        /// <summary>
        ///     X-axis coordinate.
        /// </summary>
        public float X;

        /// <summary>
        ///     Y-axis coordinate.
        /// </summary>
        public float Y;
        
        /// <summary>
        ///     Elevation (height on sea level in given point).
        /// </summary>
        public float Elevation;

        /// <summary>
        ///     Sets elevation value.
        /// </summary>
        /// <param name="elevation">Elevation value.</param>
        public void SetElevation(float elevation)
        {
            Elevation = elevation;
        }

        /// <summary>
        ///     Creates MapPoint.
        /// </summary>
        /// <param name="x">X coordinate.</param>
        /// <param name="y">Y coordinate.</param>
        /// <param name="elevation">Elevation.</param>
        public MapPoint(float x, float y, float elevation) : this(x, y)
        {
            Elevation = elevation;
        }

        /// <summary>
        ///     Creates MapPoint.
        /// </summary>
        /// <param name="x">X coordinate.</param>
        /// <param name="y">Y coordinate.</param>
        public MapPoint(float x, float y): this()
        {
            X = x;
            Y = y;
        }

        /// <summary>
        ///     Calculate distance between two points in 2D space
        /// </summary>
        /// <param name="point">Point.</param>
        /// <returns>Distance in 2D space.</returns>
        public float DistanceTo(MapPoint point)
        {
            return (float) Math.Sqrt(Math.Pow(point.X - X, 2) + Math.Pow(point.Y - Y, 2));
        }

        /// <summary>
        ///      Calculate distance between two points in 2D space
        /// </summary>
        /// <param name="x">X coordinate.</param>
        /// <param name="y">Y coordinate.</param>
        /// <returns>Distance in 2D space.</returns>
        public float DistanceTo(float x, float y)
        {
            return (float)Math.Sqrt(Math.Pow(x - X, 2) + Math.Pow(y - Y, 2));
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return string.Format("({0:F1}, {1:F1}):{2:F1}", X, Y, Elevation);
        }

        /// <summary>
        ///     Overloads + operation
        /// </summary>
        public static MapPoint operator +(MapPoint left, MapPoint right)
        {
            return new MapPoint(left.X + right.X, left.Y + right.Y);
        }

        /// <summary>
        ///     Overloads - operation
        /// </summary>
        public static MapPoint operator -(MapPoint left, MapPoint right)
        {
            return new MapPoint(left.X - right.X, left.Y - right.Y);
        }

        /// <summary>
        ///     Gets normalized.
        /// </summary>
        public MapPoint Normalize()
        {
            var distance = (float) Math.Sqrt(this.X * this.X + this.Y * this.Y);
            return new MapPoint(this.X / distance, this.Y / distance);
        }

        /// <summary>
        ///     Gets dot product.
        /// </summary>
        public float Dot(MapPoint point)
        {
            return this.X * point.Y + this.Y * point.Y;
        } 
    }
}
