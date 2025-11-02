using Arcanum.Core.GameObjects.LocationCollections;

namespace Arcanum.Core.CoreSystems.Selection;

public struct TimedSelection(List<Location> locations, TimeSpan endTime, SelectionTarget target)
{
   public List<Location> Locations = locations;
   public TimeSpan EndTime = endTime;
   public SelectionTarget Target = target;
}