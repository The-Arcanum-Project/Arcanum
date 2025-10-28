using Arcanum.Core.GameObjects.LocationCollections;

namespace Arcanum.Core.CoreSystems.Map.MapModes.MapModeImplementations;

public class TopographyMapMode : IMapMode
{
   public string Name => "Topography";
   public MapModeManager.MapModeType Type => MapModeManager.MapModeType.Topography;
   public string Description => "Displays the topography of each location on the map.";
   public string? IconSource => null;

   public int GetColorForLocation(Location location)
   {
      return location.TemplateData.Topography.Color.AsInt();
   }
}