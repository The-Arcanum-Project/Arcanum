#region

using Arcanum.Core.GameObjects.InGame.Map.LocationCollections;
using Location = Arcanum.Core.GameObjects.InGame.Map.LocationCollections.Location;

#endregion

namespace Arcanum.Core.CoreSystems.Map.MapModes.MapModeImplementations;

public sealed class LocationRankMapMode : LocationBasedMapMode
{
   public override string Name => "Location Ranks";
   public override string Description => "Displays the rank of the location.";
   public override MapModeManager.MapModeType Type => MapModeManager.MapModeType.LocationRanks;
   public override Type[] DisplayTypes { get; } = [typeof(LocationRank), typeof(Location)];

   public override string[] GetTooltip(Location location) => [$"Location Rank: {location.Rank}"];

   public override string GetLocationText(Location location) => location.Rank.ToString();

   public override object?[]? GetVisualObject(Location location) => null;

   public override void OnActivateMode()
   {
   }

   public override void OnDeactivateMode()
   {
   }

   public override object GetLocationRelatedData(Location location) => location.Rank;

   public override int GetColorForLocation(Location location) => location.Rank.Color.AsInt();
}