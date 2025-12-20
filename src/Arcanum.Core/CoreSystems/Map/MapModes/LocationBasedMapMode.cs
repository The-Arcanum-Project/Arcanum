using Vortice.Mathematics;
using Location = Arcanum.Core.GameObjects.InGame.Map.LocationCollections.Location;

namespace Arcanum.Core.CoreSystems.Map.MapModes;

public abstract class LocationBasedMapMode : IMapMode
{
   public abstract string Name { get; }
   public abstract string Description { get; }
   public abstract MapModeManager.MapModeType Type { get; }
   public abstract Type[] DisplayTypes { get; }
   public virtual bool IsLandOnly => true;

   public void Render(Color4[] colorBuffer)
   {
      var array = MapModeManager.LocationsArray;
      for (var i = 0; i < array.Length; i++)
      {
         var location = array[i];
         colorBuffer[location.ColorIndex] = new(GetColorForLocation(location));
      }
   }

   public abstract int GetColorForLocation(Location location);
   public abstract string[] GetTooltip(Location location);
   public abstract string? GetLocationText(Location location);
   public abstract object?[]? GetVisualObject(Location location);
   public abstract void OnActivateMode();
   public abstract void OnDeactivateMode();
}