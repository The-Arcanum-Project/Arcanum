using System.ComponentModel;
using Arcanum.Core.CoreSystems.NUI;
using Arcanum.Core.GameObjects.LocationCollections;
using Arcanum.Core.GlobalStates;

namespace Arcanum.Core.GameObjects.Map;

public enum AdjacencyType
{
   Sea,
}

public partial class Adjacency(Location from,
                               Location to,
                               AdjacencyType type,
                               string name,
                               string comment,
                               int startX,
                               int startY,
                               int endX,
                               int endY) : INUI, ICollectionProvider<Adjacency>, IEmpty<Adjacency>
{
   [Description("The location this adjacency starts from.")]
   public Location From { get; set; } = from;
   [Description("The location this adjacency goes to.")]
   public Location To { get; set; } = to;
   [Description("The type of adjacency.\nValid types: Sea, ")]
   public AdjacencyType Type { get; set; } = type;
   [Description("The unique name of this adjacency.")]
   public string Name { get; set; } = name;
   [Description("A comment about this adjacency.")]
   public string Comment { get; set; } = comment;
   [Description("The starting X coordinate of this adjacency on the map.")]
   public int StartX { get; set; } = startX;
   [Description("The starting Y coordinate of this adjacency on the map.")]
   public int StartY { get; set; } = startY;
   [Description("The ending X coordinate of this adjacency on the map.")]
   public int EndX { get; set; } = endX;
   [Description("The ending Y coordinate of this adjacency on the map.")]
   public int EndY { get; set; } = endY;

   public int GetLength()
   {
      var dx = EndX - StartX;
      var dy = EndY - StartY;
      return (int)Math.Sqrt(dx * dx + dy * dy);
   }

   public override string ToString()
   {
      return $"{Name}: {From.Name} -> {To.Name} ({Type})";
   }

   public static IEnumerable<Adjacency> GetGlobalItems() => Globals.Adjacencies;

   public override bool Equals(object? obj)
   {
      if (obj is null)
         return false;

      if (ReferenceEquals(this, obj))
         return true;

      return obj is Adjacency other && other.Name.Equals(Name, StringComparison.Ordinal);
   }

   // ReSharper disable once NonReadonlyMemberInGetHashCode
   public override int GetHashCode() => Name.GetHashCode();
   public bool IsReadonly => false;
   public NUISetting Settings { get; } = Config.Settings.NUIObjectSettings.AdjacencySettings;
   public INUINavigation[] Navigations
      => [new NUINavigation(From, $"From {From.Name}"), new NUINavigation(To, $"To {To.Name}")];
   public static Adjacency Empty { get; } = new(Location.Empty,
                                                Location.Empty,
                                                AdjacencyType.Sea,
                                                "Empty_Adjacency",
                                                string.Empty,
                                                0,
                                                0,
                                                0,
                                                0);
}