using Arcanum.Core.GameObjects.LocationCollections;
using Arcanum.Core.GameObjects.Map;

namespace Arcanum.Core.CoreSystems.Map.MapModes.MapModeImplementations;

public class ClimateMapMode : IMapMode
{
   public Type DisplayType => typeof(Climate);
   public bool IsLandOnly => false;
   public string Name => "Climate";
   public MapModeManager.MapModeType Type => MapModeManager.MapModeType.Climate;
   public string Description => "Displays the climate of each location on the map.";
   public string? IconSource => null;

   public int GetColorForLocation(Location location)
   {
      return location.TemplateData.Climate.Color.AsInt();
   }

   public string[] GetTooltip(Location location) => [$"Climate: {location.TemplateData.Climate.UniqueId}"];

   public string? GetLocationText(Location location) => null;

   public object?[]? GetVisualObject(Location location) => null;

   public void OnActivateMode()
   {
   }

   public void OnDeactivateMode()
   {
   }
}