using Arcanum.Core.GameObjects.LocationCollections;
using Arcanum.Core.GameObjects.LocationCollections.BaseClasses;

namespace Arcanum.Core.CoreSystems.Map.MapModes.MapModeImplementations;

public class AreaMapMode : LocationBasedMapMode
{
   public override string Name => "Areas";
   public override string Description => "Displays the Areas the locations are situated in.";
   public override MapModeManager.MapModeType Type => MapModeManager.MapModeType.Areas;
   public override Type[] DisplayTypes => [typeof(Area), typeof(Province)];

   public override int GetColorForLocation(Location location)
   {
      var parent = location.GetFirstParentOfType(LocationCollectionType.Area);
      if (parent == null)
         return MapModeColorHelper.DEFAULT_EMPTY_COLOR;

      return ((IIndexRandomColor)parent).Color;
   }

   public override string[] GetTooltip(Location location) => ["Area: " + (location.GetFirstParentOfType(LocationCollectionType.Area)?.UniqueId ?? "None")];

   public override string? GetLocationText(Location location) => location.GetFirstParentOfType(LocationCollectionType.Area)?.UniqueId;

   public override object?[]? GetVisualObject(Location location) => null;

   public override void OnActivateMode()
   {
   }

   public override void OnDeactivateMode()
   {
   }
}