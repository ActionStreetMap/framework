using System;
using ActionStreetMap.Core.Utilities;
using ActionStreetMap.Explorer.Infrastructure;
using ActionStreetMap.Unity.Wrappers;

namespace ActionStreetMap.Explorer.Utils
{
    internal static class GradientUtils
    {
        private static readonly string[] PartSplitter =  {"__"};
        private static readonly string[] ValueSplitter = { "_" };

        public static GradientWrapper ParseGradient(string gradientString)
        {
            var parts = gradientString.Split(PartSplitter, StringSplitOptions.None);

            var colorKeysRaw = parts[0].Split(ValueSplitter, StringSplitOptions.None);
            var alphaKeysRaw = parts[1].Split(ValueSplitter, StringSplitOptions.None);

            var colorKeys = new GradientWrapper.ColorKey[colorKeysRaw.Length/2];
            for (int i = 0; i < colorKeys.Length; i++)
                colorKeys[i] = new GradientWrapper.ColorKey
                {
                    Color = ColorUtility.FromHex(colorKeysRaw[i*2]).ToUnityColor(),
                    Time = float.Parse(colorKeysRaw[i*2] + 1)
                };

            var alphaKeys = new GradientWrapper.AlphaKey[alphaKeysRaw.Length/2];
            for (int i = 0; i < alphaKeys.Length; i++)
                alphaKeys[i] = new GradientWrapper.AlphaKey()
                {
                    Alpha = float.Parse(alphaKeysRaw[i * 2]),
                    Time = float.Parse(alphaKeysRaw[i * 2] + 1)
                };

            return new GradientWrapper(colorKeys, alphaKeys);
        }
    }
}
