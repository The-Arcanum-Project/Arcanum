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

   public override string ToString() => $"{Name,15}: {From.Name,15} -> {To.Name,15} ({Type})";

   public override bool Equals(object? obj)
      => obj is Adjacency other && other.Name.Equals(Name, StringComparison.Ordinal);

   public override int GetHashCode() => Name.GetHashCode();
}