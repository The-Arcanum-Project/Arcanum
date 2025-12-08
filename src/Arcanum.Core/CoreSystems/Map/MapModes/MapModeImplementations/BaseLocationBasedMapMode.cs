using Arcanum.Core.GameObjects.LocationCollections;

namespace Arcanum.Core.CoreSystems.Map.MapModes.MapModeImplementations;

public class BaseLocationBasedMapMode : LocationBasedMapMode
{
   public override string Name => "Locations";
   public override MapModeManager.MapModeType Type => MapModeManager.MapModeType.Locations;
   public override Type DisplayType => typeof(Location);
   public override string Description => "The default map mode.";
   public string? IconSource => null;

   public override int GetColorForLocation(Location location)
   {
      return location.Color.AsInt();
   }

   public bool IsLandOnly => false;

   public override string[] GetTooltip(Location location) => [$"Color: {location.Color}"];

   public override string? GetLocationText(Location location) => null;

   public override object?[]? GetVisualObject(Location location) => null;

   public override void OnActivateMode()
   {
   }

   public override void OnDeactivateMode()
   {
   }
}