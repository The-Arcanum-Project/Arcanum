using System.ComponentModel;
using System.Diagnostics;
using Arcanum.API.UtilServices.Search;
using Arcanum.Core.CoreSystems.Map.MapModes;
using Arcanum.Core.CoreSystems.NUI;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.ToolBox;
using Arcanum.Core.CoreSystems.Parsing.ParsingHelpers.ArcColor;
using Arcanum.Core.CoreSystems.SavingSystem.AGS;
using Arcanum.Core.CoreSystems.SavingSystem.AGS.Attributes;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.CoreSystems.Selection;
using Arcanum.Core.GameObjects.BaseTypes;
using Arcanum.Core.GameObjects.BaseTypes.InjectReplace;
using Arcanum.Core.GameObjects.InGame.Map;
using Arcanum.Core.GameObjects.InGame.Map.LocationCollections;
using Nexus.Core.Attributes;

namespace Arcanum.Core.GameObjects.InGame.Cultural;

[NexusConfig]
[ObjectSaveAs]
public partial class Language : IEu5Object<Language>, IMapInferable
{
   #region Nexus Properties

   [SaveAs]
   [DefaultValue(false)]
   [Description("Whether this language requires location names in the genitive case.")]
   [ParseAs("require_genitive_location_names")]
   public bool RequiredGenitiveLocationNames { get; set; }

   [SaveAs]
   [DefaultValue(false)]
   [Description("Whether this language is the default dialect for its parent language.")]
   [ParseAs("default")]
   public bool IsDefaultDialect { get; set; }

   [SaveAs]
   [DefaultValue("")]
   [Description("The string used when conjoining two first names, e.g. 'Jean-Luc'.")]
   [ParseAs("first_name_conjoiner")]
   public string FirstNameConjoiner { get; set; } = string.Empty;

   [SaveAs(SavingValueType.Identifier)]
   [DefaultValue("")]
   [Description("The string used for family names, e.g. 'de', 'von', 'bin', etc.")]
   [ParseAs("family")]
   public string Family { get; set; } = string.Empty;

   [SaveAs]
   [DefaultValue("")]
   [Description("The suffix applied to descendant names, e.g. 'son', 'dottir', etc.")]
   [ParseAs("descendant_suffix")]
   public string DescendantSuffix { get; set; } = string.Empty;

   [SaveAs]
   [DefaultValue("")]
   [Description("The prefix applied to descendant names, e.g. 'Mac', 'O'', etc.")]
   [ParseAs("descendant_prefix")]
   public string DescendantPrefix { get; set; } = string.Empty;

   [SaveAs]
   [DefaultValue("")]
   [Description("The prefix applied to descendant names for males, e.g. 'Mac', 'O'', etc.")]
   [ParseAs("descendant_prefix_male")]
   public string DescendantPrefixMale { get; set; } = string.Empty;

   [SaveAs]
   [DefaultValue("")]
   [Description("The prefix applied to descendant names for females, e.g. 'Nic', 'O'', etc.")]
   [ParseAs("descendant_prefix_female")]
   public string DescendantPrefixFemale { get; set; } = string.Empty;

   [SaveAs]
   [DefaultValue("")]
   [Description("The suffix applied to patronymic names, e.g. 'son', 'dottir', etc.")]
   [ParseAs("patronym_suffix")]
   public string PatronymSuffix { get; set; } = string.Empty;

   [SaveAs]
   [DefaultValue("")]
   [Description("The suffix applied to patronymic names for sons, e.g. 'son', 'dottir', etc.")]
   [ParseAs("patronym_suffix_son")]
   public string PatronymSuffixSon { get; set; } = string.Empty;

   [SaveAs]
   [DefaultValue("")]
   [Description("The suffix applied to patronymic names for daughters, e.g. 'son', 'dottir', etc.")]
   [ParseAs("patronym_suffix_daughter")]
   public string PatronymSuffixDaughter { get; set; } = string.Empty;

   [SaveAs]
   [DefaultValue("")]
   [Description("The prefix applied to patronymic names for sons, e.g. 'Mac', 'O'', etc.")]
   [ParseAs("patronym_prefix_son")]
   public string PatronymPrefixSon { get; set; } = string.Empty;

