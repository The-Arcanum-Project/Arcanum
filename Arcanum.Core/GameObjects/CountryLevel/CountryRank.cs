using Arcanum.Core.CoreSystems.NUI;
using Arcanum.Core.CoreSystems.Parsing.ParsingHelpers.ArcColor;
using Arcanum.Core.GlobalStates;

namespace Arcanum.Core.GameObjects.CountryLevel;

public partial class CountryRank(string name) : INUI, ICollectionProvider<CountryRank>, IEmpty<CountryRank>
{
   public string Name { get; set; } = name;
   public int Level { get; set; }
   public JominiColor Color { get; set; } = JominiColor.Empty;

   public bool IsReadonly { get; } = true;
   public NUISetting Settings { get; } = Config.Settings.NUIObjectSettings.CountryRankSettings;
   public INUINavigation[] Navigations { get; } = [];
   public static IEnumerable<CountryRank> GetGlobalItems() => Globals.CountryRanks;

   public override string ToString() => Name;

   // ReSharper disable once NonReadonlyMemberInGetHashCode
   public override int GetHashCode() => Name.GetHashCode();

   public override bool Equals(object? obj) => obj is CountryRank cr &&
                                               string.Equals(cr.Name, Name, StringComparison.Ordinal);

   public static CountryRank Empty { get; } = new("empty");
}