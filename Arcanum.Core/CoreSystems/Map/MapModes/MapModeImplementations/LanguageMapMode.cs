using Arcanum.Core.GameObjects.LocationCollections;

namespace Arcanum.Core.CoreSystems.Map.MapModes.MapModeImplementations;

public class LanguageMapMode : IMapMode
{
   public string Name => "Language";
   public MapModeManager.MapModeType Type => MapModeManager.MapModeType.Language;
   public string Description => "Displays the primary language of each location on the map.";
   public string? IconSource => null;

   public int GetColorForLocation(Location location)
   {
      return location.TemplateData.Culture.Language.Color.AsInt();
   }

   public string[] GetTooltip(Location location) => [$"Language: {location.TemplateData.Culture.Language.UniqueId}"];

   public string? GetLocationText(Location location) => null;

   public object?[]? GetVisualObject(Location location) => null;

   public void OnActivateMode()
   {
   }

   public void OnDeactivateMode()
   {
   }
}