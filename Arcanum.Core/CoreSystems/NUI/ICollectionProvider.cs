namespace Arcanum.Core.CoreSystems.NUI;

/// <summary>
/// Defines an interface for providing a collection of items of type T which could be used anywhere this type is used.
/// </summary>
/// <typeparam name="T"></typeparam>
public interface ICollectionProvider<out T> where T : notnull
{
   /// <summary>
   /// Gets a sequence of all globally available items of type T.
   /// </summary>
   /// <returns>An enumerable sequence of items.</returns>
   static abstract IEnumerable<T> GetGlobalItems();
}
