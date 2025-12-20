using Arcanum.Core.GameObjects.InGame.Map.LocationCollections.BaseClasses;
using Area = Arcanum.Core.GameObjects.InGame.Map.LocationCollections.Area;
using Location = Arcanum.Core.GameObjects.InGame.Map.LocationCollections.Location;
using Region = Arcanum.Core.GameObjects.InGame.Map.LocationCollections.Region;

namespace Arcanum.Core.CoreSystems.Map.MapModes.MapModeImplementations;

public class RegionMapMode : LocationBasedMapMode
{
   public override string Name => "Regions";
   public override string Description => "Displays the Regions the locations are situated in.";
   public override MapModeManager.MapModeType Type => MapModeManager.MapModeType.Regions;
   public override Type[] DisplayTypes => [typeof(Region), typeof(Area)];

   public override int GetColorForLocation(Location location)
   {
      var parent = location.GetFirstParentOfType(LocationCollectionType.Region);
      if (parent == null)
         return MapModeColorHelper.DEFAULT_EMPTY_COLOR;

      return ((IIndexRandomColor)parent).Color;
   }

   public override bool IsLandOnly => false;
   public override string[] GetTooltip(Location location) => ["Region: " + (location.GetFirstParentOfType(LocationCollectionType.Region)?.UniqueId ?? "None")];

   public override string? GetLocationText(Location location) => location.GetFirstParentOfType(LocationCollectionType.Region)?.UniqueId;

   public override object?[]? GetVisualObject(Location location) => null;

   public override void OnActivateMode()
   {
   }

   public override void OnDeactivateMode()
   {
   }
}