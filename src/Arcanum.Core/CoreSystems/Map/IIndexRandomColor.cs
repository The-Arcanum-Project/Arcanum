using Nexus.Core;

namespace Arcanum.Core.CoreSystems.Map;

public interface IIndexRandomColor
{
   /// <summary>
   /// The index used to generate a random color.<br/>
   /// Is assigned based on the line in the file it is first found at.
   /// </summary>
   [IgnoreModifiable]
   public int Index { get; set; }

   private static readonly System.Collections.Concurrent.ConcurrentDictionary<int, int> Cache = new();

   int Color => Cache.GetOrAdd(Index, MapModes.MapModeImplementations.MapModeColorHelper.GetMapColor(Index, true));
}