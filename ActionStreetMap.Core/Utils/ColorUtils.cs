using System;
using System.Collections.Generic;
using ActionStreetMap.Core.Unity;

namespace ActionStreetMap.Core.Utils
{
    /// <summary> Provides helper method to work with colors. </summary>
    internal static class ColorUtils
    {
        // TODO replace before release with different color
        private const string DefaultColor = "red";

        #region Known color mapping
        private static readonly Dictionary<string, Color32> KnownColors = new Dictionary<string, Color32>
        {
            // from system.drawing known colors set
            {"activeborder", new Color32(180, 180, 180, 255)},
            {"activecaption", new Color32(153, 180, 209, 255)},
            {"activecaptiontext", new Color32(0, 0, 0, 255)},
            {"appworkspace", new Color32(171, 171, 171, 255)},
            {"control", new Color32(240, 240, 240, 255)},
            {"controldark", new Color32(160, 160, 160, 255)},
            {"controldarkdark", new Color32(105, 105, 105, 255)},
            {"controllight", new Color32(227, 227, 227, 255)},
            {"controllightlight", new Color32(255, 255, 255, 255)},
            {"controltext", new Color32(0, 0, 0, 255)},
            {"desktop", new Color32(0, 0, 0, 255)},
            {"graytext", new Color32(109, 109, 109, 255)},
            {"highlight", new Color32(51, 153, 255, 255)},
            {"highlighttext", new Color32(255, 255, 255, 255)},
            {"hottrack", new Color32(0, 102, 204, 255)},
            {"inactiveborder", new Color32(244, 247, 252, 255)},
            {"inactivecaption", new Color32(191, 205, 219, 255)},
            {"inactivecaptiontext", new Color32(0, 0, 0, 255)},
            {"info", new Color32(255, 255, 225, 255)},
            {"infotext", new Color32(0, 0, 0, 255)},
            {"menu", new Color32(240, 240, 240, 255)},
            {"menutext", new Color32(0, 0, 0, 255)},
            {"scrollbar", new Color32(200, 200, 200, 255)},
            {"window", new Color32(255, 255, 255, 255)},
            {"windowframe", new Color32(100, 100, 100, 255)},
            {"windowtext", new Color32(0, 0, 0, 255)},
            {"transparent", new Color32(255, 255, 255, 0)},
            {"aliceblue", new Color32(240, 248, 255, 255)},
            {"antiquewhite", new Color32(250, 235, 215, 255)},
            {"aqua", new Color32(0, 255, 255, 255)},
            {"aquamarine", new Color32(127, 255, 212, 255)},
            {"azure", new Color32(240, 255, 255, 255)},
            {"beige", new Color32(245, 245, 220, 255)},
            {"bisque", new Color32(255, 228, 196, 255)},
            {"black", new Color32(0, 0, 0, 255)},
            {"blanchedalmond", new Color32(255, 235, 205, 255)},
            {"blue", new Color32(0, 0, 255, 255)},
            {"blueviolet", new Color32(138, 43, 226, 255)},
            {"brown", new Color32(165, 42, 42, 255)},
            {"burlywood", new Color32(222, 184, 135, 255)},
            {"cadetblue", new Color32(95, 158, 160, 255)},
            {"chartreuse", new Color32(127, 255, 0, 255)},
            {"chocolate", new Color32(210, 105, 30, 255)},
            {"coral", new Color32(255, 127, 80, 255)},
            {"cornflowerblue", new Color32(100, 149, 237, 255)},
            {"cornsilk", new Color32(255, 248, 220, 255)},
            {"crimson", new Color32(220, 20, 60, 255)},
            {"cyan", new Color32(0, 255, 255, 255)},
            {"darkblue", new Color32(0, 0, 139, 255)},
            {"darkcyan", new Color32(0, 139, 139, 255)},
            {"darkgoldenrod", new Color32(184, 134, 11, 255)},
            {"darkgray", new Color32(169, 169, 169, 255)},
            {"darkgreen", new Color32(0, 100, 0, 255)},
            {"darkkhaki", new Color32(189, 183, 107, 255)},
            {"darkmagenta", new Color32(139, 0, 139, 255)},
            {"darkolivegreen", new Color32(85, 107, 47, 255)},
            {"darkorange", new Color32(255, 140, 0, 255)},
            {"darkorchid", new Color32(153, 50, 204, 255)},
            {"darkred", new Color32(139, 0, 0, 255)},
            {"darksalmon", new Color32(233, 150, 122, 255)},
            {"darkseagreen", new Color32(143, 188, 139, 255)},
            {"darkslateblue", new Color32(72, 61, 139, 255)},
            {"darkslategray", new Color32(47, 79, 79, 255)},
            {"darkturquoise", new Color32(0, 206, 209, 255)},
            {"darkviolet", new Color32(148, 0, 211, 255)},
            {"deeppink", new Color32(255, 20, 147, 255)},
            {"deepskyblue", new Color32(0, 191, 255, 255)},
            {"dimgray", new Color32(105, 105, 105, 255)},
            {"dodgerblue", new Color32(30, 144, 255, 255)},
            {"firebrick", new Color32(178, 34, 34, 255)},
            {"floralwhite", new Color32(255, 250, 240, 255)},
            {"forestgreen", new Color32(34, 139, 34, 255)},
            {"fuchsia", new Color32(255, 0, 255, 255)},
            {"gainsboro", new Color32(220, 220, 220, 255)},
            {"ghostwhite", new Color32(248, 248, 255, 255)},
            {"gold", new Color32(255, 215, 0, 255)},
            {"goldenrod", new Color32(218, 165, 32, 255)},
            {"gray", new Color32(128, 128, 128, 255)},
            {"green", new Color32(0, 128, 0, 255)},
            {"greenyellow", new Color32(173, 255, 47, 255)},
            {"honeydew", new Color32(240, 255, 240, 255)},
            {"hotpink", new Color32(255, 105, 180, 255)},
            {"indianred", new Color32(205, 92, 92, 255)},
            {"indigo", new Color32(75, 0, 130, 255)},
            {"ivory", new Color32(255, 255, 240, 255)},
            {"khaki", new Color32(240, 230, 140, 255)},
            {"lavender", new Color32(230, 230, 250, 255)},
            {"lavenderblush", new Color32(255, 240, 245, 255)},
            {"lawngreen", new Color32(124, 252, 0, 255)},
            {"lemonchiffon", new Color32(255, 250, 205, 255)},
            {"lightblue", new Color32(173, 216, 230, 255)},
            {"lightcoral", new Color32(240, 128, 128, 255)},
            {"lightcyan", new Color32(224, 255, 255, 255)},
            {"lightgoldenrodyellow", new Color32(250, 250, 210, 255)},
            {"lightgray", new Color32(211, 211, 211, 255)},
            {"lightgreen", new Color32(144, 238, 144, 255)},
            {"lightpink", new Color32(255, 182, 193, 255)},
            {"lightsalmon", new Color32(255, 160, 122, 255)},
            {"lightseagreen", new Color32(32, 178, 170, 255)},
            {"lightskyblue", new Color32(135, 206, 250, 255)},
            {"lightslategray", new Color32(119, 136, 153, 255)},
            {"lightsteelblue", new Color32(176, 196, 222, 255)},
            {"lightyellow", new Color32(255, 255, 224, 255)},
            {"lime", new Color32(0, 255, 0, 255)},
            {"limegreen", new Color32(50, 205, 50, 255)},
            {"linen", new Color32(250, 240, 230, 255)},
            {"magenta", new Color32(255, 0, 255, 255)},
            {"maroon", new Color32(128, 0, 0, 255)},
            {"mediumaquamarine", new Color32(102, 205, 170, 255)},
            {"mediumblue", new Color32(0, 0, 205, 255)},
            {"mediumorchid", new Color32(186, 85, 211, 255)},
            {"mediumpurple", new Color32(147, 112, 219, 255)},
            {"mediumseagreen", new Color32(60, 179, 113, 255)},
            {"mediumslateblue", new Color32(123, 104, 238, 255)},
            {"mediumspringgreen", new Color32(0, 250, 154, 255)},
            {"mediumturquoise", new Color32(72, 209, 204, 255)},
            {"mediumvioletred", new Color32(199, 21, 133, 255)},
            {"midnightblue", new Color32(25, 25, 112, 255)},
            {"mintcream", new Color32(245, 255, 250, 255)},
            {"mistyrose", new Color32(255, 228, 225, 255)},
            {"moccasin", new Color32(255, 228, 181, 255)},
            {"navajowhite", new Color32(255, 222, 173, 255)},
            {"navy", new Color32(0, 0, 128, 255)},
            {"oldlace", new Color32(253, 245, 230, 255)},
            {"olive", new Color32(128, 128, 0, 255)},
            {"olivedrab", new Color32(107, 142, 35, 255)},
            {"orange", new Color32(255, 165, 0, 255)},
            {"orangered", new Color32(255, 69, 0, 255)},
            {"orchid", new Color32(218, 112, 214, 255)},
            {"palegoldenrod", new Color32(238, 232, 170, 255)},
            {"palegreen", new Color32(152, 251, 152, 255)},
            {"paleturquoise", new Color32(175, 238, 238, 255)},
            {"palevioletred", new Color32(219, 112, 147, 255)},
            {"papayawhip", new Color32(255, 239, 213, 255)},
            {"peachpuff", new Color32(255, 218, 185, 255)},
            {"peru", new Color32(205, 133, 63, 255)},
            {"pink", new Color32(255, 192, 203, 255)},
            {"plum", new Color32(221, 160, 221, 255)},
            {"powderblue", new Color32(176, 224, 230, 255)},
            {"purple", new Color32(128, 0, 128, 255)},
            {"red", new Color32(255, 0, 0, 255)},
            {"rosybrown", new Color32(188, 143, 143, 255)},
            {"royalblue", new Color32(65, 105, 225, 255)},
            {"saddlebrown", new Color32(139, 69, 19, 255)},
            {"salmon", new Color32(250, 128, 114, 255)},
            {"sandybrown", new Color32(244, 164, 96, 255)},
            {"seagreen", new Color32(46, 139, 87, 255)},
            {"seashell", new Color32(255, 245, 238, 255)},
            {"sienna", new Color32(160, 82, 45, 255)},
            {"silver", new Color32(192, 192, 192, 255)},
            {"skyblue", new Color32(135, 206, 235, 255)},
            {"slateblue", new Color32(106, 90, 205, 255)},
            {"slategray", new Color32(112, 128, 144, 255)},
            {"snow", new Color32(255, 250, 250, 255)},
            {"springgreen", new Color32(0, 255, 127, 255)},
            {"steelblue", new Color32(70, 130, 180, 255)},
            {"tan", new Color32(210, 180, 140, 255)},
            {"teal", new Color32(0, 128, 128, 255)},
            {"thistle", new Color32(216, 191, 216, 255)},
            {"tomato", new Color32(255, 99, 71, 255)},
            {"turquoise", new Color32(64, 224, 208, 255)},
            {"violet", new Color32(238, 130, 238, 255)},
            {"wheat", new Color32(245, 222, 179, 255)},
            {"white", new Color32(255, 255, 255, 255)},
            {"whitesmoke", new Color32(245, 245, 245, 255)},
            {"yellow", new Color32(255, 255, 0, 255)},
            {"yellowgreen", new Color32(154, 205, 50, 255)},
            {"buttonface", new Color32(240, 240, 240, 255)},
            {"buttonhighlight", new Color32(255, 255, 255, 255)},
            {"buttonshadow", new Color32(160, 160, 160, 255)},
            {"gradientactivecaption", new Color32(185, 209, 234, 255)},
            {"gradientinactivecaption", new Color32(215, 228, 242, 255)},
            {"menubar", new Color32(240, 240, 240, 255)},
            {"menuhighlight", new Color32(51, 153, 255, 255)},

            // custom
            {"grey", new Color32(190, 190, 190, 255)}
        };
        #endregion

