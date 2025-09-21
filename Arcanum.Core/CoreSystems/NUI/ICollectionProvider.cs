using Arcanum.Core.GameObjects.BaseTypes;

namespace Arcanum.Core.CoreSystems.NUI;

/// <summary>
/// Defines an interface for providing a collection of items of type T which could be used anywhere this type is used.
/// </summary>
/// <typeparam name="T"></typeparam>
public interface ICollectionProvider<T> where T : notnull
{
   /// <summary>
   /// Gets a sequence of all globally available items of type T.
   /// </summary>
   /// <returns>An enumerable sequence of items.</returns>
   static abstract Dictionary<string, T> GetGlobalItems();
}

/// <summary>
/// Defines a contract for types that can provide a global collection of themselves.
/// </summary>
public interface IEu5ObjectProvider<T> where T : IEu5Object<T>, new()
{
   /// <summary>
   /// When implemented in a type, gets a sequence of all globally available items of that type.
   /// </summary>
   static abstract Dictionary<string, T> GetGlobalItems();
}