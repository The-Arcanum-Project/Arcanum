using Arcanum.Core.GameObjects.LocationCollections;
using Arcanum.Core.GameObjects.Religious;

namespace Arcanum.Core.CoreSystems.Map.MapModes.MapModeImplementations;

public class ReligionGroupMapMode : IMapMode
{
   public string Name => "Religion Group";
   public Type DisplayType => typeof(ReligionGroup);
   public MapModeManager.MapModeType Type => MapModeManager.MapModeType.ReligionGroup;
   public string Description => "Displays the religion group of each location on the map.";
   public string? IconSource => null;

   public int GetColorForLocation(Location location)
   {
      return location.TemplateData.Religion.Group.Color.AsInt();
   }

   public string[] GetTooltip(Location location)
      => [$"Religion Group: {location.TemplateData.Religion.Group.UniqueId}"];

   public string? GetLocationText(Location location) => null;

   public object?[]? GetVisualObject(Location location) => null;

   public void OnActivateMode()
   {
   }

   public void OnDeactivateMode()
   {
   }
}