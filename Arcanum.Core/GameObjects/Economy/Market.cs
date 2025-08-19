namespace Arcanum.Core.GameObjects.Economy;

/// <summary>
/// Placeholder for the market type. Not sure how to do it yet.
/// </summary>
/// <param name="isEmpty"></param>
public class Market(bool isEmpty = true)
{
   public bool IsEmpty { get; } = isEmpty;
   public static Market Empty { get; } = new();
   public static Market Exists { get; } = new(false);

   public override bool Equals(object? obj) => obj is Market market && IsEmpty == market.IsEmpty;

   public override int GetHashCode() => IsEmpty.GetHashCode();

   public static bool operator ==(Market? left, Market? right)
   {
      if (left is null && right is null)
         return true;
      if (left is null || right is null)
         return false;

      return left.Equals(right);
   }

   public static bool operator !=(Market? left, Market? right) => !(left == right);
}