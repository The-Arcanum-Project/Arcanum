using Arcanum.Core.CoreSystems.NUI;
using Arcanum.Core.GameObjects.LocationCollections;
using Arcanum.Core.GlobalStates;

namespace Arcanum.Core.GameObjects.Economy;

/// <summary>
/// Placeholder for the market type. Not sure how to do it yet.
/// </summary>
public partial class Market(Location location) : INUI
{
   public Location Location { get; set; } = location;
   public static Market Empty { get; } = new((Location)Location.Empty);

   public override bool Equals(object? obj) => obj is Market market && Location == market.Location;

   public override int GetHashCode() => Location.GetHashCode();

   public static bool operator ==(Market? left, Market? right)
   {
      if (left is null && right is null)
         return true;
      if (left is null || right is null)
         return false;

      return left.Equals(right);
   }

   public static bool operator !=(Market? left, Market? right) => !(left == right);
   public bool IsReadonly { get; } = true;
   public NUISetting Settings { get; } = Config.Settings.NUISettings.MarketSettings;
   public INUINavigation[] Navigations { get; } = [];
}