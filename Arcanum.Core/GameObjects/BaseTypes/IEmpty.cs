namespace Arcanum.Core.GameObjects.BaseTypes;

public interface IEmpty<out T> where T : IEmpty<T>
{
   public static abstract T Empty { get; }
}