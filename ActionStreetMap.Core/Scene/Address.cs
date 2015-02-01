using System;

namespace ActionStreetMap.Core.Scene
{
    /// <summary> Provides location information about the object. </summary>
    public class Address
    {
        /// <summary> Gets name, e.g. house number or road name. </summary>
        public string Name { get; set; }

        /// <summary> Gets street name. </summary>
        public string Street { get; set; }

        /// <summary> Gets code, e.g. post code. </summary>
        public string Code { get; set; }

        /// <summary> Converts to string. </summary>
        public override string ToString()
        {
            return String.Format("{0} {1} {2}", Name, Street, Code);
        }
    }
}