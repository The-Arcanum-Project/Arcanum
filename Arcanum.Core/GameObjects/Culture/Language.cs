using Arcanum.Core.CoreSystems.Parsing.ParsingHelpers.ArcColor;
using Arcanum.Core.CoreSystems.Parsing.ToolBox;
using Arcanum.Core.CoreSystems.NUI;
using Arcanum.Core.GameObjects.BaseTypes;
using Arcanum.Core.GlobalStates;

namespace Arcanum.Core.GameObjects.Culture;

public partial class Language(string name) : OldNameKeyDefined(name),
                                             INUI,
                                             ICollectionProvider<Language>,
                                             IEmpty<Language>
{
   # region Nexus Properties

   [ParseAs("first_name_conjoiner")]
   public string FirstNameConjoiner { get; set; } = string.Empty;
   [ParseAs("family")]
   public string Family { get; set; } = string.Empty;
   [ParseAs("descendant_suffix")]
   public string DescendantSuffix { get; set; } = string.Empty;
   [ParseAs("descendant_prefix")]
   public string DescendantPrefix { get; set; } = string.Empty;
   [ParseAs("descendant_prefix_male")]
   public string DescendantPrefixMale { get; set; } = string.Empty;
   [ParseAs("descendant_prefix_female")]
   public string DescendantPrefixFemale { get; set; } = string.Empty;
   [ParseAs("patronym_suffix")]
   public string PatronymSuffix { get; set; } = string.Empty;
   [ParseAs("patronym_suffix_son")]
   public string PatronymSuffixSon { get; set; } = string.Empty;
   [ParseAs("patronym_suffix_daughter")]
   public string PatronymSuffixDaughter { get; set; } = string.Empty;
   [ParseAs("patronym_prefix_son")]
   public string PatronymPrefixSon { get; set; } = string.Empty;
   [ParseAs("patronym_prefix_son_vowel")]
   public string PatronymPrefixSonVowel { get; set; } = string.Empty;
   [ParseAs("patronym_prefix_daughter")]
   public string PatronymPrefixDaughter { get; set; } = string.Empty;
   [ParseAs("patronym_prefix_daughter_vowel")]
   public string PatronymPrefixDaughterVowel { get; set; } = string.Empty;
   [ParseAs("descendant_suffix_male")]
   public string DescendantSuffixMale { get; set; } = string.Empty;
   [ParseAs("descendant_suffix_female")]
   public string DescendantSuffixFemale { get; set; } = string.Empty;
   [ParseAs("character_name_short_regnal_number")]
   public string CharacterNameShortRegnalNumber { get; set; } = string.Empty;
   [ParseAs("character_name_order")]
   public string CharacterNameOrder { get; set; } = string.Empty;
   [ParseAs("location_prefix")]
   public string LocationPrefix { get; set; } = string.Empty;
   [ParseAs("location_prefix_vowel")]
   public string LocationPrefixVowel { get; set; } = string.Empty;
   [ParseAs("location_suffix")]
   public string LocationSuffix { get; set; } = string.Empty;
   [ParseAs("color")]
   public JominiColor Color { get; set; } = JominiColor.Empty;
   [ParseAs("location_prefix_elision", AstNodeType.BlockNode)]
   public ObservableRangeCollection<string> LocationPrefixElision { get; set; } = [];
   [ParseAs("male_names", AstNodeType.BlockNode)]
   public ObservableRangeCollection<string> MaleNames { get; set; } = [];
   [ParseAs("female_names", AstNodeType.BlockNode)]
   public ObservableRangeCollection<string> FemaleNames { get; set; } = [];
   [ParseAs("dynasty_names", AstNodeType.BlockNode)]
   public ObservableRangeCollection<string> DynastyNames { get; set; } = [];
   [ParseAs("lowborn", AstNodeType.BlockNode)]
   public ObservableRangeCollection<string> LowbornNames { get; set; } = [];
   [ParseAs("ship_names", AstNodeType.BlockNode)]
   public ObservableRangeCollection<string> ShipNames { get; set; } = [];
   [ParseAs("dynasty_template_keys", AstNodeType.BlockNode)]
   public ObservableRangeCollection<string> DynastyTemplateKeys { get; set; } = [];

   // This one is manually parsed.
   public ObservableRangeCollection<Language> Dialects { get; set; } = [];

   # endregion

   public bool IsReadonly => false;
   public NUISetting NUISettings { get; } = Config.Settings.NUIObjectSettings.LanguageNUI;
   public INUINavigation[] Navigations { get; } = [];
   public static IEnumerable<Language> GetGlobalItems() => Globals.Languages.Values;

   public static Language Empty { get; } = new("Arcanum_Language_Empty");

   #region ISearchable

   public override string GetNamespace => nameof(Language);

   #endregion
}