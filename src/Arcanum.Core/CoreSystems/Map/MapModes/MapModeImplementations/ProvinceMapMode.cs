using Arcanum.Core.GameObjects.LocationCollections;
using Arcanum.Core.GameObjects.LocationCollections.BaseClasses;

namespace Arcanum.Core.CoreSystems.Map.MapModes.MapModeImplementations;

public class ProvinceMapMode : IMapMode
{
   public string Name => "Provinces";
   public string Description => "Displays the provinces the locations are situated in.";
   public MapModeManager.MapModeType Type => MapModeManager.MapModeType.Provinces;
   public Type DisplayType => typeof(Province);

   public int GetColorForLocation(Location location)
   {
      var parent = location.GetFirstParentOfType(LocationCollectionType.Province);
      if (parent == null)
         return MapModeColorHelper.DEFAULT_EMPTY_COLOR;

      return ((IIndexRandomColor)parent).Color;
   }

   public string[] GetTooltip(Location location) =>
   [
      "Province: " + (location.GetFirstParentOfType(LocationCollectionType.Province)?.UniqueId ?? "None")
   ];

   public string? GetLocationText(Location location)
      => location.GetFirstParentOfType(LocationCollectionType.Province)?.UniqueId;

   public object?[]? GetVisualObject(Location location) => null;

   public void OnActivateMode()
   {
   }

   public void OnDeactivateMode()
   {
   }
}