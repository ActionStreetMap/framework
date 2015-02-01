using System;

namespace ActionStreetMap.Core
{
    /// <summary> Used to highlight bugs in algortihms of app. </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2240:ImplementISerializableCorrectly")]
    [Serializable]
    public class AlgorithmException: Exception
    {
        /// <summary> Creates AlgorithmException. </summary>
        /// <param name="message">Exception message.</param>
        public AlgorithmException(string message)
            : base(message) { }
    }
}
