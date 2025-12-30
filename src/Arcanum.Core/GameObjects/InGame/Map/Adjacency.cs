using System.ComponentModel;
using Arcanum.Core.CoreSystems.NUI;
using Arcanum.Core.GameObjects.BaseTypes;
using Arcanum.Core.GameObjects.InGame.Map.LocationCollections;

namespace Arcanum.Core.GameObjects.InGame.Map;

public enum AdjacencyType
{
   Sea,
}

public partial class Adjacency() : INUI, ICollectionProvider<Adjacency>, IEmpty<Adjacency>
{
   public Adjacency(Location from,
                    Location to,
                    AdjacencyType type,
                    string name,
                    string comment,
                    int startX,
                    int startY,
                    int endX,
                    int endY) : this()
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

   [Description("The location this adjacency starts from.")]
   [DefaultValue(null)]
   public Location From { get; set; } = null!;

   [Description("The location this adjacency goes to.")]
   [DefaultValue(null)]
   public Location To { get; set; } = null!;

   [Description("The type of adjacency.\nValid types: Sea, ")]
   [DefaultValue(AdjacencyType.Sea)]
   public AdjacencyType Type { get; set; }

   [Description("The unique name of this adjacency.")]
   [DefaultValue("")]
   public string Name { get; set; } = null!;

   [Description("A comment about this adjacency.")]
   [DefaultValue("")]
   public string Comment { get; set; } = null!;

   [Description("The starting X coordinate of this adjacency on the map.")]
   [DefaultValue(0)]
   public int StartX { get; set; }

   [Description("The starting Y coordinate of this adjacency on the map.")]
   [DefaultValue(0)]
   public int StartY { get; set; }

   [Description("The ending X coordinate of this adjacency on the map.")]
   [DefaultValue(0)]
   public int EndX { get; set; }

   [Description("The ending Y coordinate of this adjacency on the map.")]
   [DefaultValue(0)]
   public int EndY { get; set; }

   public int GetLength()
   {
      var dx = EndX - StartX;
      var dy = EndY - StartY;
      return (int)Math.Sqrt(dx * dx + dy * dy);
   }

   public override string ToString()
   {
      return $"{Name}: {From.UniqueId} -> {To.UniqueId} ({Type})";
   }

   public static Dictionary<string, Adjacency> GetGlobalItems() => Globals.Adjacencies;

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
   public NUISetting NUISettings { get; } = Config.Settings.NUIObjectSettings.AdjacencySettings;
   public INUINavigation[] Navigations => [new NUINavigation(From, $"From {From.UniqueId}"), new NUINavigation(To, $"To {To.UniqueId}")];
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