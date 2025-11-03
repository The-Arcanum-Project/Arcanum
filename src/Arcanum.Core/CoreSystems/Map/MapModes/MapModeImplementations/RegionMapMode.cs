using Arcanum.Core.GameObjects.LocationCollections;
using Arcanum.Core.GameObjects.LocationCollections.BaseClasses;
using Region = System.Drawing.Region;

namespace Arcanum.Core.CoreSystems.Map.MapModes.MapModeImplementations;

public class RegionMapMode : IMapMode
{
   public string Name => "Regions";
   public string Description => "Displays the Regions the locations are situated in.";
   public MapModeManager.MapModeType Type => MapModeManager.MapModeType.Regions;
   public Type DisplayType => typeof(Region);

   public int GetColorForLocation(Location location)
   {
      var parent = location.GetFirstParentOfType(LocationCollectionType.Region);
      if (parent == null)
         return MapModeColorHelper.DEFAULT_EMPTY_COLOR;

      return ((IIndexRandomColor)parent).Color;
   }

   public string[] GetTooltip(Location location) =>
   [
      "Region: " + (location.GetFirstParentOfType(LocationCollectionType.Region)?.UniqueId ?? "None")
   ];

   public string? GetLocationText(Location location)
      => location.GetFirstParentOfType(LocationCollectionType.Region)?.UniqueId;

   public object?[]? GetVisualObject(Location location) => null;

   public void OnActivateMode()
   {
   }

   public void OnDeactivateMode()
   {
   }
}