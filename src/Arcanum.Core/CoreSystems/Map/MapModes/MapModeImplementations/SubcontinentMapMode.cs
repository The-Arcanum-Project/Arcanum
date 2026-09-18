#region

using Arcanum.Core.GameObjects.InGame.Map.LocationCollections;
using Arcanum.Core.GameObjects.InGame.Map.LocationCollections.BaseClasses;
using Location = Arcanum.Core.GameObjects.InGame.Map.LocationCollections.Location;
using Region = Arcanum.Core.GameObjects.InGame.Map.LocationCollections.Region;

#endregion

namespace Arcanum.Core.CoreSystems.Map.MapModes.MapModeImplementations;

public sealed class SubcontinentMapMode : LocationBasedMapMode
{
   public override string Name => "Subcontinent";
   public override string Description => "Displays the Subcontinents the locations are situated in.";
   public override MapModeManager.MapModeType Type => MapModeManager.MapModeType.Subcontinent;
   public override Type[] DisplayTypes => [typeof(SubContinent), typeof(Region), typeof(Area), typeof(Province), typeof(Location)];
   public override bool DarkenWastelands => false;

   public override int GetColorForLocation(Location location)
   {
      var parent = location.GetFirstParentOfType(LocationCollectionType.SuperRegion);
      if (parent == null!)
         return MapModeColorHelper.DEFAULT_EMPTY_COLOR;

      return ((IIndexRandomColor)parent).Color;
   }

   public override bool IsLandOnly => false;

   public override string[] GetTooltip(Location location) => ["Subcontinent: " + location.GetFirstParentOfType(LocationCollectionType.SuperRegion).UniqueId];

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