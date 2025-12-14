using Arcanum.Core.GameObjects.LocationCollections;
using Arcanum.Core.GameObjects.LocationCollections.BaseClasses;

namespace Arcanum.Core.CoreSystems.Map.MapModes.MapModeImplementations;

public class ContinentMapMode : LocationBasedMapMode
{
   public override string Name => "Continents";
   public override string Description => "Displays the Continents the locations are situated in.";
   public override MapModeManager.MapModeType Type => MapModeManager.MapModeType.Continents;
   public override Type[] DisplayTypes => [typeof(Continent), typeof(SuperRegion)];
   public override bool IsLandOnly => false;

   public override int GetColorForLocation(Location location)
   {
      var parent = location.GetFirstParentOfType(LocationCollectionType.Continent);
      if (parent == null)
         return MapModeColorHelper.DEFAULT_EMPTY_COLOR;

      return ((IIndexRandomColor)parent).Color;
   }

   public override string[] GetTooltip(Location location) =>
   [
      "Continent: " + (location.GetFirstParentOfType(LocationCollectionType.Continent)?.UniqueId ?? "None")
   ];

   public override string? GetLocationText(Location location) => location.GetFirstParentOfType(LocationCollectionType.Continent)?.UniqueId;

   public override object?[]? GetVisualObject(Location location) => null;

   public override void OnActivateMode()
   {
   }

   public override void OnDeactivateMode()
   {
   }
}