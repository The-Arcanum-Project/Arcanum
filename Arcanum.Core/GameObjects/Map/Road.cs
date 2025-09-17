using Arcanum.Core.CoreSystems.NUI;
using Arcanum.Core.CoreSystems.NUI.Attributes;
using Arcanum.Core.GameObjects.BaseTypes;
using Arcanum.Core.GameObjects.LocationCollections;
using Arcanum.Core.GlobalStates;

namespace Arcanum.Core.GameObjects.Map;

public partial class Road(Location startLocation, Location endLocation) : INUI, ICollectionProvider<Road>, IEmpty<Road>
{
   [BlockEmpty]
   public Location StartLocation { get; set; } = startLocation;
   [BlockEmpty]
   public Location EndLocation { get; set; } = endLocation;

   public bool IsReadonly => false;
   public NUISetting NUISettings { get; } = Config.Settings.NUIObjectSettings.RoadSettings;
   public INUINavigation[] Navigations =>
   [
      new NUINavigation(StartLocation, $"Start: {StartLocation.Name}"),
      new NUINavigation(EndLocation, $"End: {EndLocation.Name}"),
   ];
   public static IEnumerable<Road> GetGlobalItems() => Globals.Roads;
   public static Road Empty { get; } = new(Location.Empty, Location.Empty);
}