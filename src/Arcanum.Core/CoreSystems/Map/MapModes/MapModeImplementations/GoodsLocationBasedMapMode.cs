using Arcanum.Core.GameObjects.Economy;
using Arcanum.Core.GameObjects.LocationCollections;

namespace Arcanum.Core.CoreSystems.Map.MapModes.MapModeImplementations;

public class GoodsLocationBasedMapMode : LocationBasedMapMode
{
   public override string Name => "Goods";
   public override Type DisplayType => typeof(RawMaterial);
   public override MapModeManager.MapModeType Type => MapModeManager.MapModeType.Goods;
   public override string Description => "Displays the predominant goods produced in each location on the map.";
   public string? IconSource => null;

   public override int GetColorForLocation(Location location)
   {
      return location.TemplateData.RawMaterial.Color.AsInt();
   }

   public override string[] GetTooltip(Location location) => [$"Goods: {location.TemplateData.RawMaterial.UniqueId}"];

   public override string? GetLocationText(Location location) => null;

   public override object?[]? GetVisualObject(Location location) => null;

   public override void OnActivateMode()
   {
   }

   public override void OnDeactivateMode()
   {
   }
}