using Arcanum.Core.GameObjects.LocationCollections;
using Arcanum.Core.Utils.Colors;

namespace Arcanum.Core.CoreSystems.Map.MapModes.MapModeImplementations;

public class NaturalHarborSuitabilityMapMode : IMapMode
{
   public string Name { get; } = "Natural Harbor Suitability";
   public string Description { get; } = "Displays the natural harbor suitability of each coastal location on the map.";
   public MapModeManager.MapModeType Type { get; } = MapModeManager.MapModeType.NaturalHarborSuitability;
   public Type DisplayType { get; } = typeof(float);

   public int GetColorForLocation(Location location)
   {
      return ColorGenerator.GetRedGreenGradientInverse(location.TemplateData.NaturalHarborSuitability).AsAbgrInt();
   }

   public string[] GetTooltip(Location location)
   {
      return [$"Natural Harbor Suitability: {(location.TemplateData.NaturalHarborSuitability * 100):F2}%"];
   }

   public string? GetLocationText(Location location) => null;

   public object?[]? GetVisualObject(Location location) => null;

   public void OnActivateMode()
   {
   }

   public void OnDeactivateMode()
   {
   }
}