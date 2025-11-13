using System.ComponentModel;
using Arcanum.API.UtilServices.Search;
using Arcanum.Core.CoreSystems.NUI;
using Arcanum.Core.CoreSystems.NUI.Attributes;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.ToolBox;
using Arcanum.Core.CoreSystems.SavingSystem.AGS;
using Arcanum.Core.CoreSystems.SavingSystem.AGS.Attributes;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.CoreSystems.Selection;
using Arcanum.Core.GameObjects.BaseTypes;
using Arcanum.Core.GameObjects.BaseTypes.InjectReplace;
using Common.UI;
using ModValInstance = Arcanum.Core.CoreSystems.Jomini.Modifiers.ModValInstance;

namespace Arcanum.Core.GameObjects.AbstractMechanics;

[ObjectSaveAs]
public partial class Age : IEu5Object<Age>
{
#pragma warning disable AGS004
   [Description("Unique key for ages. Must be unique among all objects of this type.")]
   [DefaultValue("null")]
   public string UniqueId { get; set; } = null!;
#pragma warning restore AGS004

   # region Nexus Properties

   [SaveAs]
   [DefaultValue(0)]
   [ParseAs("victory_card")]
   [Description("The number of victory cards during this age.")]
   public int VictoryCardNumber { get; set; }

   [SaveAs]
   [DefaultValue(1)]
   [ParseAs("year")]
   [Description("What year the age starts in.")]
   public int Year { get; set; } = 1;

   [SaveAs]
   [DefaultValue(0)]
   [ParseAs("months_for_exploration_spread")]
   [Description("Months after which an exploration of an area spreads to every other country in the same subcontinent. When the exploration spreads one time in the original subcontinent it spreads again to adjacent subcontinents after that amount of time.")]
   public int MonthsForExplorationSpread { get; set; }

   //TODO: Not exactly true, just must be >0 as there is a division in code
   [PropertyConfig(minValue: 0.001)]
   [SaveAs]
   [DefaultValue(5f)]
   [ParseAs("max_price")]
   [Description("The maximum price modifier for goods during this age.")]
   public float MaxPrice { get; set; } = 5f;

   //TODO: Not exactly true, just must be >0 as there is a division in code
   [PropertyConfig(minValue: 0.001)]
   [SaveAs]
   [DefaultValue(0.1f)]
   [ParseAs("min_price")]
   [Description("The minimum price modifier for goods during this age.")]
   public float MinPrice { get; set; } = 0.1f;

   [PropertyConfig(maxValue: 1)]
   [SaveAs]
   [DefaultValue(1f)]
   [ParseAs("price_stability")]
   [Description("A modifier to price stability during this age. Maximum value is 1.")]
   public float PriceStability { get; set; } = 1f;

   [SaveAs]
   [DefaultValue(0f)]
   [ParseAs("mercenaries")]
   [Description("Additive modifier to amount of mercenary leaders generated in each age.")]
   public float Mercenaries { get; set; }

   [SaveAs]
   [DefaultValue(false)]
   [ParseAs("hegemons_allowed")]
   [Description("Whether hegemons are allowed to be formed during this age.")]
   public bool AllowHegemons { get; set; }

   [SaveAs]
   [DefaultValue(1f)]
   [ParseAs("efficiency")]
   [Description("Modifier to the global production efficiency in this age.")]
   public float Efficiency { get; set; } = 1f;

   [SaveAs]
   [DefaultValue(0f)]
   [ParseAs("war_score_from_battles")]
   [Description("Additive modifier to war score gained from battles during this age.")]
   public float WarScoreFromBattles { get; set; }

   [SaveAs]
   [DefaultValue(null)]
   [ParseAs("modifier", AstNodeType.BlockNode, itemNodeType: AstNodeType.ContentNode)]
   [Description("Country modifiers applied during this age.")]
   public ObservableRangeCollection<ModValInstance> Modifiers { get; set; } = [];

   [SaveAs]
   [DefaultValue(null)]
   [ParseAs("max_ai_privilege_per_estate", AstNodeType.BlockNode, itemNodeType: AstNodeType.ContentNode)]
   [Description("AI will not grant new privileges if the current amount is equal or above this.")]
   public ObservableRangeCollection<EstateCountDefinition> MaxAiPrivilegesPerEstate { get; set; } = [];

   [SaveAs]
   [DefaultValue(null)]
   [ParseAs("min_ai_privilege_per_estate", AstNodeType.BlockNode, itemNodeType: AstNodeType.ContentNode)]
   [Description("AI will not revoke privileges if the current amount is equal or below this.")]
   public ObservableRangeCollection<EstateCountDefinition> MinAiPrivilegesPerEstate { get; set; } = [];

   # endregion

   #region Interface Properties

   public bool IsReadonly => true;
   public NUISetting NUISettings => Config.Settings.NUIObjectSettings.AgeSettings;
   public INUINavigation[] Navigations { get; } = [];
   public static Age Empty { get; } = new() { UniqueId = "Arcanum_Empty_Age" };
   public static Dictionary<string, Age> GetGlobalItems() => Globals.Ages;

   #endregion

   #region ISearchable

   public string GetNamespace => $"AbstractMechanics.{nameof(Age)}";
   public Eu5ObjectLocation FileLocation { get; set; } = Eu5ObjectLocation.Empty;
   public string ResultName => UniqueId;
   public List<string> SearchTerms => [UniqueId];

   public void OnSearchSelected()
   {
      SelectionManager.Eu5ObjectSelectedInSearch(this);
   }

   public ISearchResult VisualRepresentation => new SearchResultItem(null, UniqueId, GetNamespace.Replace('.', '>'));
   public Enum SearchCategory => IQueastorSearchSettings.DefaultCategories.GameObjects;

   #endregion

   public Eu5FileObj Source { get; set; } = null!;

   public AgsSettings AgsSettings { get; } = Config.Settings.AgsSettings.AgeAgsSettings;
   public string SavingKey => UniqueId;

   public override string ToString() => UniqueId;

   protected bool Equals(Age other) => UniqueId == other.UniqueId;
   public InjRepType InjRepType { get; set; } = InjRepType.None;

   public override bool Equals(object? obj)
   {
      if (obj is null)
         return false;
      if (ReferenceEquals(this, obj))
         return true;
      if (obj.GetType() != GetType())
         return false;

      return Equals((Age)obj);
   }

   // ReSharper disable once NonReadonlyMemberInGetHashCode
   public override int GetHashCode() => UniqueId.GetHashCode();
}