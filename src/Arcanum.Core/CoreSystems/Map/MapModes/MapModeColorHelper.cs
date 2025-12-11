using Arcanum.Core.CoreSystems.Parsing.ParsingHelpers.ArcColor;
using Arcanum.Core.Utils.Colors;
using Vortice.Mathematics;

namespace Arcanum.Core.CoreSystems.Map.MapModes;

public static class MapModeColorHelper
{
   public const int DEFAULT_EMPTY_COLOR = unchecked((int)0xFF313131);
   private const int SEED_MULTIPLIER_PRIME = 397;
   private const float COLOR_MIN_SATURATION = 0.35f;
   private const float COLOR_MAX_SATURATION = 1;
   private const float COLOR_MIN_VALUE = 0.7f;
   private const float COLOR_MAX_VALUE = 1.0f;

   public static Color4 GetEmptyColor4 { get; } = new ((DEFAULT_EMPTY_COLOR >> 24) & 0xFF,
                                                       (DEFAULT_EMPTY_COLOR >> 16) & 0xFF,
                                                       (DEFAULT_EMPTY_COLOR >> 8) & 0xFF,
                                                       DEFAULT_EMPTY_COLOR & 0xFF);

   public static int GetRandomColor(int seed)
   {
      var random = new Random(seed * SEED_MULTIPLIER_PRIME);
      var hue = random.NextSingle() * 360f;
      var saturation = COLOR_MIN_SATURATION + random.NextSingle() * (COLOR_MAX_SATURATION - COLOR_MIN_SATURATION);
      var value = COLOR_MIN_VALUE + random.NextSingle() * (COLOR_MAX_VALUE - COLOR_MIN_VALUE);

      var color = HsvConverter.Hsv360ToRgb(hue, saturation, value);
      return (color.A << 24) | (color.B << 16) | (color.G << 8) | color.R;
   }

   /// <summary>
   /// Generates a deterministic color for a map entity, using pre-defined values
   /// that are then scaled to fit a specific saturation and value range.
   /// </summary>
   /// <param name="index">A unique, non-negative integer for the entity (e.g., province ID).</param>
   /// <param name="isLand">True to use the land color palette, False to use the water palette.</param>
   /// <returns>An integer representing the ARGB color, with the byte order BGR.</returns>
   public static int GetMapColor(int index, bool isLand)
   {
      var hue = (float)ColorGenerator.GetHue(isLand, index);
      var baseSaturation = ColorGenerator.GetSaturation(index) / 100.0f;
      var baseValue = ColorGenerator.GetValue(index) / 100.0f;

      // This maps the [0.0, 1.0] range from the array to [MIN, MAX].
      var finalSaturation = COLOR_MIN_SATURATION + baseSaturation * (COLOR_MAX_SATURATION - COLOR_MIN_SATURATION);
      var finalValue = COLOR_MIN_VALUE + baseValue * (COLOR_MAX_VALUE - COLOR_MIN_VALUE);

      var color = HsvConverter.Hsv360ToRgb(hue, finalSaturation, finalValue);

      return unchecked((int)0xFF000000) | (color.B << 16) | (color.G << 8) | color.R;
   }
}