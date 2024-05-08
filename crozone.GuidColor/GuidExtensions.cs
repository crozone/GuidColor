using System;
using System.Diagnostics;
using System.Drawing;
using System.IO.Hashing;
using System.Runtime.InteropServices;

namespace crozone.GuidColor
{
    public static class GuidExtensions
    {
        /// <summary>
        /// Converts a GUID to a <see cref="Color"/>, and a boolean describing whether that color is considered dark.
        /// </summary>
        /// <param name="guid">The source GUID to create a color from</param>
        /// <param name="seed">The seed used for the hashing algorithm. Can be set to change the color of a set of GUIDs depending on context.</param>
        /// <returns>
        /// A <see cref="ValueTuple{Color, Boolean}"/> of the GUID color, and whether the color is considered dark.
        /// The IsDark value is intended to facilitate displaying readable, high contrast text over the top of the GUID color when it is used as a background color.
        /// If IsDark is true, light colored text should be displayed.
        /// If IsDark is false, dark colored text should be displayed.
        /// </returns>
        public static (Color Color, bool IsDark) ToColor(this Guid guid, long seed = 0)
        {
            if (guid != Guid.Empty)
            {
                Span<byte> guidBytes = stackalloc byte[16];
                bool success = guid.TryWriteBytes(guidBytes);

                // We never expect Guid.TryWriteBytes() to fail here
                Debug.Assert(success);

                // We want to generate a unique hue angle and brightness level from the GUID.
                // Since GUIDs aren't completely random bits, but instead have structure depending on the GUID kind
                // (for example GUID v1 is timestamp based), hashing provides a way to generate a randomly distributed output
                // from the GUID so that we can extract a unique looking hue and brightness.
                //
                // Any hash that produces at least 8 bytes (two uints) will do, and we don't need it to be cryptographically secure.
                // XXHash3 is fast (faster than SHA) and produces an 8 byte hash.
                Span<byte> checksumBytes = stackalloc byte[8];
                success = XxHash3.TryHash(guidBytes, checksumBytes, out _, seed);

                // We never expect XxHash3.TryHash() to fail here
                Debug.Assert(success);

                double hueAngle = (MemoryMarshal.Read<uint>(checksumBytes) / (double)uint.MaxValue) * 360;
                double brightnessMod = (MemoryMarshal.Read<uint>(checksumBytes.Slice(sizeof(uint))) / (double)uint.MaxValue);

                double brightness = (0.6 * brightnessMod) + 0.2;
                double saturation = 1;

                bool isDark = hueAngle switch
                {
                    // If the color is between orangered and cobolt, which are perceptually darker colours,
                    // use a higher threshold before considering the colour "dark".
                    //
                    < 30 or > 210 => brightness <= 0.7,

                    // Otherwise, a brightness of 0.45 appears to be a good threshold.
                    //
                    _ => brightness <= 0.45
                };

                Color color = ColorHelpers.HslToRgbColor(hueAngle, saturation, brightness);
                return (color, isDark);
            }
            else
            {
                // If the GUID is an empty GUID, return black.
                return (Color.Black, true);
            }
        }

        /// <summary>
        /// Converts a GUID to a HTML color string, and a boolean describing whether that color is considered "dark".
        /// </summary>
        /// <param name="guid">The source GUID to create a color from</param>
        /// <param name="seed">The seed used for the hashing algorithm. Can be set to change the color of a set of GUIDs depending on context.</param>
        /// <returns>
        /// A <see cref="ValueTuple{String, Boolean}"/> of the GUID color as a HTML color string, and whether the color is considered dark.
        /// The IsDark value is intended to facilitate displaying readable, high contrast text over the top of the GUID color when it is used as a background color.
        /// If IsDark is true, light colored text should be displayed.
        /// If IsDark is false, dark colored text should be displayed.
        /// </returns>
        public static (string Color, bool IsDark) ToHtmlColor(this Guid guid, long seed = 0)
        {
            (Color guidColor, bool isDark) = guid.ToColor(seed);
            string htmlColor = ColorTranslator.ToHtml(guidColor);
            return (htmlColor, isDark);
        }
    }
}
