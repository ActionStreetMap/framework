using System;

namespace ActionStreetMap.Core.Unity
{
    /// <summary> Defines RGBA color. </summary>
    public class Color32
    {
        /// <summary> Red. </summary>
        public byte R { get; private set; }

        /// <summary> Green. </summary>
        public byte G { get; private set; }

        /// <summary> Blue. </summary>
        public byte B { get; private set; }

        /// <summary> Alpha. </summary>
        public byte A { get; private set; }

        private readonly int _intValue;

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

            _intValue = R;
            _intValue = (_intValue << 8) + G;
            _intValue = (_intValue << 8) + B;
        }

        /// <summary> Gets int reprentation. </summary>
        public int IntValue { get { return _intValue; } }

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
        public override int GetHashCode()
        {
            return _intValue;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (!(obj is Color32))
                return false;

            var color = obj as Color32;

            return R == color.R && G == color.G && B == color.B && A == color.A;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return String.Format("({0},{1},{2},{3})", R, G, B, A);
        }
    }
}