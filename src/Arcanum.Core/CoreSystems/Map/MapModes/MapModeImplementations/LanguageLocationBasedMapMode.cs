using Arcanum.Core.GameObjects.Cultural;
using Arcanum.Core.GameObjects.LocationCollections;

namespace Arcanum.Core.CoreSystems.Map.MapModes.MapModeImplementations;

public class LanguageLocationBasedMapMode : LocationBasedMapMode
{
   public override string Name => "Language";
   public override Type DisplayType => typeof(Language);
   public override MapModeManager.MapModeType Type => MapModeManager.MapModeType.Language;
   public override string Description => "Displays the primary language of each location on the map.";
   public string? IconSource => null;

   public override int GetColorForLocation(Location location)
   {
      return location.TemplateData.Culture.Language.Color.AsInt();
   }

   public override string[] GetTooltip(Location location) => [$"Language: {location.TemplateData.Culture.Language.UniqueId}"];

   public override string? GetLocationText(Location location) => null;

   public override object?[]? GetVisualObject(Location location) => null;

   public override void OnActivateMode()
   {
   }

   public override void OnDeactivateMode()
   {
   }
}