   [SaveAs]
   [DefaultValue("")]
   [Description("The prefix applied to patronymic names for sons when the father's name starts")]
   [ParseAs("patronym_prefix_son_vowel")]
   public string PatronymPrefixSonVowel { get; set; } = string.Empty;

   [SaveAs]
   [DefaultValue("")]
   [Description("The prefix applied to patronymic names for daughters when the father's name starts")]
   [ParseAs("patronym_prefix_daughter")]
   public string PatronymPrefixDaughter { get; set; } = string.Empty;

   [SaveAs]
   [DefaultValue("")]
   [Description("The prefix applied to patronymic names for daughters when the father's name starts with a vowel")]
   [ParseAs("patronym_prefix_daughter_vowel")]
   public string PatronymPrefixDaughterVowel { get; set; } = string.Empty;

   [SaveAs]
   [DefaultValue("")]
   [Description("The suffix applied to descendant names for males, e.g. 'son', 'dottir', etc.")]
   [ParseAs("descendant_suffix_male")]
   public string DescendantSuffixMale { get; set; } = string.Empty;

   [SaveAs]
   [DefaultValue("")]
   [Description("The suffix applied to descendant names for females, e.g. 'son', 'dottir', etc.")]
   [ParseAs("descendant_suffix_female")]
   public string DescendantSuffixFemale { get; set; } = string.Empty;

   [SaveAs]
   [DefaultValue("")]
   [Description("The format used for a short representation of a character's name with a regnal number, e.g. 'Louis XIV'.")]
   [ParseAs("character_name_short_regnal_number")]
   public string CharacterNameShortRegnalNumber { get; set; } = string.Empty;

   [SaveAs]
   [DefaultValue("")]
   [Description("How to order the parts of a character's name, e.g. 'first last', 'last, first', etc.")]
   [ParseAs("character_name_order")]
   public string CharacterNameOrder { get; set; } = string.Empty;

   [SaveAs]
   [DefaultValue("")]
   [Description("A prefix dependant on the origin location of the character")]
   [ParseAs("location_prefix")]
   public string LocationPrefix { get; set; } = string.Empty;

   [SaveAs]
   [DefaultValue("")]
   [Description("A prefix dependant on the origin location of the character, used when the character name starts with a vowel")]
   [ParseAs("location_prefix_vowel")]
   public string LocationPrefixVowel { get; set; } = string.Empty;

   [SaveAs]
   [DefaultValue("")]
   [Description("A suffix dependant on the origin location of the character")]
   [ParseAs("location_suffix")]
   public string LocationSuffix { get; set; } = string.Empty;

   [SaveAs]
   [DefaultValue(null)]
   [Description("The color of this language on the map")]
   [ParseAs("color")]
   public JominiColor Color { get; set; } = JominiColor.Empty;

   [SaveAs]
   [DefaultValue(false)]
   [Description(Globals.REPLACE_DESCRIPTION)]
   [ParseAs("location_prefix_elision", AstNodeType.BlockNode)]
   public ObservableRangeCollection<string> LocationPrefixElision { get; set; } = [];

   [SaveAs(SavingValueType.Identifier)]
   [DefaultValue(false)]
   [Description("A list of available male names for this language")]
   [ParseAs("male_names", AstNodeType.BlockNode)]
   public ObservableRangeCollection<string> MaleNames { get; set; } = [];

   [SaveAs(SavingValueType.Identifier)]
   [DefaultValue(false)]
   [Description("A list of available female names for this language")]
   [ParseAs("female_names", AstNodeType.BlockNode)]
   public ObservableRangeCollection<string> FemaleNames { get; set; } = [];

   [SaveAs(SavingValueType.Identifier)]
   [DefaultValue(false)]
   [Description("A list of available dynasty names for this language")]
   [ParseAs("dynasty_names", AstNodeType.BlockNode)]
   public ObservableRangeCollection<string> DynastyNames { get; set; } = [];

   [SaveAs(SavingValueType.Identifier)]
   [DefaultValue(false)]
   [Description("A list of available lowborn names for this language")]
   [ParseAs("lowborn", AstNodeType.BlockNode)]
   public ObservableRangeCollection<string> LowbornNames { get; set; } = [];

