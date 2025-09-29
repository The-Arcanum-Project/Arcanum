using Arcanum.Core.GameObjects.LocationCollections;

namespace Arcanum.Core.CoreSystems.Selection;

public static class Selection
{
   public static List<Location> SelectedLocations { get; private set; } = [];
}