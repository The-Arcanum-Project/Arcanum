using Arcanum.Core.GameObjects.InGame.Map.LocationCollections.BaseClasses;
using Area = Arcanum.Core.GameObjects.InGame.Map.LocationCollections.Area;
using Location = Arcanum.Core.GameObjects.InGame.Map.LocationCollections.Location;
using Province = Arcanum.Core.GameObjects.InGame.Map.LocationCollections.Province;

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
      if (parent == null!)
         return MapModeColorHelper.DEFAULT_EMPTY_COLOR;

      return ((IIndexRandomColor)parent).Color;
   }

   public override bool IsLandOnly => false;
   public override string[] GetTooltip(Location location) => ["Area: " + (location.GetFirstParentOfType(LocationCollectionType.Area).UniqueId)];

   public override string GetLocationText(Location location) => location.GetFirstParentOfType(LocationCollectionType.Area).UniqueId;

   public override object?[]? GetVisualObject(Location location) => null;

   public override void OnActivateMode()
   {
   }

   public override void OnDeactivateMode()
   {
   }
}