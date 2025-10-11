using Arcanum.Core.CoreSystems.Map.MapModes;
using Arcanum.Core.GameObjects.LocationCollections;

namespace Arcanum.Core.CoreSystems.NUI;

/// <summary>
/// Defines a contract for classes that can infer a list of items of type T based on a selection of locations.
/// </summary>
/// <typeparam name="T"></typeparam>
public interface IMapInferable<T> : IHasMapMode
{
   /// <summary>
   /// This retrieves a list of items of type T based on the selection.
   /// </summary>
   /// <returns></returns>
   public static abstract List<T> GetInferredList(IEnumerable<Location> sLocs);

   /// <summary>
   /// Returns a list of locations relevant to the provided items of type T.
   /// </summary>
   /// <param name="items"></param>
   /// <returns></returns>
   public static abstract List<Location> GetRelevantLocations(IEnumerable<T> items);
}