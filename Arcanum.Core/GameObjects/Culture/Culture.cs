using System.ComponentModel;
using Arcanum.API.UtilServices.Search;
using Arcanum.Core.CoreSystems.Jomini.Modifiers;
using Arcanum.Core.CoreSystems.NUI;
using Arcanum.Core.CoreSystems.NUI.Attributes;
using Arcanum.Core.CoreSystems.Parsing.ParsingHelpers.ArcColor;
using Arcanum.Core.CoreSystems.Parsing.ToolBox;
using Arcanum.Core.CoreSystems.SavingSystem.AGS;
using Arcanum.Core.CoreSystems.SavingSystem.AGS.Attributes;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.GameObjects.BaseTypes;
using Arcanum.Core.GameObjects.Culture.SubObjects;
using Common.UI;

namespace Arcanum.Core.GameObjects.Culture;

public enum Opinion
{
   [EnumAgsData("enemy")]
   Enemy,

   [EnumAgsData("negative")]
   Negative,

   [EnumAgsData("neutral")]
   Neutral,

   [EnumAgsData("positive")]
   Positive,

   [EnumAgsData("kindred")]
   Kindred,
}

[ObjectSaveAs]
public partial class Culture : IEu5Object<Culture>
{
   #region Nexus Properties

   [SaveAs]
   [DefaultValue("")]
   [ParseAs("language")]
   [Description("The language or dialect of this culture.")]
   public Language Language { get; set; } = Language.Empty;

   [SaveAs]
   [DefaultValue(null)]
   [ParseAs("color")]
   [Description("The color of this culture.")]
   public JominiColor Color { get; set; } = JominiColor.Empty;

   [SaveAs]
   [DefaultValue("")]
   [ParseAs("dynasty_name_type")]
   [Description("The type of family names this culture uses.")]
   public string DynastyNameType { get; set; } = string.Empty;

   [SaveAs]
   [DefaultValue(false)]
   [ParseAs("use_patronym")]
   [Description("If this culture uses patronyms instead of family names.")]
   public bool UsePatronym { get; set; }

   [SaveAs]
   [DefaultValue(null)]
   [ParseAs("opinions", itemNodeType: AstNodeType.ContentNode)]
   [Description("Opinions towards other cultures.")]
   public ObservableRangeCollection<CultureOpinionValue> Opinions { get; set; } = [];

   [SaveAs]
   [DefaultValue(null)]
   [ParseAs("culture_groups")]
   [Description("The groups this culture belongs to.")]
   public ObservableRangeCollection<string> CultureGroups { get; set; } = [];

   [SaveAs]
   [DefaultValue(null)]
   [ParseAs("tags")]
   [Description("The tags this culture belongs to.\nConvention is to put the more unique ones first and less unique ones last.")]
   public ObservableRangeCollection<string> GfxTags { get; set; } = [];

   [SaveAs]
   [DefaultValue(null)]
   [ParseAs("noun_keys")]
   [Description("The noun keys this culture uses.")]
   public ObservableRangeCollection<string> NounKeys { get; set; } = [];

   [SaveAs]
   [DefaultValue(null)]
   [ParseAs("adjective_keys")]
   [Description("The adjective keys this culture uses.")]
   public ObservableRangeCollection<string> AdjectiveKeys { get; set; } = [];

   [SaveAs]
   [DefaultValue(null)]
   [ParseAs("country_modifier", itemNodeType: AstNodeType.ContentNode)]
   [Description("Modifiers applied to countries of this culture.")]
   public ObservableRangeCollection<ModValInstance> CountryModifiers { get; set; } = [];

   [SaveAs]
   [DefaultValue(null)]
   [ParseAs("location_modifier", itemNodeType: AstNodeType.ContentNode)]
   [Description("Modifiers applied to locations of this culture.")]
   public ObservableRangeCollection<ModValInstance> LocationModifiers { get; set; } = [];

   [SaveAs]
   [DefaultValue(null)]
   [ParseAs("character_modifier", itemNodeType: AstNodeType.ContentNode)]
   [Description("Modifiers applied to characters of this culture.")]
   public ObservableRangeCollection<ModValInstance> CharacterModifiers { get; set; } = [];

   #endregion

#pragma warning disable AGS004
   [ReadonlyNexus]
   [Description("Unique key of this Culture. Must be unique among all objects of this type.")]
   [DefaultValue("null")]
   public string UniqueId { get; set; } = null!;

   [SuppressAgs]
   public Eu5FileObj Source { get; set; } = null!;
#pragma warning restore AGS004

   #region IEu5Object

   public string GetNamespace => $"{nameof(Culture)}";
   public void OnSearchSelected() => UIHandle.Instance.PopUpHandle.OpenPropertyGridWindow(this);
   public ISearchResult VisualRepresentation => new SearchResultItem(null, UniqueId, string.Empty);
   public IQueastorSearchSettings.Category SearchCategory => IQueastorSearchSettings.Category.GameObjects;
   public bool IsReadonly => true;
   public NUISetting NUISettings => Config.Settings.NUIObjectSettings.CultureSettings;
   public INUINavigation[] Navigations => [new NUINavigation(Language == Language.Empty ? null : Language, "Language")];
   public AgsSettings AgsSettings => Config.Settings.AgsSettings.CultureAgsSettings;
   public static Dictionary<string, Culture> GetGlobalItems() => Globals.Cultures;

   public static Culture Empty { get; } = new() { UniqueId = "Arcanum_Empty_Culture" };

   #endregion

   public override string ToString() => UniqueId;
}