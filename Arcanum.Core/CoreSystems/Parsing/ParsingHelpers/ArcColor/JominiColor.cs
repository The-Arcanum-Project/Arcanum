using Arcanum.Core.GameObjects.BaseTypes;
using Color = System.Windows.Media.Color;

namespace Arcanum.Core.CoreSystems.Parsing.ParsingHelpers.ArcColor;

public abstract record JominiColor : IEmpty<JominiColor>
{
   private int? _cachedIntValue;

   /// <summary>
   /// Gets the ARGB integer representation of the color.
   /// The expensive conversion is only performed once.
   /// </summary>
   public int AsInt()
   {
      if (_cachedIntValue.HasValue)
         return _cachedIntValue.Value;

      var mediaColor = ToMediaColor();
      return (_cachedIntValue = (mediaColor.A << 24) | (mediaColor.B << 16) | (mediaColor.G << 8) | mediaColor.R).Value;
   }

   public abstract Color ToMediaColor();
   public abstract JominiColorType Type { get; }

   public enum JominiColorType
   {
      Key,
      Rgb,
      Hsv,
      Hsv360,
      Hex,
   }

   private JominiColor()
   {
   }

   public sealed record MediaColor(Color Color) : JominiColor
   {
      public override Color ToMediaColor() => Color;
      public override JominiColorType Type => JominiColorType.Rgb;
      public override string ToString() => $"rgb {{ {Color.R} {Color.G} {Color.B} }}";
   }

   public sealed record ColorKey(string Key) : JominiColor
   {
      public override Color ToMediaColor() => ColorResolver.Instance.Resolve(Key).ToMediaColor();
      public override JominiColorType Type => JominiColorType.Key;
      public override string ToString() => $"{Key}";
   }

   public sealed record Rgb(byte R, byte G, byte B) : JominiColor
   {
      public override Color ToMediaColor() => Color.FromRgb(R, G, B);
      public override JominiColorType Type => JominiColorType.Rgb;
      public override string ToString() => $"rgb {{ {R} {G} {B} }}";
   }

   // Standard HSV where H is [0, 360], S and V are [0, 1]
   public sealed record Hsv(double H, double S, double V) : JominiColor
   {
      public override Color ToMediaColor() => HsvConverter.Hsv360ToRgb(H * 360, S, V);
      public override JominiColorType Type => JominiColorType.Hsv;
      public override string ToString() => $"hsv {{ {H:F2} {S:F2} {V:F2} }}";
   }

   public sealed record Hsv360(double H, double S, double V) : JominiColor
   {
      public override Color ToMediaColor() => HsvConverter.Hsv360ToRgb(H, S, V);

      public override JominiColorType Type => JominiColorType.Hsv360;
      public override string ToString() => $"hsv360 {{ {H:F0} {S:F0} {V:F0} }}";
   }

   public sealed record Int(int Value) : JominiColor
   {
      public override Color ToMediaColor()
      {
         var r = (byte)((Value >> 16) & 0xFF);
         var g = (byte)((Value >> 8) & 0xFF);
         var b = (byte)(Value & 0xFF);
         return Color.FromRgb(r, g, b);
      }

      public override JominiColorType Type => JominiColorType.Hex;
      public override string ToString() => $"{Value:X6}";
   }

   public static JominiColor Empty { get; } = new Rgb(49, 49, 49);
}