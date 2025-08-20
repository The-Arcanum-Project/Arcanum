namespace Arcanum.Core.GameObjects;

public interface IParseable<T> where T : class
{
   public bool Parse(string? str, out T? result);
}