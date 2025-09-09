using Arcanum.Core.CoreSystems.NUI;
using Arcanum.Core.GameObjects.LocationCollections;
using Arcanum.Core.GlobalStates;

namespace Arcanum.Core.GameObjects.Economy;

/// <summary>
/// Placeholder for the market type. Not sure how to do it yet.
/// </summary>
public partial class Market(Location location) : INUI, ICollectionProvider<Market>, IEmpty<Market>
{
   private Location _location = location;
   public Location Location
   {
      get => _location;
      set => _location = value;
   }
   public static Market Empty { get; } = new((Location)Location.Empty);

   public static IEnumerable<Market> GetGlobalItems() => Globals.Locations.Values.Where(loc => loc.HasMarket).Select(loc => loc.Market);

   public override bool Equals(object? obj) => obj is Market market && Location == market.Location;

   // ReSharper disable once NonReadonlyMemberInGetHashCode
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
   public NUISetting Settings { get; } = Config.Settings.NUIObjectSettings.MarketSettings;
   public INUINavigation[] Navigations { get; } = [];

   public override string ToString() => $"Market of {Location.Name}";
}