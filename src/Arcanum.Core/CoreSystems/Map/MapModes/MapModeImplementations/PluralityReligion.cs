using System.Runtime.InteropServices;
using Arcanum.Core.GameObjects.LocationCollections;
using Arcanum.Core.GameObjects.Religious;

namespace Arcanum.Core.CoreSystems.Map.MapModes.MapModeImplementations;

public class PluralityReligion : LocationBasedMapMode
{
   public override string Name => "Plurality Religion";
   public override string Description => "Displays the plurality culture of each location on the map.";
   public override MapModeManager.MapModeType Type => MapModeManager.MapModeType.PluralityReligion;
   public override Type[] DisplayTypes { get; } = [typeof(Religion)];

   [ThreadStatic]
   private static Dictionary<Religion, double>? _religionMap;

   public override int GetColorForLocation(Location location) => CalculatePluralityCulture(location).Color.AsInt();

   private static Religion CalculatePluralityCulture(Location location)
   {
      _religionMap ??= new(8);
      _religionMap.Clear();
      foreach (var pop in location.Pops)
      {
         // Using CollectionsMarshal (Available in .NET 5+) avoids double-hashing (Contains + Add)
         ref var currentWeight = ref CollectionsMarshal.GetValueRefOrAddDefault(_religionMap, pop.Religion, out _);
         currentWeight += pop.Size;
      }

      var bestReligion = Religion.Empty;
      var maxWeight = -1.0;

      foreach (var kvp in _religionMap)
         if (kvp.Value > maxWeight)
         {
            maxWeight = kvp.Value;
            bestReligion = kvp.Key;
         }

      return bestReligion;
   }

   public override string[] GetTooltip(Location location)
   {
      var culture = CalculatePluralityCulture(location);
      return [$"Plurality Religion: {culture.UniqueId}"];
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