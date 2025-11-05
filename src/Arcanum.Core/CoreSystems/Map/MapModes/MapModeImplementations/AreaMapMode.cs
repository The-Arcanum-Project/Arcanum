using Arcanum.Core.GameObjects.LocationCollections;
using Arcanum.Core.GameObjects.LocationCollections.BaseClasses;

namespace Arcanum.Core.CoreSystems.Map.MapModes.MapModeImplementations;

public class AreaMapMode : IMapMode
{
   public string Name => "Areas";
   public string Description => "Displays the Areas the locations are situated in.";
   public MapModeManager.MapModeType Type => MapModeManager.MapModeType.Areas;
   public Type DisplayType => typeof(Area);

   public int GetColorForLocation(Location location)
   {
      var parent = location.GetFirstParentOfType(LocationCollectionType.Area);
      if (parent == null)
         return MapModeColorHelper.DEFAULT_EMPTY_COLOR;

      return ((IIndexRandomColor)parent).Color;
   }

   public string[] GetTooltip(Location location) =>
   [
      "Area: " + (location.GetFirstParentOfType(LocationCollectionType.Area)?.UniqueId ?? "None")
   ];

   public string? GetLocationText(Location location)
      => location.GetFirstParentOfType(LocationCollectionType.Area)?.UniqueId;

   public object?[]? GetVisualObject(Location location) => null;

   public void OnActivateMode()
   {
   }

   public void OnDeactivateMode()
   {
   }
}