        /// <summary> Gets color from given name. </summary>
        /// <param name="name">Name.</param>
        /// <returns>Color.</returns>
        public static Color32 FromName(string name)
        {
            var lowerCaseName = name.ToLowerInvariant();
            return KnownColors.ContainsKey(lowerCaseName) ? 
                KnownColors[lowerCaseName] : 
                KnownColors[DefaultColor];
        }

        /// <summary> Gets color from its hex representation. </summary>
        /// <param name="color">Hex color string.</param>
        /// <returns>Color.</returns>
        public static Color32 FromHex(string color)
        {
            byte red = (byte) (HexToInt(color[1]) + HexToInt(color[0]) * 16.000);
            byte green = (byte)(HexToInt(color[3]) + HexToInt(color[2]) * 16.000);
            byte blue = (byte)(HexToInt(color[5]) + HexToInt(color[4]) * 16.000);
            var finalColor = new Color32(red, green, blue, 1);
            return finalColor;
        }

        /// <summary> Gets color from its unknown representation. Supports name or hex. </summary>
        /// <param name="colorString">Color string.</param>
        /// <returns>Color.</returns>
        public static Color32 FromUnknown(string colorString)
        {
            var lowercase = colorString.ToLowerInvariant();
            return lowercase.StartsWith("#") ?
                FromHex(lowercase.Substring(1)) :
                FromName(lowercase);
        }

