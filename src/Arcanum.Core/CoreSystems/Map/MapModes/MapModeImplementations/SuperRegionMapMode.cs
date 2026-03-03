using Arcanum.Core.GameObjects.InGame.Map.LocationCollections;
using Arcanum.Core.GameObjects.InGame.Map.LocationCollections.BaseClasses;
using Location = Arcanum.Core.GameObjects.InGame.Map.LocationCollections.Location;
using Region = Arcanum.Core.GameObjects.InGame.Map.LocationCollections.Region;

namespace Arcanum.Core.CoreSystems.Map.MapModes.MapModeImplementations;

public class SuperRegionMapMode : LocationBasedMapMode
{
   public override string Name => "SubContinents";
   public override string Description => "Displays the SubContinents the locations are situated in.";
   public override MapModeManager.MapModeType Type => MapModeManager.MapModeType.SubContinents;
   public override Type[] DisplayTypes => [typeof(SubContinent), typeof(Region), typeof(Area), typeof(Province), typeof(Location)];

   public override int GetColorForLocation(Location location)
   {
      var parent = location.GetFirstParentOfType(LocationCollectionType.SuperRegion);
      if (parent == null!)
         return MapModeColorHelper.DEFAULT_EMPTY_COLOR;

      return ((IIndexRandomColor)parent).Color;
   }

   public override bool IsLandOnly => false;

   public override string[] GetTooltip(Location location) => ["SubContinent: " + (location.GetFirstParentOfType(LocationCollectionType.SuperRegion).UniqueId),];

   public override string GetLocationText(Location location) => location.GetFirstParentOfType(LocationCollectionType.SuperRegion).UniqueId;

   public override object?[]? GetVisualObject(Location location) => null;

   public override void OnActivateMode()
   {
   }

   public override void OnDeactivateMode()
   {
   }

   public override object GetLocationRelatedData(Location location) => location.Province.Area.Region.SubContinent;
}