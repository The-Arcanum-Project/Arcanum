namespace Arcanum.Core.CoreSystems.NUI;

/// <summary>
/// Defines an interface for providing a collection of items of type T which could be used anywhere this type is used.
/// </summary>
/// <typeparam name="T"></typeparam>
public interface ICollectionProvider<T>
{
   /// <summary>
   /// Gets a sequence of all globally available items of type T.
   /// </summary>
   /// <returns>An enumerable sequence of items.</returns>
   IEnumerable<T> GetGlobalItems();
}

public static class CollectionProviderExtensions
{
   /// <summary>
   /// Filters the sequence of global items based on a predicate.
   /// </summary>
   /// <typeparam name="T">The type of items.</typeparam>
   /// <param name="provider">The collection provider instance.</param>
   /// <param name="predicate">A function to test each element for a condition.</param>
   /// <returns>An <see cref="IEnumerable{T}"/> that contains elements from the input sequence that satisfy the condition.</returns>
   public static IEnumerable<T> GetGlobalItems<T>(this ICollectionProvider<T> provider, Func<T, bool> predicate)
   {
      // Note: We return IEnumerable<T> to allow further chaining.
      // The caller can use .ToList() if they need a list.
      return provider.GetGlobalItems().Where(predicate);
   }
}