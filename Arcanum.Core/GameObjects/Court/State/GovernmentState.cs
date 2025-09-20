using System.ComponentModel;
using Arcanum.API.UtilServices.Search;
using Arcanum.Core.CoreSystems.Jomini.Date;
using Arcanum.Core.CoreSystems.NUI;
using Arcanum.Core.CoreSystems.Parsing.ToolBox;
using Arcanum.Core.CoreSystems.SavingSystem.AGS;
using Arcanum.Core.CoreSystems.SavingSystem.AGS.Attributes;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.GameObjects.BaseTypes;
using Arcanum.Core.GlobalStates;
using Common.UI;
using Nexus.Core;

namespace Arcanum.Core.GameObjects.Court.State;

[ObjectSaveAs]
public partial class GovernmentState : IEu5Object<GovernmentState>
{
   [SaveAs]
   [DefaultValue(GovernmentType.Monarchy)]
   [Description("The type of government this state represents.")]
   [ParseAs("type")]
   public GovernmentType Type { get; set; } = GovernmentType.Monarchy;

   [SaveAs]
   [DefaultValue("")]
   [Description("The type of a regency this government has.")]
   [ParseAs("regency")]
   public string Regency { get; set; } = string.Empty;

   [SaveAs]
   [DefaultValue("")]
   [Description("The current regent of this government.")]
   [ParseAs("active_regent")]
   public string ActiveRegent { get; set; } = string.Empty;

   [SaveAs]
   [DefaultValue(null)]
   [Description("The date this government entered a regency.")]
   [ParseAs("start_regency_date")]
   public JominiDate StartRegencyDate { get; set; } = JominiDate.Empty;

   [SaveAs]
   [DefaultValue(null)]
   [Description("The date this government exited a regency.")]
   [ParseAs("end_regency_date")]
   public JominiDate EndRegencyDate { get; set; } = JominiDate.Empty;

   [SaveAs]
   [DefaultValue("")]
   [Description("The current ruler of this government state.")]
   [ParseAs("ruler")]
   public string Ruler { get; set; } = string.Empty;

   [SaveAs]
   [DefaultValue("")]
   [Description("The current consort of this government state.")]
   [ParseAs("consort")]
   public string Consort { get; set; } = string.Empty;

   [SaveAs]
   [DefaultValue("")]
   [Description("The current heir of this government.")]
   [ParseAs("heir")]
   public string Heir { get; set; } = string.Empty;

   [SaveAs]
   [DefaultValue("")]
   [Description("TODO???")]
   [ParseAs("designated_heir_reason")]
   public string DesignateHeirReason { get; set; } = string.Empty;

   [SaveAs]
   [DefaultValue("")]
   [Description("How the heir is selected in this government state.")]
   [ParseAs("heir_selection")]
   public string HeirSelection { get; set; } = string.Empty;

   [SaveAs]
   [DefaultValue(null)]
   [Description("All reforms that have been enacted in this government state.")]
   [ParseAs("reforms", AstNodeType.BlockNode)]
   public ObservableRangeCollection<string> Reforms { get; set; } = [];

   [SaveAs]
   [DefaultValue(null)]
   [Description("All laws that have been enacted in this government state.")]
   [ParseAs("laws", AstNodeType.BlockNode, itemNodeType: AstNodeType.ContentNode)]
   public ObservableRangeCollection<EnactedLaw> EnactedLaws { get; set; } = [];

   [SaveAs]
   [DefaultValue(null)]
   [Description("All rulers that have ruled in this government state.")]
   [ParseAs("ruler_term", isEmbedded: true, isShatteredList: true, itemNodeType: AstNodeType.BlockNode)]
   public ObservableRangeCollection<RulerTerm> RulerTerms { get; set; } = [];

   [SaveAs]
   [DefaultValue(null)]
   [Description("All regnal numbers that have been used in this government state.")]
   [ParseAs("regnal_number", AstNodeType.BlockNode, itemNodeType: AstNodeType.ContentNode)]
   public ObservableRangeCollection<RegnalNumber> RegnalNumbers { get; set; } = [];

   [SaveAs]
   [DefaultValue(null)]
   [Description("All estate privileges that are currently enacted.")]
   [ParseAs("privilege")]
   public ObservableRangeCollection<string> Privileges { get; set; } = [];

   #region IEu5Object Implementation

   public string GetNamespace => $"Court.{nameof(GovernmentState)}";
   public void OnSearchSelected() => UIHandle.Instance.PopUpHandle.OpenPropertyGridWindow(this);

   public ISearchResult VisualRepresentation => new SearchResultItem(null, SavingKey, string.Empty);
   public IQueastorSearchSettings.Category SearchCategory => IQueastorSearchSettings.Category.GameObjects;
   public bool IsReadonly => false;
   public NUISetting NUISettings => Config.Settings.NUIObjectSettings.GovernmentStateSettings;
   public INUINavigation[] Navigations { get; } = [];
   public AgsSettings AgsSettings => Config.Settings.AgsSettings.GovernmentStateAgsSettings;
   public FileObj Source { get; set; } = null!;
   public string SavingKey => "government";

   private string _privateKey = string.Empty;

   [SuppressAgs]
   [IgnoreModifiable]
   public string UniqueId
   {
      get => SavingKey;
      set => _privateKey = value;
   }
   public static IEnumerable<GovernmentState> GetGlobalItems() => [];

   public static GovernmentState Empty { get; } = new() { UniqueId = "Arcanum_Empty_GovernmentState" };

   #endregion
}