using Arcanum.Core.CoreSystems.Parsing.ParsingHelpers.ArcColor;
using Arcanum.Core.GameObjects.Cultural;
using Arcanum.Core.GameObjects.LocationCollections;

namespace Arcanum.Core.CoreSystems.Map.MapModes.MapModeImplementations;

public class LanguageMapMode : LocationBasedMapMode
{
   public override string Name => "Language";
   public override Type[] DisplayTypes => [typeof(Language)];
   public override MapModeManager.MapModeType Type => MapModeManager.MapModeType.Language;
   public override string Description => "Displays the primary language of each location on the map.";
   public string? IconSource => null;

   public override int GetColorForLocation(Location location)
   {
      // TODO: this is very inefficient. Do an Aggregate link or smth here?
      var jomColor = location.TemplateData.Culture.Language.Color;
      if (jomColor == JominiColor.Empty)
      {
         var tlang = location.TemplateData.Culture.Language;
         foreach (var lang in Globals.Languages.Values)
         {
            if (lang.Dialects.Contains(tlang))
               jomColor = lang.Color;
         }
      }

      return jomColor.AsInt();
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