using Arcanum.Core.GameObjects.LocationCollections;
using Arcanum.Core.GameObjects.Map;

namespace Arcanum.Core.CoreSystems.Map.MapModes.MapModeImplementations;

public class ReligionMapMode : IMapMode
{
   public string Name => "Religion";
   public MapModeManager.MapModeType Type => MapModeManager.MapModeType.Religion;
   public string Description => "Displays the dominant religion of each location on the map.";
   public string? IconSource => null;

   public int GetColorForLocation(Location location)
   {
      return location.TemplateData.Religion.Color.AsInt();
   }
}