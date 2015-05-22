using System;

namespace ActionStreetMap.Infrastructure.Primitives
{
    /// <summary> MutableTuple implementation. </summary>
    public class MutableTuple<T1, T2> : IEquatable<MutableTuple<T1, T2>>
    {
        /// <summary> Gets or sets first item. </summary>
        public T1 Item1 { get; set; }

        /// <summary> Gets or sets second item.
        /// </summary>
        public T2 Item2 { get; set; }

        /// <summary> Creates <see cref="MutableTuple{T1,T2}"/>.</summary>
        /// <param name="item1">First item.</param>
        /// <param name="item2">Second item.</param>
        public MutableTuple(T1 item1, T2 item2)
        {
            Item1 = item1;
            Item2 = item2;
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return Item1.GetHashCode() ^ Item2.GetHashCode();
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }
            return Equals((MutableTuple<T1, T2>)obj);
        }

        /// <inheritdoc />
        public bool Equals(MutableTuple<T1, T2> other)
        {
            return other.Item1.Equals(Item1) && other.Item2.Equals(Item2);
        }
    } 
}
