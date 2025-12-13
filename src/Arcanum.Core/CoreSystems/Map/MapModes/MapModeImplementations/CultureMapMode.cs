using Arcanum.Core.GameObjects.Cultural;
using Arcanum.Core.GameObjects.LocationCollections;

namespace Arcanum.Core.CoreSystems.Map.MapModes.MapModeImplementations;

public class CultureMapMode : LocationBasedMapMode
{
   public override string Name => "Culture";
   public override Type[] DisplayTypes => [typeof(Culture)];
   public override MapModeManager.MapModeType Type => MapModeManager.MapModeType.Culture;
   public override string Description => "Displays the culture of each location on the map.";
   public string? IconSource => null;

   public override int GetColorForLocation(Location location)
   {
      return location.TemplateData.Culture.Color.AsInt();
   }

   public override string[] GetTooltip(Location location) => [$"Culture: {location.TemplateData.Culture.UniqueId}"];

   public override string? GetLocationText(Location location) => null;

   public override object?[]? GetVisualObject(Location location) => null;

   public override void OnActivateMode()
   {
   }

   public override void OnDeactivateMode()
   {
   }
}