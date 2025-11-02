namespace Arcanum.Core.CoreSystems.Parsing.ParsingHelpers.ArcColor;

public class ColorResolver
{
   public readonly Dictionary<string, JominiColor> ColorMap = new();

   private static readonly Lazy<ColorResolver> LazyInstance = new(() => new());

   public static ColorResolver Instance => LazyInstance.Value;

   private ColorResolver()
   {
   }

   public bool TryAddColor(string key, JominiColor color)
   {
      return ColorMap.TryAdd(key, color);
   }

   public void ModifyColor(string key, JominiColor color)
   {
      if (ColorMap.ContainsKey(key))
         ColorMap[key] = color;
   }

   public void RemoveColor(string key)
   {
      ColorMap.Remove(key);
   }

   public JominiColor Resolve(string key)
   {
      return ColorMap.TryGetValue(key, out var color) ? color : JominiColor.Empty;
   }
}