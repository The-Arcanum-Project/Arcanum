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
using Arcanum.Core.CoreSystems.Selection;
using Arcanum.Core.GameObjects.BaseTypes;
using Arcanum.Core.GameObjects.BaseTypes.InjectReplace;
using Common.UI;

namespace Arcanum.Core.GameObjects.CountryLevel;

[ObjectSaveAs]
public partial class CountryRank : IEu5Object<CountryRank>
{
   #region Nexus Properties

   [SaveAs]
   [DefaultValue(false)]
   [ParseAs("victory_card")]
   [Description("The factor for victory cards during this age.")]
   public bool VictoryCardFactor { get; set; }

   [SaveAs]
   [ParseAs("color")]
   [DefaultValue(null)]
   [Description("The color associated with this CountryRank.")]
   public JominiColor Color { get; set; } = JominiColor.Empty;

   [SaveAs]
   [ParseAs("level")]
   [DefaultValue(1)]
   [Description("The level of this CountryRank. Higher levels typically indicate greater prestige or authority.")]
   public int Level { get; set; }

   [SaveAs]
   [ParseAs("ai_level")]
   [DefaultValue(1)]
   [Description("The AI level associated with this CountryRank, influencing how the AI utilizes this rank in gameplay.")]
   public int AiLevel { get; set; }

   [SaveAs]
   [ParseAs("character_ai_cooldown")]
   [DefaultValue(0)]
   [Description("The cooldown period for characters of this CountryRank when controlled by the AI.")]
   public int CharacterAiCooldown { get; set; }

   [SaveAs]
   [ParseAs("diplomacy_ai_cooldown")]
   [DefaultValue(0)]
   [Description("The cooldown period for diplomatic actions associated with this CountryRank when controlled by the AI.")]
   public int DiplomacyAiCooldown { get; set; }

   [SaveAs]
   [ParseAs("language_power_scale")]
   [DefaultValue(1f)]
   [Description("The scale at which language power is applied for this CountryRank.")]
   public float LanguagePowerScale { get; set; } = 1f;

   [SaveAs]
   [ParseAs("rank_modifier", itemNodeType: AstNodeType.ContentNode)]
   [DefaultValue(null)]
   [Description("A collection of modifiers that apply to this CountryRank, affecting various aspects of gameplay.")]
   public ObservableRangeCollection<ModValInstance> RankModifiers { get; set; } = [];

   #endregion

#pragma warning disable AGS004
   [Description("Unique key of this CountryRank. Must be unique among all objects of this type.")]
   [DefaultValue("null")]
   public string UniqueId { get; set; } = null!;

   [SuppressAgs]
   public Eu5FileObj Source { get; set; } = Eu5FileObj.Empty;
   public Eu5ObjectLocation FileLocation { get; set; } = Eu5ObjectLocation.Empty;
#pragma warning restore AGS004

   #region IEu5Object

   public string GetNamespace => $"Country.{nameof(CountryRank)}";
   public void OnSearchSelected() => SelectionManager.Eu5ObjectSelectedInSearch(this);
   public ISearchResult VisualRepresentation => new SearchResultItem(null, UniqueId, GetNamespace.Replace('.', '>'));
   public Enum SearchCategory => IQueastorSearchSettings.DefaultCategories.GameObjects;
   public bool IsReadonly => true;
   public NUISetting NUISettings => Config.Settings.NUIObjectSettings.CountryRankSettings;
   public INUINavigation[] Navigations => [];
   public AgsSettings AgsSettings => Config.Settings.AgsSettings.CountryRankAgsSettings;
   public static Dictionary<string, CountryRank> GetGlobalItems() => Globals.CountryRanks;

   public static CountryRank Empty { get; } = new() { UniqueId = "Arcanum_Empty_CountryRank" };
   public InjRepType InjRepType { get; set; } = InjRepType.None;

   public override string ToString() => UniqueId;

   #endregion
}