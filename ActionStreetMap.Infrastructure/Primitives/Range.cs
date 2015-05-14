using System;

namespace ActionStreetMap.Infrastructure.Primitives
{
    /// <summary> Represents range. </summary>
    /// <typeparam name="T">IComparable type.</typeparam>
    public struct Range<T> where T : IComparable<T>
    {
        /// <summary> Minimum value of the range. </summary>
        public readonly T Minimum;

        /// <summary> Maximum value of the range. </summary>
        public readonly T Maximum;

        /// <summary> Creates instance of <see cref="Range{T}"/>. </summary>
        /// <param name="min">Minimum value of the range. </param>
        /// <param name="max">Maximum value of the range.</param>
        public Range(T min, T max)
        {
            Minimum = min;
            Maximum = max;
        }

        /// <summary> Determines if the range is valid. </summary>
        /// <returns>True if range is valid, else false.</returns>
        public bool IsValid() { return Minimum.CompareTo(Maximum) <= 0; }

        /// <summary> Determines if the provided value is inside the range. </summary>
        /// <param name="value">The value to test.</param>
        /// <returns>True if the value is inside Range, else false.</returns>
        public bool Contains(T value)
        {
            return (Minimum.CompareTo(value) <= 0) && (value.CompareTo(Maximum) <= 0);
        }

        /// <summary> Determines if this Range is inside the bounds of another range. </summary>
        /// <param name="range">The parent range to test on.</param>
        /// <returns>True if range is inclusive, else false.</returns>
        public bool IsInside(Range<T> range)
        {
            return this.IsValid() && range.IsValid() && range.Contains(this.Minimum) && range.Contains(this.Maximum);
        }

        /// <summary> Determines if another range is inside the bounds of this range. </summary>
        /// <param name="range">The child range to test.</param>
        /// <returns>True if range is inside, else false.</returns>
        public bool Contains(Range<T> range)
        {
            return IsValid() && range.IsValid() && Contains(range.Minimum) && Contains(range.Maximum);
        }

        /// <summary> Presents the Range in readable format. </summary>
        /// <returns>String representation of the Range.</returns>
        public override string ToString() { return String.Format("[{0} - {1}]", Minimum, Maximum); }
    }
}
