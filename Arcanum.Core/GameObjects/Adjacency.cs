using Arcanum.Core.GameObjects.LocationCollections;

namespace Arcanum.Core.GameObjects;

public enum AdjacencyType
{
   Sea,
}

public class Adjacency
{
   public Adjacency(Location from,
                    Location to,
                    AdjacencyType type,
                    string name,
                    string comment,
                    int startX,
                    int startY,
                    int endX,
                    int endY)
   {
      From = from;
      To = to;
      Type = type;
      Name = name;
      Comment = comment;
      StartX = startX;
      StartY = startY;
      EndX = endX;
      EndY = endY;
   }

   public Location From { get; }
   public Location To { get; }
   public AdjacencyType Type { get; }
   public string Name { get; }
   public string Comment { get; }
   public int StartX { get; }
   public int StartY { get; }
   public int EndX { get; }
   public int EndY { get; }

   public int GetLength()
   {
      var dx = EndX - StartX;
      var dy = EndY - StartY;
      return (int)Math.Sqrt(dx * dx + dy * dy);
   }
   
   public override string ToString() => $"{Name}: {From.Name} -> {To.Name} ({Type})";

   public override bool Equals(object? obj)
      => obj is Adjacency other && other.Name.Equals(Name, StringComparison.Ordinal);

   public override int GetHashCode() => Name.GetHashCode();
}