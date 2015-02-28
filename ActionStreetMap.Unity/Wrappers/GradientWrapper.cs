using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ActionStreetMap.Unity.Wrappers
{
    /// <summary> Wraps unity gradient. </summary>
    public class GradientWrapper
    {
#if !CONSOLE
        private readonly Gradient _gradient;

        /// <summary> Evaluates color. </summary>
        public Color Evaluate(float time)
        {
            return _gradient.Evaluate(time);
        }

        /// <summary> Creates gradient. </summary>
        public GradientWrapper(IEnumerable<ColorKey> colorKeys, IEnumerable<AlphaKey> alphaKeys)
        {
            var gradient = new Gradient();
            gradient.SetKeys(
                colorKeys.Select(c => new GradientColorKey(c.Color, c.Time)).ToArray(),
                alphaKeys.Select(c => new GradientAlphaKey(c.Alpha, c.Time)).ToArray());
            _gradient = gradient;
        } 
#else
        /// <summary> Evaluates color. </summary>
        public Color Evaluate(float time)
        {
            return Color.red;
        }

        /// <summary> Creates gradient. </summary>
        public GradientWrapper(IEnumerable<ColorKey> colorKeys, IEnumerable<AlphaKey> alphaKeys)
        {
        }

#endif
        #region Nested types

        /// <summary> Represents color key. </summary>
        public struct ColorKey
        {
            /// <summary> Color. </summary>
            public Color Color;
            /// <summary> Time. </summary>
            public float Time;
        }

        /// <summary> Represents alpha key. </summary>
        public struct AlphaKey
        {
            /// <summary> Alpha. </summary>
            public float Alpha;
            /// <summary> Time. </summary>
            public float Time;
        }

        #endregion
    }
}
