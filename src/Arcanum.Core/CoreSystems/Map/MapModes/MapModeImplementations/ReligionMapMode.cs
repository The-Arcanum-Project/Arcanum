using Arcanum.Core.GameObjects.LocationCollections;
using Arcanum.Core.GameObjects.Religious;

namespace Arcanum.Core.CoreSystems.Map.MapModes.MapModeImplementations;

public class ReligionMapMode : LocationBasedMapMode
{
   public override string Name => "Religion";
   public override Type[] DisplayTypes => [typeof(Religion)];
   public override MapModeManager.MapModeType Type => MapModeManager.MapModeType.Religion;
   public override string Description => "Displays the dominant religion of each location on the map.";
   public string? IconSource => null;

   public override int GetColorForLocation(Location location) => location.TemplateData.Religion.Color.AsInt();

   public override string[] GetTooltip(Location location) => [$"Religion: {location.TemplateData.Religion.UniqueId}"];

   public override string? GetLocationText(Location location) => null;

   public override object?[]? GetVisualObject(Location location) => null;

   public override void OnActivateMode()
   {
   }

   public override void OnDeactivateMode()
   {
   }
}