using Arcanum.Core.GameObjects.LocationCollections;
using Arcanum.Core.GameObjects.Religious;

namespace Arcanum.Core.CoreSystems.Map.MapModes.MapModeImplementations;

public class ReligionMapMode : IMapMode
{
   public string Name => "Religion";
   public Type DisplayType => typeof(Religion);
   public MapModeManager.MapModeType Type => MapModeManager.MapModeType.Religion;
   public string Description => "Displays the dominant religion of each location on the map.";
   public string? IconSource => null;

   public int GetColorForLocation(Location location) => location.TemplateData.Religion.Color.AsInt();

   public string[] GetTooltip(Location location) => [$"Religion: {location.TemplateData.Religion.UniqueId}"];

   public string? GetLocationText(Location location) => null;

   public object?[]? GetVisualObject(Location location) => null;

   public void OnActivateMode()
   {
   }

   public void OnDeactivateMode()
   {
   }
}