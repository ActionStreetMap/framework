using System;

namespace ActionStreetMap.Core.Utils
{
    internal static class MathUtils
    {
        /// <summary> Round digit count. </summary>
        internal const int RoundDigitCount = 5;

        /// <summary> Current epsion </summary>
        internal const double Epsion = 0.00001;

        /// <summary> Converts degrees to radians. </summary>
        public static double Deg2Rad(double degrees)
        {
            return Math.PI*degrees/180.0;
        }

        /// <summary> Converts radians to degrees. </summary>
        public static double Rad2Deg(double radians)
        {
            return 180.0*radians/Math.PI;
        }

        /// <summary> Compares equality of two double using epsilon. </summary>
        /// <param name="a">First double.</param>
        /// <param name="b">Second double.</param>
        /// <param name="epsilon">Epsilon.</param>
        /// <returns>True if equal.</returns>
        public static bool AreEqual(double a, double b, double epsilon = double.Epsilon)
        {
            return Math.Abs(a - b) < epsilon;
        }

        /// <summary> Clamp function. </summary>
        public static double Clamp(double value, double min, double max)
        {
            return (value < min) ? min : (value > max) ? max : value;
        }
    }
}