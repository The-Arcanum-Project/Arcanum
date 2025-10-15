using Arcanum.Core.GameObjects.LocationCollections;

namespace Arcanum.Core.CoreSystems.Map.MapModes.MapModeImplementations;

public class None : IMapMode
{
   public string Name => "None";
   public MapModeManager.MapModeType Type => MapModeManager.MapModeType.None;
   public string Description => "The default map mode.";
   public string? IconSource => null;

   public int GetColorForLocation(Location location)
   {
      return (int)((location.Bounds.X * 37 + location.Bounds.Y * 57) % 0xFFFFFF);
   }
}