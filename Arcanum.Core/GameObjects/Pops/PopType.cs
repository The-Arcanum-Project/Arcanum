namespace Arcanum.Core.GameObjects.Pops;

public class PopType(string name,
                     string colorKey,
                     float foodConsumption,
                     float assimilationConversionFactor)
{
   public string Name { get; } = name;
   public string ColorKey { get; } = colorKey;
   public float FoodConsumption { get; } = foodConsumption;
   public float AssimilationConversionFactor { get; } = assimilationConversionFactor;

   public override string ToString()
   {
      return $"{Name} ({ColorKey})";
   }

   public override bool Equals(object? obj)
   {
      if (obj is PopType other)
         return Name == other.Name;

      return false;
   }

   public override int GetHashCode()
   {
      return Name.GetHashCode();
   }

   public static bool operator ==(PopType? left, PopType? right)
   {
      if (left is null && right is null)
         return true;

      if (left is null || right is null)
         return false;

      return left.Equals(right);
   }

   public static bool operator !=(PopType? left, PopType? right)
   {
      return !(left == right);
   }
}