        private static int HexToInt(char hexChar) 
        {
	        switch (hexChar) 
            {
                case '0': return 0;
                case '1': return 1;
		        case '2': return 2;
		        case '3': return 3;
		        case '4': return 4;
		        case '5': return 5;
		        case '6': return 6;
		        case '7': return 7;
		        case '8': return 8;
		        case '9': return 9;
		        case 'a': return 10;
                case 'b': return 11;
                case 'c': return 12;
                case 'd': return 13;
                case 'e': return 14;
                case 'f': return 15;
	        }
            throw new ArgumentException(String.Format("Unknown hex: {0}", hexChar));
        }

        public static string ColorToGradient(string colorString)
        {
            var color = FromUnknown(colorString);
            var prevColor = EnlightColor(color, 0.4);
            var nextColor = EnlightColor(color, -0.4);

            return String.Format("gradient({0}, {1} 50%, {2})",
                prevColor.ToHex(),
                color.ToHex(),
                nextColor.ToHex());
        }

        public static Color32 EnlightColor(Color32 color, double ratio)
        {
            double hue, saturation, value;
            ColorToHSV(color, out hue, out saturation, out value);

            value += ratio;
            var newColor = ColorFromHSV(hue, saturation, value);

            return newColor;
        }

        /// <summary>
        ///  Converts color to HSV reprsentation.
        ///  See http://stackoverflow.com/questions/359612/how-to-change-rgb-color-to-hsv
        /// </summary>
        public static void ColorToHSV(Color32 color, out double hue, out double saturation, out double value)
        {
            int max = Math.Max(color.R, Math.Max(color.G, color.B));
            int min = Math.Min(color.R, Math.Min(color.G, color.B));

            hue = color.GetHue();
            saturation = (max == 0) ? 0 : 1d - (1d * min / max);
            value = max / 255d;
        }

