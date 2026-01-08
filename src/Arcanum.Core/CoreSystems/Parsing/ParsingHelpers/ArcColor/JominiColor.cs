using Arcanum.Core.GameObjects.BaseTypes;
using Vortice.Mathematics;
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

   public int AsHex()
   {
      var c = ToMediaColor();
      return ((c.A << 24) | (c.R << 16) | (c.G << 8) | c.B) & 0xFFFFFF;
   }

   public string AsHexString() => AsHex().ToString("X6");

   public abstract Color ToMediaColor();
   public abstract JominiColorType Type { get; }
   public abstract Color4 ToColor4();

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
      public override Color4 ToColor4() => new(Color.R / 255.0f, Color.G / 255.0f, Color.B / 255.0f, Color.A / 255.0f);
   }

   public sealed record ColorKey(string Key) : JominiColor
   {
      public override Color ToMediaColor() => ColorResolver.Instance.Resolve(Key).ToMediaColor();
      public override JominiColorType Type => JominiColorType.Key;
      public override string ToString() => $"{Key}";

      public override Color4 ToColor4() => ColorResolver.Instance.Resolve(Key).ToColor4();
   }

   public sealed record Rgb(byte R, byte G, byte B, byte A = 255) : JominiColor
   {
      public override Color ToMediaColor() => Color.FromArgb(A, R, G, B);
      public override JominiColorType Type => JominiColorType.Rgb;

      public override string ToString() => A == 255
                                              ? $"rgb {{ {R} {G} {B} }}"
                                              : $"rgb {{ {R} {G} {B} {A} }}";

      public override Color4 ToColor4() => new(R / 255.0f, G / 255.0f, B / 255.0f, A / 255.0f);
   }

   public sealed record Hex(int Value) : JominiColor
   {
      public override Color ToMediaColor()
      {
         var r = (byte)((Value >> 16) & 0xFF);
         var g = (byte)((Value >> 8) & 0xFF);
         var b = (byte)(Value & 0xFF);
         return Color.FromRgb(r, g, b);
      }

      public override JominiColorType Type => JominiColorType.Hex;
      public override string ToString() => $"hex {{ {Value:X6} }}";

      public override Color4 ToColor4()
      {
         var r = (byte)((Value >> 16) & 0xFF);
         var g = (byte)((Value >> 8) & 0xFF);
         var b = (byte)(Value & 0xFF);
         return new(r / 255.0f, g / 255.0f, b / 255.0f);
      }
   }

   // Standard HSV where H is [0, 360], S and V are [0, 1]
   public sealed record Hsv(double H, double S, double V, double A = 1.0) : JominiColor
   {
      public override Color ToMediaColor()
      {
         var c = HsvConverter.Hsv360ToRgb(H * 360, S, V);
         return Color.FromArgb((byte)(A * 255), c.R, c.G, c.B);
      }

      public override JominiColorType Type => JominiColorType.Hsv;

      public override string ToString() => A >= 0.999
                                              ? $"hsv {{ {H:F2} {S:F2} {V:F2} }}"
                                              : $"hsv {{ {H:F2} {S:F2} {V:F2} {A:F2} }}";

      public override Color4 ToColor4()
      {
         var color = HsvConverter.Hsv360ToRgb(H * 360, S, V);
         return new(color.R / 255.0f, color.G / 255.0f, color.B / 255.0f, (float)A);
      }
   }

   public sealed record Hsv360(double H, double S, double V, double A = 1.0) : JominiColor
   {
      public override Color ToMediaColor()
      {
         var c = HsvConverter.Hsv360ToRgb(H, S, V);
         return Color.FromArgb((byte)(A * 255), c.R, c.G, c.B);
      }

      public override JominiColorType Type => JominiColorType.Hsv360;

      public override string ToString() => A >= 0.999
                                              ? $"hsv360 {{ {H:F0} {S:F0} {V:F0} }}"
                                              : $"hsv360 {{ {H:F0} {S:F0} {V:F0} {A:F2} }}";

      public override Color4 ToColor4()
      {
         var color = HsvConverter.Hsv360ToRgb(H, S, V);
         return new(color.R / 255.0f, color.G / 255.0f, color.B / 255.0f, (float)A);
      }
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

      public override Color4 ToColor4()
      {
         var r = (byte)((Value >> 16) & 0xFF);
         var g = (byte)((Value >> 8) & 0xFF);
         var b = (byte)(Value & 0xFF);
         return new(r / 255.0f, g / 255.0f, b / 255.0f);
      }
   }

   public static JominiColor Empty { get; } = new Rgb(49, 49, 49);

   public override int GetHashCode() => AsInt();
}