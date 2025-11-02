using Arcanum.Core.GameObjects.Cultural;
using Arcanum.Core.GameObjects.LocationCollections;

namespace Arcanum.Core.CoreSystems.Map.MapModes.MapModeImplementations;

public class CultureMapMode : IMapMode
{
   public string Name => "Culture";
   public Type DisplayType => typeof(Culture);
   public MapModeManager.MapModeType Type => MapModeManager.MapModeType.Culture;
   public string Description => "Displays the culture of each location on the map.";
   public string? IconSource => null;

   public int GetColorForLocation(Location location)
   {
      return location.TemplateData.Culture.Color.AsInt();
   }

   public string[] GetTooltip(Location location) => [$"Culture: {location.TemplateData.Culture.UniqueId}"];

   public string? GetLocationText(Location location) => null;

   public object?[]? GetVisualObject(Location location) => null;

   public void OnActivateMode()
   {
   }

   public void OnDeactivateMode()
   {
   }
}