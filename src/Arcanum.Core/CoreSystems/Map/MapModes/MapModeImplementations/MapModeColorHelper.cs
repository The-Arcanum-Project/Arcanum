using Arcanum.Core.CoreSystems.Parsing.ParsingHelpers.ArcColor;

namespace Arcanum.Core.CoreSystems.Map.MapModes.MapModeImplementations;

public static class MapModeColorHelper
{
   public const int DEFAULT_EMPTY_COLOR = unchecked((int)0xFF313131);
   private const int SEED_MULTIPLIER_PRIME = 397;
   private const float COLOR_MIN_SATURATION = 0.35f;
   private const float COLOR_MAX_SATURATION = 1;
   private const float COLOR_MIN_VALUE = 0.7f;
   private const float COLOR_MAX_VALUE = 1.0f;

   public static int GetRandomColor(int seed)
   {
      var random = new Random(seed * SEED_MULTIPLIER_PRIME);
      var hue = random.NextSingle() * 360f;
      var saturation = COLOR_MIN_SATURATION + random.NextSingle() * (COLOR_MAX_SATURATION - COLOR_MIN_SATURATION);
      var value = COLOR_MIN_VALUE + random.NextSingle() * (COLOR_MAX_VALUE - COLOR_MIN_VALUE);

      var color = HsvConverter.Hsv360ToRgb(hue, saturation, value);
      return (color.A << 24) | (color.B << 16) | (color.G << 8) | color.R;
   }
}