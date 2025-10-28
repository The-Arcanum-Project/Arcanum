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
}