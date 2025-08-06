namespace Arcanum.Core.Utils.Sorting;

public interface IDependencyNode<T>
{
   /// <summary>
   /// Unique identifier for this node.
   /// </summary>
   T Id { get; }

   /// <summary>
   /// Identifiers of the dependencies this node requires.
   /// </summary>
   IEnumerable<T> Dependencies { get; }
}