        /// <summary>
        ///  Converts HSV reprsentation to Color.
        ///  See http://stackoverflow.com/questions/359612/how-to-change-rgb-color-to-hsv
        /// </summary>
        public static Color32 ColorFromHSV(double hue, double saturation, double value)
        {
            int hi = Convert.ToInt32(Math.Floor(hue / 60)) % 6;
            double f = hue / 60 - Math.Floor(hue / 60);

            value = value * 255;
            value = value > 255 ? 255 : value;
            value = value < 0 ? 0 : value;
            var v = Convert.ToByte(value);
            var p = Convert.ToByte(value * (1 - saturation));
            var q = Convert.ToByte(value * (1 - f * saturation));
            var t = Convert.ToByte(value * (1 - (1 - f) * saturation));

            if (hi == 0)
                return new Color32(v, t, p, 255);
            if (hi == 1)
                return new Color32(q, v, p, 255);
            if (hi == 2)
                return new Color32(p, v, t, 255);
            if (hi == 3)
                return new Color32(p, q, v, 255);
            if (hi == 4)
                return new Color32(t, p, v, 255);
            return new Color32(v, p, q, 255);
        }

        private static string ToHex(this Color32 color)
        {
            return String.Format("#{0:x2}{1:x2}{2:x2}", color.R, color.G, color.B);
        }

        private static int GetHue(this Color32 color)
        {
            return GetHue(color.R, color.G, color.B);
        }

        private static int GetHue(int red, int green, int blue)
        {
            float min = Math.Min(Math.Min(red, green), blue);
            float max = Math.Max(Math.Max(red, green), blue);

            float hue;
            if (max == red)
                hue = (green - blue) / (max - min);
            else if (max == green)
                hue = 2f + (blue - red) / (max - min);
            else
                hue = 4f + (red - green) / (max - min);

            hue = hue * 60;
            if (hue < 0) hue = hue + 360;

            return (int)Math.Round(hue);
        }
    }
}
