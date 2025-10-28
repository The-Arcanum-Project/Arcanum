using Arcanum.Core.GameObjects.LocationCollections;

namespace Arcanum.Core.CoreSystems.Map.MapModes.MapModeImplementations;

public class GoodsMapMode : IMapMode
{
   public string Name => "Goods";
   public MapModeManager.MapModeType Type => MapModeManager.MapModeType.Goods;
   public string Description => "Displays the predominant goods produced in each location on the map.";
   public string? IconSource => null;

   public int GetColorForLocation(Location location)
   {
      return location.TemplateData.RawMaterial.Color.AsInt();
   }
}