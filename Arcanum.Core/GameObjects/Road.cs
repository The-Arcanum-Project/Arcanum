using Arcanum.Core.CoreSystems.NUI;
using Arcanum.Core.GameObjects.LocationCollections;
using Arcanum.Core.GlobalStates;

namespace Arcanum.Core.GameObjects;

public partial class Road(Location startLocation, Location endLocation) : INUI, ICollectionProvider<Road>, IEmpty<Road>
{
   public Location StartLocation { get; set; } = startLocation;
   public Location EndLocation { get; set; } = endLocation;

   public bool IsReadonly => false;
   public NUISetting Settings { get; } = Config.Settings.NUIObjectSettings.RoadSettings;
   public INUINavigation[] Navigations =>
   [
      new NUINavigation(StartLocation, $"Start: {StartLocation.Name}"),
      new NUINavigation(EndLocation, $"End: {EndLocation.Name}"),
   ];
   public static IEnumerable<Road> GetGlobalItems() => Globals.Roads;
   public static Road Empty { get; } = new(Location.Empty, Location.Empty);
}