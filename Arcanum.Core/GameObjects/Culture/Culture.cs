using System.ComponentModel;
using Arcanum.Core.CoreSystems.NUI;
using Arcanum.Core.CoreSystems.NUI.Attributes;
using Arcanum.Core.CoreSystems.Parsing.ParsingHelpers.ArcColor;
using Arcanum.Core.GlobalStates;

namespace Arcanum.Core.GameObjects.Culture;

public partial class Culture(string name) : INUI, IEmpty<Culture>, ICollectionProvider<Culture>
{
   # region Nexus Properties

   [Description("The name of this culture.")]
   [ReadonlyNexus]
   public string Name { get; set; } = name;
   [Description("The language or dialect of this culture.")]
   public string Language { get; set; } = string.Empty;
   [Description("The color of this culture.")]
   public JominiColor Color { get; set; } = JominiColor.Empty;
   [Description("The type of family names this culture uses.")]
   public string DynastyNameType { get; set; } = string.Empty;

   [Description("If this culture uses patronyms instead of family names.")]
   public bool UsePatronym { get; set; } = false;

   [Description("Opinions towards other cultures.")]
   public ObservableRangeCollection<KeyValuePair<Culture, SimpleOpinion>> Opinions { get; set; } = [];
   [Description("The groups this culture belongs to.")]
   public ObservableRangeCollection<string> CultureGroups { get; set; } = [];
   [Description("The tags this culture belongs to.\nConvention is to put the more unique ones first and less unique ones last.")]
   public ObservableRangeCollection<string> GfxTags { get; set; } = [];
   [Description("The noun keys this culture uses.")]
   public ObservableRangeCollection<string> NounKeys { get; set; } = [];
   [Description("The adjective keys this culture uses.")]
   public ObservableRangeCollection<string> AdjectiveKeys { get; set; } = [];

   # endregion

   public bool IsReadonly => false;
   public NUISetting NUISettings { get; } = Config.Settings.NUIObjectSettings.CultureSettings;
   public INUINavigation[] Navigations { get; } = [];
   public static Culture Empty { get; } = new("Arcanum_Empty_Culture");
   public static IEnumerable<Culture> GetGlobalItems() => Globals.Cultures.Values;

   public override string ToString() => Name;
}