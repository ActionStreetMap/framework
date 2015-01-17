// from Rx Official, but convert struct to class(for iOS AOT issue)

using System;

namespace ActionStreetMap.Infrastructure.Reactive
{
    /// <summary />
    [Serializable]
    public class Unit : IEquatable<Unit>
    {
        static readonly Unit @default = new Unit();

        /// <summary />
        public static Unit Default { get { return @default; } }

        /// <summary />
        public static bool operator ==(Unit first, Unit second)
        {
            return true;
        }

        /// <summary />
        public static bool operator !=(Unit first, Unit second)
        {
            return false;
        }
        /// <summary />
        public bool Equals(Unit other)
        {
            return true;
        }
        /// <summary />
        public override bool Equals(object obj)
        {
            return obj is Unit;
        }
        /// <summary />
        public override int GetHashCode()
        {
            return 0;
        }
        /// <summary />
        public override string ToString()
        {
            return "()";
        }
    }
}