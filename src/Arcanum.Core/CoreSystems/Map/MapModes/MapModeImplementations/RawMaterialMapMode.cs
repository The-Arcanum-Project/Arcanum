using Arcanum.Core.CoreSystems.Parsing.ParsingHelpers.ArcColor;
using Arcanum.Core.Utils.Colors;
using Location = Arcanum.Core.GameObjects.InGame.Map.LocationCollections.Location;
using RawMaterial = Arcanum.Core.GameObjects.InGame.Economy.RawMaterial;

namespace Arcanum.Core.CoreSystems.Map.MapModes.MapModeImplementations;

public class RawMaterialMapMode : LocationBasedMapMode
{
   public override string Name => "Goods";
   public override Type[] DisplayTypes => [typeof(RawMaterial)];
   public override MapModeManager.MapModeType Type => MapModeManager.MapModeType.Goods;
   public override string Description => "Displays the predominant goods produced in each location on the map.";
   public string? IconSource => null;
   private bool _isInitialized;
   private int _emptyColor;

   public override int GetColorForLocation(Location location)
   {
      var color = location.TemplateData.RawMaterial.Color;
      if (color == JominiColor.Empty)
         return _emptyColor;

      return color.AsInt();
   }

   public override string[] GetTooltip(Location location) => [$"Goods: {location.TemplateData.RawMaterial.UniqueId}"];

   public override string? GetLocationText(Location location) => null;

   public override object?[]? GetVisualObject(Location location) => null;

   public override void OnActivateMode()
   {
      if (_isInitialized)
         return;

      var emptyColor = ColorGenerator.GetMostDistinctColor(Globals.RawMaterials.Values.Select(x => x.Color).ToList());
      _emptyColor = new JominiColor.Rgb(emptyColor.R, emptyColor.G, emptyColor.B).AsInt();
      _isInitialized = true;
   }

   public override void OnDeactivateMode()
   {
   }
}