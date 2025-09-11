using Arcanum.Core.GameObjects;
using Color = System.Windows.Media.Color;

namespace Arcanum.Core.CoreSystems.Parsing.ParsingHelpers.ArcColor;

public abstract record JominiColor : IEmpty<JominiColor>
{
   public abstract Color ToMediaColor();
   public abstract JominiColorType Type { get; }

   public enum JominiColorType
   {
      Key,
      Rgb,
      Hsv,
      Hsv360,
   }

   private JominiColor()
   {
   }

   public sealed record ColorKey(string Key) : JominiColor
   {
      public override Color ToMediaColor() => ColorResolver.Instance.Resolve(Key);
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
      public override Color ToMediaColor() => HsvConverter.ToRgb(H, S, V);
      public override JominiColorType Type => JominiColorType.Hsv;
      public override string ToString() => $"hsv {{ {H:F2} {S:F2} {V:F2} }}";
   }

   public sealed record Hsv360(double H, double S, double V) : JominiColor
   {
      public override Color ToMediaColor()
      {
         // Convert scaled values to standard [0, 1] range before conversion.
         var standardS = S / 100.0;
         var standardV = V / 100.0;
         return HsvConverter.ToRgb(H, standardS, standardV);
      }

      public override JominiColorType Type => JominiColorType.Hsv360;
      public override string ToString() => $"hsv360 {{ {H:F0} {S:F0} {V:F0} }}";
   }

   public static JominiColor Empty { get; } = new ColorKey("ARCANUM_EMPTY_COLOR");
}