using Arcanum.Core.CoreSystems.NUI;
using Arcanum.Core.GlobalStates;

namespace Arcanum.Core.GameObjects;

public partial class LocationRank(string name, int order) : INUI, ICollectionProvider<LocationRank>
{
   public string Name { get; set; } = name;
   public string ColorKey { get; set; } = string.Empty;
   public bool IsMaxRank { get; set; }
   /// <summary>
   /// LocationRanks are ordered by appearance in the file, meaning the first one is the best and everything beneath is worse.
   /// </summary>
   public int Order { get; set; } = order;

   public bool IsReadonly => true;
   public NUISetting Settings { get; } = Config.Settings.NUIObjectSettings.LocationRankSettings;
   public INUINavigation[] Navigations { get; } = [];
   public static IEnumerable<LocationRank> GetGlobalItems() => Globals.LocationRanks;

   public override string ToString() => $"({Order}) {Name}";

   public override bool Equals(object? obj)
   {
      if (obj is LocationRank other)
         return Name == other.Name;

      return false;
   }

   // ReSharper disable once NonReadonlyMemberInGetHashCode
   public override int GetHashCode() => Name.GetHashCode();
}