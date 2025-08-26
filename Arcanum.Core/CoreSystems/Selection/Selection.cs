using Arcanum.Core.GameObjects.LocationCollections;

namespace Arcanum.Core.CoreSystems.Selection;

public static class Selection
{
   public static ICollection<Location> SelectedLocations { get; private set; } = new List<Location>();
}