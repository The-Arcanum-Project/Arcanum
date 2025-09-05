using Arcanum.Core.CoreSystems.Common;

namespace Arcanum.Core.CoreSystems.Parsing.ParsingHelpers;

public static class ColorHelper
{
   public static bool IsValidColor(string color, LocationContext ctx)
   {
      // TODO: @Minnator implement parser for colors to allow validation
      return true;
   }

   public static void ParseAndSetColor(string color, LocationContext ctx, object obj, string propertyName = "Color")
   {
      if (!IsValidColor(color, ctx))
         return;

      var prop = obj.GetType().GetProperty(propertyName);
      if (prop is null || !prop.CanWrite || prop.PropertyType != typeof(string))
         throw new
            InvalidOperationException($"Property '{propertyName}' not found or not writable on type '{obj.GetType().Name}'.");

      prop.SetValue(obj, color);
   }
}