using Arcanum.Core.GameObjects.LocationCollections;

namespace Arcanum.Core.CoreSystems.Map.MapModes.MapModeImplementations;

public class VegetationMapMode : IMapMode
{
   public string Name => "Vegetation";
   public MapModeManager.MapModeType Type => MapModeManager.MapModeType.Vegetation;
   public string Description => "Displays the vegetation type of each location on the map.";
   public string? IconSource => null;

   public int GetColorForLocation(Location location)
   {
      return location.TemplateData.Vegetation.Color.AsInt();
   }
}