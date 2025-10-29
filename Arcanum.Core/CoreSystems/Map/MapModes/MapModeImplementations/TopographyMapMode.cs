using Arcanum.Core.GameObjects.LocationCollections;

namespace Arcanum.Core.CoreSystems.Map.MapModes.MapModeImplementations;

public class TopographyMapMode : IMapMode
{
   public bool IsLandOnly => false;
   public string Name => "Topography";
   public MapModeManager.MapModeType Type => MapModeManager.MapModeType.Topography;
   public string Description => "Displays the topography of each location on the map.";
   public string? IconSource => null;

   public int GetColorForLocation(Location location) => location.TemplateData.Topography.Color.AsInt();

   public string[] GetTooltip(Location location) => [$"Topography: {location.TemplateData.Topography.UniqueId}"];

   public string? GetLocationText(Location location) => null;

   public object?[]? GetVisualObject(Location location) => null;

   public void OnActivateMode()
   {
   }

   public void OnDeactivateMode()
   {
   }
}