using System.Diagnostics.CodeAnalysis;
using Color = System.Windows.Media.Color;

namespace Arcanum.Core.Utils;

public static class ColorConversion
{
   // Converts RGB to HSV
   [SuppressMessage("ReSharper", "InconsistentNaming")]
   public static (double hue, double saturation, double value) RgbToHsv(byte r, byte g, byte b)
   {
      var R = r / 255.0;
      var G = g / 255.0;
      var B = b / 255.0;

      var max = Math.Max(R, Math.Max(G, B));
      var min = Math.Min(R, Math.Min(G, B));
      var delta = max - min;

      double h = 0,
             s = 0,
             v = max;

      if (max > 0)
         s = delta / max;

      if (delta > 0)
      {
         if (Math.Abs(max - R) < 0.01)
            h = (G - B) / delta;
         else if (Math.Abs(max - G) < 0.01)
            h = 2 + (B - R) / delta;
         else
            h = 4 + (R - G) / delta;

         h *= 60;
         if (h < 0)
            h += 360;
      }

      return (h, s, v);
   }

   // Converts HSV to RGB
   public static Color HsvToRgb(double h, double s, double v)
   {
      var i = (int)Math.Floor(h / 60) % 6;
      var f = h / 60 - Math.Floor(h / 60);

      // ReSharper disable once InconsistentNaming
      var V = (byte)(v * 255);
      var p = (byte)(v * (1 - s) * 255);
      var q = (byte)(v * (1 - f * s) * 255);
      var t = (byte)(v * (1 - (1 - f) * s) * 255);

      return i switch
      {
         0 => Color.FromRgb(V, t, p),
         1 => Color.FromRgb(q, V, p),
         2 => Color.FromRgb(p, V, t),
         3 => Color.FromRgb(p, q, V),
         4 => Color.FromRgb(t, p, V),
         _ => Color.FromRgb(V, p, q),
      };
   }
}