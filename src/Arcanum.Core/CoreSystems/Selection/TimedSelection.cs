using Location = Arcanum.Core.GameObjects.InGame.Map.LocationCollections.Location;

namespace Arcanum.Core.CoreSystems.Selection;

public struct TimedSelection(List<Location> locations, TimeSpan endTime, SelectionTarget target)
{
   public List<Location> Locations = locations;
   public TimeSpan EndTime = endTime;
   public SelectionTarget Target = target;
}