   [SaveAs(SavingValueType.Identifier)]
   [DefaultValue(false)]
   [Description("A list of available ship names for this language")]
   [ParseAs("ship_names", AstNodeType.BlockNode)]
   public ObservableRangeCollection<string> ShipNames { get; set; } = [];

   [SaveAs(SavingValueType.Identifier)]
   [DefaultValue(false)]
   [Description("A list of dynasty templates that can be used by this language")]
   [ParseAs("dynasty_template_keys", AstNodeType.BlockNode)]
   public ObservableRangeCollection<string> DynastyTemplateKeys { get; set; } = [];

   [SaveAs]
   [DefaultValue(false)]
   [Description("A list of dialects that are part of this language")]
   [ParseAs("dialects", isEmbedded: true, itemNodeType: AstNodeType.BlockNode)]
   public ObservableRangeCollection<Language> Dialects { get; set; } = [];

   [SaveAs]
   [DefaultValue("")]
   [Description("The language to fall back to if no other dialects are available.")]
   [ParseAs("fallback")]
   public string FallbackDialect { get; set; } = string.Empty; //TODO fix this to be embedded object

   [SaveAs]
   [DefaultValue("")]
   [Description("A prefix dependant on the origin location of the character, used for ancient names")]
   [ParseAs("location_prefix_ancient")]
   public string LocationPrefixAncient { get; set; } = string.Empty;

   [SaveAs]
   [DefaultValue("")]
   [Description("A prefix dependant on the origin location of the character, used for ancient names when the character name starts with a vowel")]
   [ParseAs("location_prefix_ancient_vowel")]
   public string LocationPrefixAncientVowel { get; set; } = string.Empty;

   #endregion

#pragma warning disable AGS004
   [Description("Unique key of this Language. Must be unique among all objects of this type.")]
   [DefaultValue("null")]
   public string UniqueId { get; set; } = null!;

   [SuppressAgs]
   public Eu5FileObj Source { get; set; } = Eu5FileObj.Empty;
   public Eu5ObjectLocation FileLocation { get; set; } = Eu5ObjectLocation.Empty;
#pragma warning restore AGS004

   #region IEu5Object

   public string GetNamespace => $"Culture.{nameof(Language)}";
   public void OnSearchSelected() => SelectionManager.Eu5ObjectSelectedInSearch(this);
   public ISearchResult VisualRepresentation => new SearchResultItem(null, UniqueId, GetNamespace.Replace('.', '>'));
   public Enum SearchCategory => IQueastorSearchSettings.DefaultCategories.GameObjects;
   public bool IsReadonly => false;
   public NUISetting NUISettings => Config.Settings.NUIObjectSettings.LanguageNuiSettings;
   public INUINavigation[] Navigations => [];
   public AgsSettings AgsSettings => Config.Settings.AgsSettings.LanguageAgsSettings;
   public InjRepType InjRepType { get; set; } = InjRepType.None;
   public static Dictionary<string, Language> GetGlobalItems() => Globals.Languages;

   public static Language Empty { get; } = new() { UniqueId = "Arcanum_Empty_Language" };

   #endregion

   #region IMapInferable

   public MapModeManager.MapModeType GetMapMode => MapModeManager.MapModeType.Language;

   public List<IEu5Object> GetInferredList(IEnumerable<Location> sLocs)
   {
      HashSet<IEu5Object> items = [];
      foreach (var loc in sLocs)
      {
         if (loc.TemplateData == LocationTemplateData.Empty && loc.TemplateData.Culture.Language != Empty)
            continue;

         items.Add(loc.TemplateData.Culture.Language);
      }

      return items.ToList();
   }

   public List<Location> GetRelevantLocations(IEu5Object[] items)
   {
      Debug.Assert(items.All(x => x is Language));
      var objs = items.Cast<Language>().ToArray();

      List<Location> locations = [];

      foreach (var loc in Globals.Locations.Values)
         if (objs.Contains(loc.TemplateData.Culture.Language) &&
             loc.TemplateData != LocationTemplateData.Empty &&
             loc.TemplateData.Culture.Language != Empty)
            locations.Add(loc);

      return locations;
   }

   #endregion
}