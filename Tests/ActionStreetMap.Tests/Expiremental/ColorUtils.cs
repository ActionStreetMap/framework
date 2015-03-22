using System;
using System.Drawing;

namespace ActionStreetMap.Tests.Expiremental
{
    /// <summary>
    ///     Used to create gradient string.
    ///     See http://stackoverflow.com/questions/359612/how-to-change-rgb-color-to-hsv
    /// </summary>
    class ColorUtils
    {
        public static string GetGradientString(string colorString)
        {
            var color = ColorTranslator.FromHtml(colorString);
            var prevColor = EnlightColor(color, 0.4);
            var nextColor = EnlightColor(color, -0.4);

            return String.Format("{0}_0_{1}_.5_{2}_1__1_0_1_1", 
                ColorTranslator.ToHtml(prevColor).Substring(1),
                ColorTranslator.ToHtml(color).Substring(1),
                ColorTranslator.ToHtml(nextColor).Substring(1));
        }

        public static Color EnlightColor(Color color, double ratio)
        {
            double hue, saturation, value;
            ColorToHSV(color, out hue, out saturation, out value);

            value += ratio;
            var newColor = ColorFromHSV(hue, saturation, value);

            return newColor;
        }

        public static void ColorToHSV(Color color, out double hue, out double saturation, out double value)
        {
            int max = Math.Max(color.R, Math.Max(color.G, color.B));
            int min = Math.Min(color.R, Math.Min(color.G, color.B));

            hue = color.GetHue();
            saturation = (max == 0) ? 0 : 1d - (1d * min / max);
            value = max / 255d;
        }

        public static Color ColorFromHSV(double hue, double saturation, double value)
        {
            int hi = Convert.ToInt32(Math.Floor(hue / 60)) % 6;
            double f = hue / 60 - Math.Floor(hue / 60);

            value = value * 255;
            value = value > 255 ? 255 : value;
            value = value < 0 ? 0 : value;
            var v = Convert.ToInt32(value);
            var p = Convert.ToInt32(value * (1 - saturation));
            var q = Convert.ToInt32(value * (1 - f * saturation));
            var t = Convert.ToInt32(value * (1 - (1 - f) * saturation));

            if (hi == 0)
                return Color.FromArgb(255, v, t, p);
            else if (hi == 1)
                return Color.FromArgb(255, q, v, p);
            else if (hi == 2)
                return Color.FromArgb(255, p, v, t);
            else if (hi == 3)
                return Color.FromArgb(255, p, q, v);
            else if (hi == 4)
                return Color.FromArgb(255, t, p, v);
            else
                return Color.FromArgb(255, v, p, q);
        }
    }
}
