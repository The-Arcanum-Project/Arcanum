using Arcanum.Core.GameObjects.LocationCollections;

namespace Arcanum.Core.CoreSystems.Map.MapModes.MapModeImplementations;

public class BaseMapMode : IMapMode
{
   public string Name { get; } = "Base Map Mode";
   public MapModeManager.MapModeType Type { get; } = MapModeManager.MapModeType.BaseMapMode;
   public string Description { get; } = "The default map mode.";
   public string? IconSource { get; } = null;
   public int GetColorForLocation(Location location) => unchecked((int)0xFFFFFFFF); // White
}