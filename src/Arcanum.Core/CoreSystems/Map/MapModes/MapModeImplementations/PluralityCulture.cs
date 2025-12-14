using System.Runtime.InteropServices;
using Arcanum.Core.GameObjects.Cultural;
using Arcanum.Core.GameObjects.LocationCollections;

namespace Arcanum.Core.CoreSystems.Map.MapModes.MapModeImplementations;

public class PluralityCulture : LocationBasedMapMode
{
   public override string Name => "Plurality Culture";
   public override string Description => "Displays the plurality culture of each location on the map.";
   public override MapModeManager.MapModeType Type => MapModeManager.MapModeType.PluralityCulture;
   public override Type[] DisplayTypes { get; } = [typeof(Culture)];

   [ThreadStatic]
   private static Dictionary<Culture, double>? _cultureMap;

   public override int GetColorForLocation(Location location) => CalculatePluralityCulture(location).Color.AsInt();

   private static Culture CalculatePluralityCulture(Location location)
   {
      _cultureMap ??= new(8);
      _cultureMap.Clear();
      foreach (var pop in location.Pops)
      {
         // Using CollectionsMarshal (Available in .NET 5+) avoids double-hashing (Contains + Add)
         ref var currentWeight = ref CollectionsMarshal.GetValueRefOrAddDefault(_cultureMap, pop.Culture, out _);
         currentWeight += pop.Size;
      }

      var bestCulture = Culture.Empty;
      var maxWeight = -1.0;

      foreach (var kvp in _cultureMap)
         if (kvp.Value > maxWeight)
         {
            maxWeight = kvp.Value;
            bestCulture = kvp.Key;
         }

      return bestCulture;
   }

   public override string[] GetTooltip(Location location)
   {
      var culture = CalculatePluralityCulture(location);
      return [$"Plurality Culture: {culture.UniqueId}"];
   }

   public override string? GetLocationText(Location location) => null;

   public override object?[]? GetVisualObject(Location location) => null;

   public override void OnActivateMode()
   {
   }

   public override void OnDeactivateMode()
   {
   }
}