using Arcanum.Core.CoreSystems.History;
using Arcanum.Core.GameObjects.LocationCollections;
using Arcanum.Core.Settings;

namespace Arcanum.Core.GlobalStates;

public enum AppState
{
   Error,
   Loading,
   EditingAllowed,
   EditingDisabled,
   Saving,
}

public static class Globals
{
   public static HashSet<Location> Locations { get; } = [];
}