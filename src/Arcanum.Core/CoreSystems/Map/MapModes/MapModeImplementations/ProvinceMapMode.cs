using Arcanum.Core.GameObjects.InGame.Map.LocationCollections.BaseClasses;
using Location = Arcanum.Core.GameObjects.InGame.Map.LocationCollections.Location;
using Province = Arcanum.Core.GameObjects.InGame.Map.LocationCollections.Province;

namespace Arcanum.Core.CoreSystems.Map.MapModes.MapModeImplementations;

public class ProvinceMapMode : LocationBasedMapMode
{
   public override string Name => "Provinces";
   public override string Description => "Displays the provinces the locations are situated in.";
   public override MapModeManager.MapModeType Type => MapModeManager.MapModeType.Provinces;
   public override Type[] DisplayTypes => [typeof(Province), typeof(Location)];
   public override bool IsLandOnly => false;

   public override int GetColorForLocation(Location location)
   {
      var parent = location.GetFirstParentOfType(LocationCollectionType.Province);
      if (parent == null)
         return MapModeColorHelper.DEFAULT_EMPTY_COLOR;

      return ((IIndexRandomColor)parent).Color;
   }

   public override string[] GetTooltip(Location location) =>
   [
      "Province: " + (location.GetFirstParentOfType(LocationCollectionType.Province)?.UniqueId ?? "None")
   ];

   public override string? GetLocationText(Location location) => location.GetFirstParentOfType(LocationCollectionType.Province)?.UniqueId;

   public override object?[]? GetVisualObject(Location location) => null;

   public override void OnActivateMode()
   {
   }

   public override void OnDeactivateMode()
   {
   }
}