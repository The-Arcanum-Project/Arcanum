#region

using System.Runtime.InteropServices;
using Arcanum.Core.GameObjects.InGame.Map.LocationCollections;
using Arcanum.Core.GameObjects.InGame.Pops;

#endregion

namespace Arcanum.Core.CoreSystems.Map.MapModes.MapModeImplementations;

public class PluralityPopType : LocationBasedMapMode
{
   public override string Name => "Plurality Pop Type";
   public override string Description => "Displays the plurality culture of each location on the map.";
   public override MapModeManager.MapModeType Type => MapModeManager.MapModeType.PluralityPopType;
   public override Type[] DisplayTypes { get; } = [typeof(PopType)];

   [ThreadStatic]
   private static Dictionary<PopType, double>? _cultureMap;

   public override int GetColorForLocation(Location location) => CalculatePluralityPopType(location).Color.AsInt();

   private static PopType CalculatePluralityPopType(Location location)
   {
      _cultureMap ??= new(8);
      _cultureMap.Clear();
      foreach (var pop in location.Pops)
      {
         ref var currentWeight = ref CollectionsMarshal.GetValueRefOrAddDefault(_cultureMap, pop.PopType, out _);
         currentWeight += pop.Size;
      }

      var bestPopType = PopType.Empty;
      var maxWeight = -1.0;

      foreach (var kvp in _cultureMap)
         if (kvp.Value > maxWeight)
         {
            maxWeight = kvp.Value;
            bestPopType = kvp.Key;
         }

      return bestPopType;
   }

   public override string[] GetTooltip(Location location)
   {
      var culture = CalculatePluralityPopType(location);
      return [$"Plurality Pop Type: {culture.UniqueId}"];
   }

   public override string? GetLocationText(Location location) => null;

   public override object?[]? GetVisualObject(Location location) => null;

   public override void OnActivateMode()
   {
   }

   public override void OnDeactivateMode()
   {
   }

   public override object GetLocationRelatedData(Location location) => CalculatePluralityPopType(location);
}