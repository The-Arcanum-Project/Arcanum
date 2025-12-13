using Arcanum.Core.GameObjects.LocationCollections;
using Arcanum.Core.Utils.Colors;

namespace Arcanum.Core.CoreSystems.Map.MapModes.MapModeImplementations;

public class NaturalHarborSuitabilityMapMode : LocationBasedMapMode
{
   public override string Name { get; } = "Natural Harbor Suitability";
   public override string Description { get; } = "Displays the natural harbor suitability of each coastal location on the map.";
   public override MapModeManager.MapModeType Type { get; } = MapModeManager.MapModeType.NaturalHarborSuitability;
   public override Type[] DisplayTypes { get; } = [typeof(float)];

   public override int GetColorForLocation(Location location)
   {
      return ColorGenerator.GetRedGreenGradientInverse(location.TemplateData.NaturalHarborSuitability).AsAbgrInt();
   }

   public override string[] GetTooltip(Location location)
   {
      return [$"Natural Harbor Suitability: {(location.TemplateData.NaturalHarborSuitability * 100):F2}%"];
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