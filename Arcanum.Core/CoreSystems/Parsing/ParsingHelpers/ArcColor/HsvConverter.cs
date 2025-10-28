namespace Arcanum.Core.CoreSystems.Parsing.ParsingHelpers.ArcColor;

public static class HsvConverter
{
   public static System.Windows.Media.Color Hsv360ToRgb(double h, double s, double v)
   {
      // Clamp values to ensure they are in the expected range.
      h = Math.Max(0, Math.Min(360, h));
      s = Math.Max(0, Math.Min(1, s));
      v = Math.Max(0, Math.Min(1, v));

      if (Math.Abs(h - 360) < TOLERANCE)
         h = 0;

      if (s == 0)
      {
         var gray = (byte)(v * 255);
         return System.Windows.Media.Color.FromRgb(gray, gray, gray);
      }

      var hueSector = h / 60.0;
      var sectorIndex = (int)Math.Floor(hueSector);
      var fractionalSector = hueSector - sectorIndex;

      var p = v * (1 - s);
      var q = v * (1 - fractionalSector * s);
      var t = v * (1 - (1 - fractionalSector) * s);

      double r = 0,
             g = 0,
             b = 0;

      switch (sectorIndex % 6)
      {
         case 0:
            r = v;
            g = t;
            b = p;
            break;
         case 1:
            r = q;
            g = v;
            b = p;
            break;
         case 2:
            r = p;
            g = v;
            b = t;
            break;
         case 3:
            r = p;
            g = q;
            b = v;
            break;
         case 4:
            r = t;
            g = p;
            b = v;
            break;
         case 5:
            r = v;
            g = p;
            b = q;
            break;
      }

      return System.Windows.Media.Color.FromRgb((byte)Math.Round(r * 255),
                                                (byte)Math.Round(g * 255),
                                                (byte)Math.Round(b * 255));
   }

   private const double TOLERANCE = 0.01;
}