using Arcanum.Core.GameObjects.LocationCollections;
using Vortice.Mathematics;

namespace Arcanum.Core.CoreSystems.Map.MapModes;

public abstract class LocationBasedMapMode : IMapMode
{
   public abstract string Name { get; }
   public abstract string Description { get; }
   public abstract MapModeManager.MapModeType Type { get; }
   public abstract Type[] DisplayTypes { get; }

   public void Render(Color4[] colorBuffer)
   {
      var index = 0;
      foreach (var location in Globals.Locations.Values)
         colorBuffer[index++] = new (GetColorForLocation(location));
   }

   public abstract int GetColorForLocation(Location location);
   public abstract string[] GetTooltip(Location location);
   public abstract string? GetLocationText(Location location);
   public abstract object?[]? GetVisualObject(Location location);
   public abstract void OnActivateMode();
   public abstract void OnDeactivateMode();
}