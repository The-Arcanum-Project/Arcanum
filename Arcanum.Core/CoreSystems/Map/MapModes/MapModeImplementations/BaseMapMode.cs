using Arcanum.Core.GameObjects.LocationCollections;

namespace Arcanum.Core.CoreSystems.Map.MapModes.MapModeImplementations;

public class BaseMapMode : IMapMode
{
   public string Name => "Base";
   public MapModeManager.MapModeType Type => MapModeManager.MapModeType.Base;
   public string Description => "The default map mode.";
   public string? IconSource => null;

   public int GetColorForLocation(Location location)
   {
      return location.Color.AsInt();
   }

   public string[] GetTooltip(Location location) => [$"Color: {location.Color}"];

   public string? GetLocationText(Location location) => null;

   public object?[]? GetVisualObject(Location location) => null;

   public void OnActivateMode()
   {
   }

   public void OnDeactivateMode()
   {
   }
}