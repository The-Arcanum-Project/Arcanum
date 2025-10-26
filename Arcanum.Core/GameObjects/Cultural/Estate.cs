using System.ComponentModel;
using Arcanum.API.UtilServices.Search;
using Arcanum.Core.CoreSystems.Jomini.Modifiers;
using Arcanum.Core.CoreSystems.NUI;
using Arcanum.Core.CoreSystems.NUI.Attributes;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.ToolBox;
using Arcanum.Core.CoreSystems.Parsing.ParsingHelpers.ArcColor;
using Arcanum.Core.CoreSystems.SavingSystem.AGS;
using Arcanum.Core.CoreSystems.SavingSystem.AGS.Attributes;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.GameObjects.BaseTypes;
using Common.UI;

namespace Arcanum.Core.GameObjects.Cultural;

[ObjectSaveAs]
public partial class Estate : IEu5Object<Estate>
{
   public enum RevoltCourtLanguage
   {
      [EnumAgsData("court_language")]
      CourtLanguage,

      [EnumAgsData("liturgical_language")]
      LiturgicalLanguage,

      [EnumAgsData("common_language")]
      CommonLanguage,
   }

   #region Nexus Properties

   [SaveAs]
   [ParseAs("color")]
   [DefaultValue(null)]
   [Description("The name of the Estate as shown in-game.")]
   public JominiColor Color { get; set; } = JominiColor.Empty;

   [SaveAs]
   [ParseAs("power_per_pop")]
   [DefaultValue(0f)]
   [Description("The power per pop value of this Estate.")]
   public float PowerPerPop { get; set; }

   [SaveAs]
   [ParseAs("tax_per_pop")]
   [DefaultValue(0f)]
   [Description("The tax per pop value of this Estate.")]
   public float TaxPerPop { get; set; }

   [SaveAs]
   [ParseAs("rival")]
   [DefaultValue(0f)]
   [Description(Globals.REPLACE_DESCRIPTION)]
   public float Rival { get; set; }

   [SaveAs]
   [ParseAs("alliance")]
   [DefaultValue(0f)]
   [Description(Globals.REPLACE_DESCRIPTION)]
   public float Alliance { get; set; }

   [SaveAs]
   [ParseAs("revolt_court_language")]
   [DefaultValue(RevoltCourtLanguage.CourtLanguage)]
   [Description("The court language used during revolts associated with this Estate.")]
   public RevoltCourtLanguage RevoltCourtLanguageSetting { get; set; } = RevoltCourtLanguage.CourtLanguage;

   [SaveAs]
   [ParseAs("priority_for_dynasty_head")]
   [DefaultValue(false)]
   [Description("When calculating a new dynasty head, the dynasty will check characters of this estate first before falling back to the other estates")]
   public bool PriorityForDynastyHead { get; set; }

   [ParseAs("can_spawn_random_characters")]
   [SaveAs]
   [DefaultValue(true)]
   [Description("If false, characters of this estate will not spawn randomly.")]
   public bool CanSpawnRandomCharacters { get; set; } = true;

   [SaveAs]
   [ParseAs("can_have_characters")]
   [DefaultValue(false)]
   [Description("If false, characters cannot belong to this estate.")]
   public bool CanHaveCharacters { get; set; } = true;

   [SaveAs]
   [ParseAs("ruler_opinion_modifier")]
   [DefaultValue(false)]
   [Description("If true, characters of this estate will give a ruler opinion modifier.")]
   public bool RulerOpinionModifier { get; set; }

   [SaveAs]
   [ParseAs("can_generate_mercenary_leaders")]
   [DefaultValue(false)]
   [Description("If true, characters of this estate can become mercenary leaders.")]
   public bool CanGenerateMercenaryLeaders { get; set; }

   [SaveAs]
   [ParseAs("bank")]
   [DefaultValue(false)]
   [Description("If true, this estate can loan money to the ruler.")]
   public bool Bank { get; set; }

   [SaveAs]
   [ParseAs("ruler")]
   [DefaultValue(false)]
   [Description("If true, this estate can provide a ruler.")]
   public bool Ruler { get; set; }

   [SaveAs]
   [ParseAs("use_diminutive")]
   [DefaultValue(false)]
   [Description("If true, characters of this estate will use diminutives in their names.")]
   public bool UseDiminutive { get; set; }

   [SaveAs]
   [ParseAs("characters_have_dynasty")]
   [DefaultValue("")]
   [Description("If characters will spawn with a dynasty or not but some how weirdly defined.")]
   public string CharactersHaveDynasty { get; set; } = string.Empty;

   [SaveAs]
   [ParseAs("high_power", itemNodeType: AstNodeType.ContentNode)]
   [DefaultValue(null)]
   [Description("The modifiers applied when this Estate has high power.")]
   public ObservableRangeCollection<ModValInstance> HighPowerModifiers { get; set; } = [];

   [SaveAs]
   [ParseAs("low_power", itemNodeType: AstNodeType.ContentNode)]
   [DefaultValue(null)]
   [Description("The modifiers applied when this Estate has low power.")]
   public ObservableRangeCollection<ModValInstance> LowPowerModifiers { get; set; } = [];

   [SaveAs]
   [ParseAs("power", itemNodeType: AstNodeType.ContentNode)]
   [DefaultValue(null)]
   [Description("The modifiers scaled by the current power of this Estate.")]
   public ObservableRangeCollection<ModValInstance> PowerModifiers { get; set; } = [];
   [SaveAs]
   [ParseAs("satisfaction", itemNodeType: AstNodeType.ContentNode)]
   [DefaultValue(null)]
   [Description("The modifiers scaled by the current satisfaction of this Estate.")]
   public ObservableRangeCollection<ModValInstance> SatisfactionModifiers { get; set; } = [];

   #endregion

#pragma warning disable AGS004
   [Description("Unique key of this Estate. Must be unique among all objects of this type.")]
   [DefaultValue("null")]
   public string UniqueId { get; set; } = null!;

   [SuppressAgs]
   public Eu5FileObj Source { get; set; } = null!;
   public Eu5ObjectLocation FileLocation { get; set; } = Eu5ObjectLocation.Empty;
#pragma warning restore AGS004

   #region IEu5Object

   public string GetNamespace => $"Culture.{nameof(Estate)}";
   public void OnSearchSelected() => UIHandle.Instance.MainWindowsHandle.SetToNui(this);
   public ISearchResult VisualRepresentation => new SearchResultItem(null, UniqueId, GetNamespace.Replace('.', '>'));
   public Enum SearchCategory => IQueastorSearchSettings.DefaultCategories.GameObjects;
   public bool IsReadonly => true;
   public NUISetting NUISettings => Config.Settings.NUIObjectSettings.EstateSettings;
   public INUINavigation[] Navigations => [];
   public AgsSettings AgsSettings => Config.Settings.AgsSettings.EstateAgsSettings;
   public static Dictionary<string, Estate> GetGlobalItems() => Globals.Estates;

   public static Estate Empty { get; } = new() { UniqueId = "Arcanum_Empty_Estate" };

   public override string ToString() => UniqueId;

   #endregion
}