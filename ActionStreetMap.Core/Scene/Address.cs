using System;

namespace ActionStreetMap.Core.Scene
{
    /// <summary> Provides location information about the object. </summary>
    public class Address
    {
        /// <summary> Name, e.g. house number or road name. </summary>
        public string Name;

        /// <summary> Street name. </summary>
        public string Street;

        /// <summary> Code, e.g. post code. </summary>
        public string Code;

        /// <inheritdoc />
        public override string ToString()
        {
            return String.Format("{0} {1} {2}", Name, Street, Code);
        }
    }
}