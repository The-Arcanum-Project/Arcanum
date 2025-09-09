using System.Diagnostics.CodeAnalysis;

namespace Arcanum.Core.GameObjects;

public interface IEmpty<out T> where T : IEmpty<T>
{
   public static abstract T Empty { get; }
}

public static class EmptyObjectRetriever
{
   /// <summary>
   /// Tries to get the static 'Empty' instance from a type that implements IEmpty
   /// .
   /// This method uses C# 11 generic constraints for compile-time safety and performance.
   /// </summary>
   /// <typeparam name="T">The type to check, which must implement IEmpty
   ///    .
   /// </typeparam>
   /// <param name="emptyInstance">When this method returns true, contains the 'Empty' instance of type T.</param>
   /// <returns>True if the type implements IEmpty
   ///    and the instance was retrieved; otherwise, false.
   /// </returns>
   public static bool TryGetEmpty<T>([MaybeNullWhen(false)] out T emptyInstance) where T : IEmpty<T>
   {
      // Because of the 'where T : IEmpty<T>' constraint, the compiler guarantees
      // that any type 'T' passed to this method will have a static 'Empty' property.
        
      emptyInstance = T.Empty;
      return true;
   }
}