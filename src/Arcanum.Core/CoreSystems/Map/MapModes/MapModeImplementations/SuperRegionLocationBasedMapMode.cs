using Arcanum.Core.GameObjects.LocationCollections;
using Arcanum.Core.GameObjects.LocationCollections.BaseClasses;

namespace Arcanum.Core.CoreSystems.Map.MapModes.MapModeImplementations;

public class SuperRegionLocationBasedMapMode : LocationBasedMapMode
{
   public override string Name => "SuperRegions";
   public override string Description => "Displays the SuperRegions the locations are situated in.";
   public override MapModeManager.MapModeType Type => MapModeManager.MapModeType.SuperRegions;
   public override Type DisplayType => typeof(SuperRegion);

   public override int GetColorForLocation(Location location)
   {
      var parent = location.GetFirstParentOfType(LocationCollectionType.SuperRegion);
      if (parent == null)
         return MapModeColorHelper.DEFAULT_EMPTY_COLOR;

      return ((IIndexRandomColor)parent).Color;
   }

   public override string[] GetTooltip(Location location) =>
   [
      "SuperRegion: " + (location.GetFirstParentOfType(LocationCollectionType.SuperRegion)?.UniqueId ?? "None")
   ];

   public override string? GetLocationText(Location location) => location.GetFirstParentOfType(LocationCollectionType.SuperRegion)?.UniqueId;

   public override object?[]? GetVisualObject(Location location) => null;

   public override void OnActivateMode()
   {
   }

   public override void OnDeactivateMode()
   {
   }
}