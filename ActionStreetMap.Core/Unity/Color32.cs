using System;

namespace ActionStreetMap.Core.Unity
{
    /// <summary> Defines RGBA color. </summary>
    public class Color32
    {
        /// <summary> Red. </summary>
        public byte R;

        /// <summary> Green. </summary>
        public byte G;

        /// <summary> Blue. </summary>
        public byte B;

        /// <summary> Alpha. </summary>
        public byte A;

        /// <summary> Creates color. </summary>
        /// <param name="r">Red.</param>
        /// <param name="g">Green.</param>
        /// <param name="b">Blue.</param>
        /// <param name="a">Alpha.</param>
        public Color32(byte r, byte g, byte b, byte a)
        {
            R = r;
            G = g;
            B = b;
            A = a;
        }

        /// <summary> Converts to int reprentation. </summary>
        /// <returns></returns>
        public int ToInt()
        {
            int rgb = R;
            rgb = (rgb << 8) + G;
            rgb = (rgb << 8) + B;

            return rgb;
        }

        /// <summary>
        ///     Calculate distance to given color.This algorithm is combination both weighted Euclidean distance functions, where
        ///     the weight factors depend on how big the "red" component of the colour is.
        ///     http://www.compuphase.com/cmetric.htm
        /// </summary>
        /// <param name="other">Color.</param>
        /// <returns>Distance.</returns>
        public double DistanceTo(Color32 other)
        {
            long rmean = (R + other.R)/2;
            long r = R - other.R;
            long g = G - other.G;
            long b = B - other.B;
            return Math.Sqrt((((512 + rmean)*r*r) >> 8) + 4*g*g + (((767 - rmean)*b*b) >> 8));
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return String.Format("({0},{1},{2},{3})", R, G, B, A);
        }
    }
}