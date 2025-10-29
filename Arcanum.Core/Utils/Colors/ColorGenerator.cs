namespace Arcanum.Core.Utils.Colors;

public static class ColorGenerator
{
   /// <summary>
   /// Converts a Color object to its 32-bit ARGB (Alpha, Red, Green, Blue) integer representation.
   /// This is standard for WPF and GDI.
   /// </summary>
   public static int AsArgbInt(this System.Windows.Media.Color color)
   {
      return (color.A << 24) | (color.R << 16) | (color.G << 8) | color.B;
   }

   /// <summary>
   /// Converts a Color object to its 32-bit ABGR (Alpha, Blue, Green, Red) integer representation.
   /// This is common in graphics APIs like DirectX and OpenGL.
   /// </summary>
   public static int AsAbgrInt(this System.Windows.Media.Color color)
   {
      // Notice the R and B channels are swapped in their bitwise positions.
      return (color.A << 24) | (color.B << 16) | (color.G << 8) | color.R;
   }

   /// <summary>
   /// A struct to represent a color in the HSL (Hue, Saturation, Lightness) color space.
   /// </summary>
   private struct HslColor
   {
      public double H; // Hue, from 0 to 360
      public double S; // Saturation, from 0 to 1
      public double L; // Lightness, from 0 to 1
   }

   /// <summary>
   /// Generates a list of colors that are perceptually close to a given base color.
   /// </summary>
   /// <param name="baseColor">The starting color.</param>
   /// <param name="count">The number of color variations to generate.</param>
   /// <param name="saturationVariation">The maximum amount Saturation can change (e.g., 0.1 for +/- 10%).</param>
   /// <param name="lightnessVariation">The maximum amount Lightness can change (e.g., 0.1 for +/- 10%).</param>
   /// <returns>A list of generated Color objects.</returns>
   public static List<System.Windows.Media.Color> GenerateVariations(
      System.Windows.Media.Color baseColor,
      int count,
      double saturationVariation = 0.1,
      double lightnessVariation = 0.1)
   {
      if (count <= 0)
         return [];

      var variations = new List<System.Windows.Media.Color>(count);
      var baseHsl = RgbToHsl(baseColor);
      var random = new Random();

      for (var i = 0; i < count; i++)
      {
         // Create a new HSL color based on the original
         var newHsl = baseHsl;

         // Jiggle the Saturation and Lightness within the specified range
         // The formula (random.NextDouble() * 2 - 1) generates a random number between -1 and 1
         var satJiggle = (random.NextDouble() * 2 - 1) * saturationVariation;
         var lightJiggle = (random.NextDouble() * 2 - 1) * lightnessVariation;

         newHsl.S += satJiggle;
         newHsl.L += lightJiggle;

         // Clamp the values to ensure they remain in the valid [0, 1] range
         newHsl.S = Math.Max(0, Math.Min(1, newHsl.S));
         newHsl.L = Math.Max(0, Math.Min(1, newHsl.L));

         // Convert the new HSL color back to RGB and add it to the list
         variations.Add(HslToRgb(newHsl));
      }

      return variations;
   }

   #region HSL / RGB Conversion Helpers

   private static HslColor RgbToHsl(System.Windows.Media.Color color)
   {
      var r = color.R / 255.0;
      var g = color.G / 255.0;
      var b = color.B / 255.0;

      var max = Math.Max(r, Math.Max(g, b));
      var min = Math.Min(r, Math.Min(g, b));
      double h = 0,
             s = 0,
             l = (max + min) / 2;

      if (Math.Abs(max - min) < 0.001)
      {
         h = s = 0; // achromatic (gray)
      }
      else
      {
         var d = max - min;
         s = l > 0.5 ? d / (2 - max - min) : d / (max + min);

         if (AreFloatsEqual(max, r))
            h = (g - b) / d + (g < b ? 6 : 0);
         else if (AreFloatsEqual(max, g))
            h = (b - r) / d + 2;
         else if (AreFloatsEqual(max, b))
            h = (r - g) / d + 4;

         h /= 6;
      }

      return new HslColor
      {
         H = h * 360,
         S = s,
         L = l,
      };
   }

   private static bool AreFloatsEqual(double a, double b, double epsilon = 0.001)
   {
      return Math.Abs(a - b) < epsilon;
   }

   private static System.Windows.Media.Color HslToRgb(HslColor hsl)
   {
      double r,
             g,
             b;
      var h = hsl.H / 360.0;
      var s = hsl.S;
      var l = hsl.L;

      if (Math.Abs(s) < 0.001)
      {
         r = g = b = l; // achromatic
      }
      else
      {
         var q = l < 0.5 ? l * (1 + s) : l + s - l * s;
         var p = 2 * l - q;

         r = HueToRgb(p, q, h + 1.0 / 3.0);
         g = HueToRgb(p, q, h);
         b = HueToRgb(p, q, h - 1.0 / 3.0);
      }

      return System.Windows.Media.Color.FromRgb((byte)(r * 255), (byte)(g * 255), (byte)(b * 255));
   }

   private static double HueToRgb(double p, double q, double t)
   {
      if (t < 0)
         t += 1;
      if (t > 1)
         t -= 1;
      return t switch
      {
         < 1.0 / 6.0 => p + (q - p) * 6 * t,
         < 1.0 / 2.0 => q,
         < 2.0 / 3.0 => p + (q - p) * (2.0 / 3.0 - t) * 6,
         _ => p,
      };
   }

   #endregion
}