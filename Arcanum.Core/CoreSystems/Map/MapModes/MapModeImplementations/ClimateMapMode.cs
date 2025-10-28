using Arcanum.Core.GameObjects.LocationCollections;

namespace Arcanum.Core.CoreSystems.Map.MapModes.MapModeImplementations;

public class ClimateMapMode : IMapMode
{
   public string Name => "Climate";
   public MapModeManager.MapModeType Type => MapModeManager.MapModeType.Climate;
   public string Description => "Displays the climate of each location on the map.";
   public string? IconSource => null;

   public int GetColorForLocation(Location location)
   {
      return location.TemplateData.Climate.Color.AsInt();
   }
}