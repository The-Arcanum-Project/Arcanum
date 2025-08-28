namespace Arcanum.Core.GameObjects.LocationCollections;

public class Tag(string name)
{
   public string Name { get; set; } = name;

   public static Tag Empty { get; } = new(string.Empty);

   public override string ToString() => Name;

   public override bool Equals(object? obj)
   {
      if (obj is Tag other)
         return Name == other.Name;

      return false;
   }

   // ReSharper disable once NonReadonlyMemberInGetHashCode
   public override int GetHashCode() => Name.GetHashCode();
}