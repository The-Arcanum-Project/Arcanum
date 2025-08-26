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
}