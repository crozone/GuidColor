using System;
using System.Drawing;

namespace crozone.GuidColor
{
    internal static class ColorHelpers
    {
        /// <summary>
        /// Converts HSL color value to RGB fractional color value
        /// </summary>
        /// <param name="hue">Hue angle value between [0,360]</param>
        /// <param name="saturation">Saturation value between [0,1]</param>
        /// <param name="lightness">Lightness value between [0,1]</param>
        /// <returns>RGB color values, with each value between [0,1]</returns>
        /// <see href="https://en.wikipedia.org/wiki/HSL_and_HSV#HSL_to_RGB_alternative"/>
        public static (double red, double green, double blue) HslToRgb(double hue, double saturation, double lightness)
        {
            // Normalize hue angle
            hue %= 360;
            if (hue < 0)
            {
                hue += 360;
            }

            double a = saturation * Math.Min(lightness, 1 - lightness);

            double GetChannel(int n)
            {
                double k = (n + hue / 30) % 12;
                return lightness - a * Math.Max(-1, Math.Min(Math.Min(k - 3, 9 - k), 1));
            }

            return (GetChannel(0), GetChannel(8), GetChannel(4));
        }

        /// <summary>
        /// Converts HSL color value to RGB integer color value
        /// </summary>
        /// <param name="hue">Hue angle value between [0,360]</param>
        /// <param name="saturation">Saturation value between [0,1]</param>
        /// <param name="lightness">Lightness value between [0,1]</param>
        /// <returns>The <see cref="Color"/> equivalent of the input HSL values</returns>
        public static Color HslToRgbColor(double hue, double saturation, double lightness)
        {
            (double red, double green, double blue) = HslToRgb(hue, saturation, lightness);

            return Color.FromArgb(
                (byte)Math.Min(255, (int)(red * 256)),
                (byte)Math.Min(255, (int)(green * 256)),
                (byte)Math.Min(255, (int)(blue * 256))
                );
        }
    }
}
