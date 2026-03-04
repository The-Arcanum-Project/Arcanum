using Common.UI.Map;

namespace Common.UI.Interfaces;

public interface IMapHandle
{
   public bool NotifyMapLoaded(MapParsingData data);
}