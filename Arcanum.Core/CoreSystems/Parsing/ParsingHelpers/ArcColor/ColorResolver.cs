using System.Windows.Media;
using Color = System.Drawing.Color;

namespace Arcanum.Core.CoreSystems.Parsing.ParsingHelpers.ArcColor;

public class ColorResolver
{
   private readonly Dictionary<string, System.Windows.Media.Color> _colorMap = new();

   private static readonly Lazy<ColorResolver> LazyInstance = new(() => new());

   public static ColorResolver Instance => LazyInstance.Value;

   private ColorResolver()
   {
   }

   public void AddColor(string key, Color color)
   {
      var mediaColor = System.Windows.Media.Color.FromArgb(color.A, color.R, color.G, color.B);
      _colorMap[key] = mediaColor;
   }

   public void ModifyColor(string key, Color color)
   {
      if (_colorMap.ContainsKey(key))
      {
         var mediaColor = System.Windows.Media.Color.FromArgb(color.A, color.R, color.G, color.B);
         _colorMap[key] = mediaColor;
      }
   }

   public void RemoveColor(string key)
   {
      _colorMap.Remove(key);
   }

   public System.Windows.Media.Color Resolve(string key)
   {
      return _colorMap.TryGetValue(key, out var color) ? color : Colors.Magenta;
   }
}