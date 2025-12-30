using Climate = Arcanum.Core.GameObjects.InGame.Map.Climate;
using Location = Arcanum.Core.GameObjects.InGame.Map.LocationCollections.Location;

namespace Arcanum.Core.CoreSystems.Map.MapModes.MapModeImplementations;

public class ClimateMapMode : LocationBasedMapMode
{
   public override Type[] DisplayTypes => [typeof(Climate)];
   public override bool IsLandOnly => false;
   public override string Name => "Climate";
   public override MapModeManager.MapModeType Type => MapModeManager.MapModeType.Climate;
   public override string Description => "Displays the climate of each location on the map.";
   public string? IconSource => null;

   public override int GetColorForLocation(Location location)
   {
      return location.TemplateData.Climate.Color.AsInt();
   }

   public override string[] GetTooltip(Location location) => [$"Climate: {location.TemplateData.Climate.UniqueId}"];

   public override string? GetLocationText(Location location) => null;

   public override object?[]? GetVisualObject(Location location) => null;

   public override void OnActivateMode()
   {
   }

   public override void OnDeactivateMode()
   {
   }
}