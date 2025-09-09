namespace Arcanum.Core.GameObjects;

public interface IEmpty<out T> where T : IEmpty<T>
{
   public static abstract T Empty { get; }
}