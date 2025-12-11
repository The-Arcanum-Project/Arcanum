using Arcanum.Core.GameObjects.LocationCollections;
using Arcanum.Core.GameObjects.Map;

namespace Arcanum.Core.CoreSystems.Map.MapModes.MapModeImplementations;

public class VegetationMapMode : LocationBasedMapMode
{
   public bool IsLandOnly => false;
   public override string Name => "Vegetation";
   public override Type[] DisplayTypes => [typeof(Vegetation)];
   public override MapModeManager.MapModeType Type => MapModeManager.MapModeType.Vegetation;
   public override string Description => "Displays the vegetation type of each location on the map.";
   public string? IconSource => null;

   public override int GetColorForLocation(Location location) => location.TemplateData.Vegetation.Color.AsInt();

   public override string[] GetTooltip(Location location) => [$"Vegetation: {location.TemplateData.Vegetation.UniqueId}"];

   public override string? GetLocationText(Location location) => null;

   public override object?[]? GetVisualObject(Location location) => null;

   public override void OnActivateMode()
   {
   }

   public override void OnDeactivateMode()
   {
   }
}