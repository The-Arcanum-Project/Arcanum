using System.ComponentModel;
using Arcanum.API.UtilServices.Search;
using Arcanum.Core.CoreSystems.Common;
using Arcanum.Core.CoreSystems.ErrorSystem.BaseErrorTypes;
using Arcanum.Core.CoreSystems.ErrorSystem.Diagnostics;
using Arcanum.Core.CoreSystems.Jomini.Modifiers;
using Arcanum.Core.CoreSystems.NUI;
using Arcanum.Core.CoreSystems.NUI.Attributes;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.ToolBox;
using Arcanum.Core.CoreSystems.Parsing.ParsingHelpers.ArcColor;
using Arcanum.Core.CoreSystems.SavingSystem.AGS;
using Arcanum.Core.CoreSystems.SavingSystem.AGS.Attributes;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.GameObjects.BaseTypes;
using Arcanum.Core.GameObjects.Culture;
using Common.UI;

namespace Arcanum.Core.GameObjects.Pops;

[ObjectSaveAs]
public partial class PopType : IEu5Object<PopType>
{
   #region Nexus Properties

   [SaveAs]
   [ParseAs("color")]
   [DefaultValue(null)]
   [Description("Color used to represent this PopType in various UI elements.")]
   public JominiColor Color { get; set; } = JominiColor.Empty;

   [SaveAs]
   [ParseAs("editor")]
   [DefaultValue(0f)]
   [Description(Globals.REPLACE_DESCRIPTION)]
   public float Editor { get; set; }

   [SaveAs]
   [ParseAs("pop_food_consumption")]
   [DefaultValue(1f)]
   [Description("Amount of food consumed by a single pop of this type per month.")]
   public float PopFoodConsumption { get; set; }

   [SaveAs]
   [ParseAs("assimilation_conversion_factor")]
   [DefaultValue(0f)]
   [Description("Factor affecting the rate of cultural assimilation for this pop type. Higher values lead to faster assimilation.")]
   public float AssimilationConversionFactor { get; set; }

   [SaveAs]
   [ParseAs("city_graphics")]
   [DefaultValue(0f)]
   [Description(Globals.REPLACE_DESCRIPTION)]
   public float CityGraphics { get; set; }

   [SaveAs]
   [ParseAs("promotion_factor")]
   [DefaultValue(0f)]
   [Description("Factor influencing the likelihood of this pop type being promoted to a higher tier.")]
   public float PromotionFactor { get; set; }

   [SaveAs]
   [ParseAs("migration_factor")]
   [DefaultValue(0f)]
   [Description("Factor influencing the likelihood of this pop type migrating to other regions.")]
   public float MigrationFactor { get; set; }

   [SaveAs]
   [ParseAs("grow")]
   [DefaultValue(false)]
   [Description("Indicates whether this pop type is capable of growth.")]
   public bool Grow { get; set; }

   [SaveAs]
   [ParseAs("upper")]
   [DefaultValue(false)]
   [Description("Indicates whether this pop type is considered an upper-class pop.")]
   public bool Upper { get; set; }

   [SaveAs]
   [ParseAs("has_cap")]
   [DefaultValue(false)]
   [Description("Indicates whether this pop type has a population cap.")]
   public bool HasCap { get; set; }

   [SaveAs]
   [ParseAs("tribal_rules")]
   [DefaultValue(false)]
   [Description("If true, this pop type follows tribal rules.")]
   public bool TribalRules { get; set; }

   [SaveAs]
   [ParseAs("counts_towards_market_language")]
   [DefaultValue(false)]
   [Description("If true, pops of this type count towards the market language requirements.")]
   public bool CountsTowardsMarketLanguage { get; set; }

   [SaveAs]
   [DefaultValue("")]
   [ParseAs("promote_to", isShatteredList: true, itemNodeType: AstNodeType.ContentNode)]
   [Description("The PopTypes that this pop type can promote to.")]
   public ObservableRangeCollection<PopType> Dynasty { get; set; } = [];

   [SaveAs]
   [ParseAs("literacy_impact", itemNodeType: AstNodeType.ContentNode)]
   [DefaultValue(null)]
   [Description("Modifiers affecting the literacy rate of this pop type.")]
   public ObservableRangeCollection<ModValInstance> LiteracyImpact { get; set; } = [];

   [SaveAs]
   [ParseAs("pop_percentage_impact", itemNodeType: AstNodeType.ContentNode)]
   [DefaultValue(null)]
   [Description(Globals.REPLACE_DESCRIPTION)]
   public ObservableRangeCollection<ModValInstance> PopPercentageImpact { get; set; } = [];

   [SaveAs]
   [ParseAs("-", itemNodeType: AstNodeType.BlockNode, iEu5KeyType: typeof(Estate))]
   [DefaultValue(null)]
   [Description("List of estate attribute definitions associated with this pop type.")]
   public ObservableRangeCollection<EstateAttributeDefinition> EstateAttributes { get; set; } = [];

   #endregion

#pragma warning disable AGS004
   [ReadonlyNexus]
   [Description("Unique key of this PopType. Must be unique among all objects of this type.")]
   [DefaultValue("null")]
   public string UniqueId { get; set; } = null!;

   [SuppressAgs]
   public Eu5FileObj Source { get; set; } = null!;
#pragma warning restore AGS004

   #region IEu5Object

   public string GetNamespace => $"Pops.{nameof(PopType)}";
   public void OnSearchSelected() => UIHandle.Instance.PopUpHandle.OpenPropertyGridWindow(this);
   public ISearchResult VisualRepresentation => new SearchResultItem(null, UniqueId, string.Empty);
   public Enum SearchCategory => IQueastorSearchSettings.DefaultCategories.GameObjects;
   public bool IsReadonly => true;
   public NUISetting NUISettings => Config.Settings.NUIObjectSettings.PopTypeSettings;
   public INUINavigation[] Navigations => [];
   public AgsSettings AgsSettings => Config.Settings.AgsSettings.PopTypeAgsSettings;
   public static Dictionary<string, PopType> GetGlobalItems() => Globals.PopTypes;

   public static PopType Empty { get; } = new() { UniqueId = "Arcanum_Empty_PopType" };

   public override string ToString() => UniqueId;

   #endregion
}