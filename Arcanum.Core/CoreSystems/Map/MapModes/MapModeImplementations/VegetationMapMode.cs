using Arcanum.Core.GameObjects.LocationCollections;

namespace Arcanum.Core.CoreSystems.Map.MapModes.MapModeImplementations;

public class VegetationMapMode : IMapMode
{
   public bool IsLandOnly => false;
   public string Name => "Vegetation";
   public MapModeManager.MapModeType Type => MapModeManager.MapModeType.Vegetation;
   public string Description => "Displays the vegetation type of each location on the map.";
   public string? IconSource => null;

   public int GetColorForLocation(Location location) => location.TemplateData.Vegetation.Color.AsInt();

   public string[] GetTooltip(Location location) => [$"Vegetation: {location.TemplateData.Vegetation.UniqueId}"];

   public string? GetLocationText(Location location) => null;

   public object?[]? GetVisualObject(Location location) => null;

   public void OnActivateMode()
   {
   }

   public void OnDeactivateMode()
   {
   }
}