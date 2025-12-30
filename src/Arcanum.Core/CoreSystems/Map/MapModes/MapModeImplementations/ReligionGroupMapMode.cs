using Location = Arcanum.Core.GameObjects.InGame.Map.LocationCollections.Location;
using ReligionGroup = Arcanum.Core.GameObjects.InGame.Religious.ReligionGroup;

namespace Arcanum.Core.CoreSystems.Map.MapModes.MapModeImplementations;

public class ReligionGroupMapMode : LocationBasedMapMode
{
   public override string Name => "Religion Group";
   public override Type[] DisplayTypes => [typeof(ReligionGroup)];
   public override MapModeManager.MapModeType Type => MapModeManager.MapModeType.ReligionGroup;
   public override string Description => "Displays the religion group of each location on the map.";
   public string? IconSource => null;

   public override int GetColorForLocation(Location location)
   {
      return location.TemplateData.Religion.Group.Color.AsInt();
   }

   public override string[] GetTooltip(Location location) => [$"Religion Group: {location.TemplateData.Religion.Group.UniqueId}"];

   public override string? GetLocationText(Location location) => null;

   public override object?[]? GetVisualObject(Location location) => null;

   public override void OnActivateMode()
   {
   }

   public override void OnDeactivateMode()
   {
   }
}