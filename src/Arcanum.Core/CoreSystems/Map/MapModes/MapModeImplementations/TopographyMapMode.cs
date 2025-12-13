using Arcanum.Core.GameObjects.LocationCollections;
using Arcanum.Core.GameObjects.Map;

namespace Arcanum.Core.CoreSystems.Map.MapModes.MapModeImplementations;

public class TopographyMapMode : LocationBasedMapMode
{
   public bool IsLandOnly => false;
   public override Type[] DisplayTypes => [typeof(Topography)];
   public override string Name => "Topography";
   public override MapModeManager.MapModeType Type => MapModeManager.MapModeType.Topography;
   public override string Description => "Displays the topography of each location on the map.";
   public string? IconSource => null;

   public override int GetColorForLocation(Location location) => location.TemplateData.Topography.Color.AsInt();

   public override string[] GetTooltip(Location location) => [$"Topography: {location.TemplateData.Topography.UniqueId}"];

   public override string? GetLocationText(Location location) => null;

   public override object?[]? GetVisualObject(Location location) => null;

   public override void OnActivateMode()
   {
   }

   public override void OnDeactivateMode()
   {
   }
}