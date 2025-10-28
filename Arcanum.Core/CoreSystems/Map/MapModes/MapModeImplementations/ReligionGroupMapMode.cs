using Arcanum.Core.GameObjects.LocationCollections;

namespace Arcanum.Core.CoreSystems.Map.MapModes.MapModeImplementations;

public class ReligionGroupMapMode : IMapMode
{
   public string Name => "Religion Group";
   public MapModeManager.MapModeType Type => MapModeManager.MapModeType.ReligionGroup;
   public string Description => "Displays the religion group of each location on the map.";
   public string? IconSource => null;

   public int GetColorForLocation(Location location)
   {
      return location.TemplateData.Religion.Group.Color.AsInt();
   }
}