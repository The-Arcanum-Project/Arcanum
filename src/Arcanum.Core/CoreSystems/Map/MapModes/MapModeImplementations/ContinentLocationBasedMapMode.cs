using Arcanum.Core.GameObjects.LocationCollections;
using Arcanum.Core.GameObjects.LocationCollections.BaseClasses;
using Vortice.Mathematics;

namespace Arcanum.Core.CoreSystems.Map.MapModes.MapModeImplementations;

public class ContinentLocationBasedMapMode : IMapMode
{
   public string Name => "Continents";
   public string Description => "Displays the Continents the locations are situated in.";
   public MapModeManager.MapModeType Type => MapModeManager.MapModeType.Continents;
   public Type DisplayType => typeof(Continent);


   public void Render(Color4[] colorBuffer)
   {
      throw new NotImplementedException();
   }

   public string[] GetTooltip(Location location)
   {
      throw new NotImplementedException();
   }

   public string? GetLocationText(Location location)
   {
      throw new NotImplementedException();
   }

   public object?[]? GetVisualObject(Location location)
   {
      throw new NotImplementedException();
   }

   public void OnActivateMode()
   {
      throw new NotImplementedException();
   }

   public void OnDeactivateMode()
   {
      throw new NotImplementedException();
   }
}