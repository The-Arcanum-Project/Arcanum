using Nexus.Core;

namespace Arcanum.Core.CoreSystems.Map;

public interface IIndexRandomColor
{
   [IgnoreModifiable]
   public int Index { get; set; }

   private static readonly System.Collections.Concurrent.ConcurrentDictionary<int, int> Cache = new();

   int Color => Cache.GetOrAdd(Index, MapModes.MapModeImplementations.MapModeColorHelper.GetRandomColor);
}