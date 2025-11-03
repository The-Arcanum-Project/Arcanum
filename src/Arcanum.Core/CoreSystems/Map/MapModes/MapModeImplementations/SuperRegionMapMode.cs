using Arcanum.Core.GameObjects.LocationCollections;
using Arcanum.Core.GameObjects.LocationCollections.BaseClasses;

namespace Arcanum.Core.CoreSystems.Map.MapModes.MapModeImplementations;

public class SuperRegionMapMode : IMapMode
{
   public string Name => "SuperRegions";
   public string Description => "Displays the SuperRegions the locations are situated in.";
   public MapModeManager.MapModeType Type => MapModeManager.MapModeType.SuperRegions;
   public Type DisplayType => typeof(SuperRegion);

   public int GetColorForLocation(Location location)
   {
      var parent = location.GetFirstParentOfType(LocationCollectionType.SuperRegion);
      if (parent == null)
         return MapModeColorHelper.DEFAULT_EMPTY_COLOR;

      return ((IIndexRandomColor)parent).Color;
   }

   public string[] GetTooltip(Location location) =>
   [
      "SuperRegion: " + (location.GetFirstParentOfType(LocationCollectionType.SuperRegion)?.UniqueId ?? "None")
   ];

   public string? GetLocationText(Location location)
      => location.GetFirstParentOfType(LocationCollectionType.SuperRegion)?.UniqueId;

   public object?[]? GetVisualObject(Location location) => null;

   public void OnActivateMode()
   {
   }

   public void OnDeactivateMode()
   {
   }
}