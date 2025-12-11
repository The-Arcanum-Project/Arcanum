using Color = System.Windows.Media.Color;

namespace Arcanum.Core.CoreSystems.Parsing.ParsingHelpers.ArcColor;

public static class HsvConverter
{
   public static Color Hsv360ToRgb(double h, double s, double v)
   {
      if (h < 0)
         h = 0;
      else if (h > 360)
         h = 360;
      if (s < 0)
         s = 0;
      else if (s > 1)
         if (s <= 100)
            s /= 100f;
         else
            s = 1;

      if (v < 0)
         v = 0;
      else if (v > 1)
         if (v <= 100)
            v /= 100f;
         else
            v = 1;

      var c = v * s;
      var x = c * (1 - Math.Abs(h / 60f % 2 - 1));
      var m = v - c;

      double r = 0,
             g = 0,
             b = 0;

      // Use integer division to find the hue segment
      var hueSegment = (int)Math.Floor(h / 60f) % 6;

      switch (hueSegment)
      {
         case 0:
            r = c;
            g = x;
            b = 0;
            break;
         case 1:
            r = x;
            g = c;
            b = 0;
            break;
         case 2:
            r = 0;
            g = c;
            b = x;
            break;
         case 3:
            r = 0;
            g = x;
            b = c;
            break;
         case 4:
            r = x;
            g = 0;
            b = c;
            break;
         case 5:
            r = c;
            g = 0;
            b = x;
            break;
      }

      return Color.FromRgb((byte)Math.Round((r + m) * 255),
                           (byte)Math.Round((g + m) * 255),
                           (byte)Math.Round((b + m) * 255));
   }
}