using Arcanum.Core.GameObjects.LocationCollections;
using Arcanum.Core.GameObjects.LocationCollections.BaseClasses;

namespace Arcanum.Core.CoreSystems.Map.MapModes.MapModeImplementations;

public class ContinentMapMode : IMapMode
{
   public string Name => "Continents";
   public string Description => "Displays the Continents the locations are situated in.";
   public MapModeManager.MapModeType Type => MapModeManager.MapModeType.Continents;
   public Type DisplayType => typeof(Continent);

   public int GetColorForLocation(Location location)
   {
      var parent = location.GetFirstParentOfType(LocationCollectionType.Continent);
      if (parent == null)
         return MapModeColorHelper.DEFAULT_EMPTY_COLOR;

      return ((IIndexRandomColor)parent).Color;
   }

   public string[] GetTooltip(Location location) =>
   [
      "Continent: " + (location.GetFirstParentOfType(LocationCollectionType.Continent)?.UniqueId ?? "None")
   ];

   public string? GetLocationText(Location location)
      => location.GetFirstParentOfType(LocationCollectionType.Continent)?.UniqueId;

   public object?[]? GetVisualObject(Location location) => null;

   public void OnActivateMode()
   {
   }

   public void OnDeactivateMode()
   {
   }
}