using System;
using System.Text.RegularExpressions;
using ActionStreetMap.Core.Utils;
using ActionStreetMap.Explorer.Infrastructure;
using ActionStreetMap.Unity.Wrappers;
using UnityEngine;

namespace ActionStreetMap.Explorer.Utils
{
    internal static class GradientUtils
    {
        private static readonly Regex MapCssGradientRegEx = new Regex(@"rgb ?\([ 0-9.%,]+?\)|#[0-9a-fA-F]{3,6}\s[0-9]{1,3}[%|px]|#[0-9a-fA-F]{3,6}|(aqua|black|blue|fuchsia|gray|green|lime|maroon|navy|olive|orange|purple|red|silver|teal|white|yellow){1}(\s[0-9]{1,3}\s*[%|px]?)?");
        public static GradientWrapper ParseGradient(string gradientString)
        {
            var results = MapCssGradientRegEx.Matches(gradientString);
            var count = results.Count;
            if (count == 0)
                throw new ArgumentException(String.Format(Strings.InvalidGradientString, gradientString));

            var colorKeys = new GradientWrapper.ColorKey[count];
            for (int i = 0; i < count; i++)
            {
                var values = results[i].Groups[0].Value.Split(' ');
                var color = ColorUtils.FromUnknown(values[0]);
                float time = i == 0 ? 0 :
                    (i == results.Count - 1) ? 1 :
                    float.Parse(values[1].Substring(0, values[1].Length - 1)) / 100f;
                colorKeys[i] = colorKeys[i] = new GradientWrapper.ColorKey
                {
                    Color = color.ToUnityColor(),
                    Time = time
                };
            }

            var alphaKeys = new GradientWrapper.AlphaKey[]
            {
                new GradientWrapper.AlphaKey() {Alpha = 1, Time = 0},
                new GradientWrapper.AlphaKey() {Alpha = 1, Time = 1},
            };
          
            return new GradientWrapper(colorKeys, alphaKeys);
        }

        public static Color GetColor(GradientWrapper gradientWrapper, Vector3 point, float freq)
        {
            var value = (Noise.Perlin3D(point, freq) + 1f) / 2f;
            return gradientWrapper.Evaluate(value);
        }
    }
}
