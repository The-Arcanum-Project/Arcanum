using Arcanum.Core.GameObjects.LocationCollections;

namespace Arcanum.Core.CoreSystems.Map.MapModes.MapModeImplementations;

public class BaseMapMode : IMapMode
{
   public string Name { get; } = "Base";
   public MapModeManager.MapModeType Type { get; } = MapModeManager.MapModeType.Base;
   public string Description { get; } = "The default map mode.";
   public string? IconSource { get; } = null;

   public int GetColorForLocation(Location location)
   {
      return location.Color.AsInt();
   }
}