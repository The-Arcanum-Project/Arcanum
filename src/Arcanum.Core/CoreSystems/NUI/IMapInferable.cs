using Arcanum.Core.CoreSystems.Map.MapModes;
using Arcanum.Core.GameObjects.BaseTypes;
using Arcanum.Core.GameObjects.LocationCollections;

namespace Arcanum.Core.CoreSystems.NUI;

/// <summary>
/// Defines a contract for classes that can infer a list of items of type T based on a selection of locations.
/// </summary>
public interface IMapInferable
{
   /// <summary>
   /// This retrieves a list of items of type T based on the selection.
   /// </summary>
   /// <returns></returns>
   public List<IEu5Object> GetInferredList(IEnumerable<Location> sLocs);

   /// <summary>
   /// Returns a list of locations relevant to the provided items of type T.
   /// </summary>
   /// <param name="items"></param>
   /// <returns></returns>
   public List<Location> GetRelevantLocations(IEu5Object[] items);

   /// <summary>
   /// Returns the map mode type associated with this inferable.
   /// </summary>
   public MapModeManager.MapModeType GetMapMode { get; }
}