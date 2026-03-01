using System.ComponentModel;
using Arcanum.API.UtilServices.Search;
using Arcanum.Core.CoreSystems.Jomini.Date;
using Arcanum.Core.CoreSystems.NUI;
using Arcanum.Core.CoreSystems.NUI.Attributes;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.ToolBox;
using Arcanum.Core.CoreSystems.SavingSystem.AGS;
using Arcanum.Core.CoreSystems.SavingSystem.AGS.Attributes;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.CoreSystems.Selection;
using Arcanum.Core.GameObjects.BaseTypes;
using Arcanum.Core.GameObjects.BaseTypes.InjectReplace;
using Arcanum.Core.GameObjects.InGame.Court.State.SubClasses;
using Arcanum.Core.GameObjects.InGame.Map.LocationCollections;
using Nexus.Core.Attributes;
using Estate = Arcanum.Core.GameObjects.InGame.Cultural.Estate;

namespace Arcanum.Core.GameObjects.InGame.Court.State;

[NexusConfig]
[ObjectSaveAs]
public partial class GovernmentState : IEu5Object<GovernmentState>
{
   public GovernmentState() : this(isEmpty: false)
   {
   }

   private GovernmentState(bool isEmpty)
   {
      if (isEmpty)
      {
         UniqueId = "Arcanum_Empty_GovernmentState";
         InheritRulerTerms = null!;
      }
      else
      {
         InheritRulerTerms = Country.Empty;
         Ruler = Character.Empty;
      }
   }

   [SaveAs(savingMethod: "SocientalValueEntrySaving")]
   [DefaultValue(null)]
   [Description("The societal values upheld by this government.")]
   [ParseAs(Globals.DO_NOT_PARSE_ME,
            isShatteredList: true,
            iEu5KeyType: typeof(SocientalValue),
            itemNodeType: AstNodeType.ContentNode)]
   public ObservableRangeCollection<SocientalValueEntry> SocietalValues { get; set; } = [];

   [SaveAs(alwaysWrite: true)]
   [DefaultValue(GovernmentType.None)]
   [Description("The type of government this state represents.")]
   [ParseAs("type")]
   public GovernmentType Type { get; set; } = GovernmentType.None;

   [SaveAs]
   [DefaultValue("")]
   [Description("The type of a regency this government has.")]
   [ParseAs("regency")]
   public string Regency { get; set; } = string.Empty;

   [SaveAs]
   [DefaultValue("")]
   [Description("The type of a regency this government has.")]
   [ParseAs("inherit_ruler_terms")]
   public Country InheritRulerTerms
   {
      get => field ?? Country.Empty;
      set;
   }

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

   [SaveAs(SavingValueType.Identifier)]
   [DefaultValue(null)]
   [Description("The current ruler of this government state.")]
   [ParseAs("ruler")]
   public Character Ruler { get; set; } = null!;

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

   [SaveAs(SavingValueType.Identifier)]
   [DefaultValue(null)]
   [Description(Globals.REPLACE_DESCRIPTION)]
   [ParseAs("designated_heir_reason")]
   public DesignateHeirReason DesignateHeirReason { get; set; } = DesignateHeirReason.Empty;

   [SaveAs(SavingValueType.Identifier)]
   [DefaultValue("")]
   [Description("How the heir is selected in this government state.")]
   [ParseAs("heir_selection")]
   public string HeirSelection { get; set; } = string.Empty;

   [SaveAs(SavingValueType.IAgs, isEmbeddedObject: true, saveEmbeddedAsIdentifier: false)]
   [DefaultValue(null)]
   [Description("Properties defining the parliament of this government")]
   [ParseAs("parliament", AstNodeType.BlockNode, isEmbedded: true)]
   public ParliamentDefinition ParliamentDefinition { get; set; } = ParliamentDefinition.Empty;

   [SaveAs(SavingValueType.Identifier)]
   [DefaultValue(null)]
   [Description("All reforms that have been enacted in this government state.")]
   [ParseAs("reforms", AstNodeType.BlockNode)]
   public ObservableRangeCollection<string> Reforms { get; set; } = [];

   [SaveAs]
   [DefaultValue(null)]
   [Description("All laws that have been enacted in this government state.")]
   [ParseAs("laws", AstNodeType.BlockNode, itemNodeType: AstNodeType.ContentNode)]
   public ObservableRangeCollection<EnactedLaw> EnactedLaws { get; set; } = [];

   [SaveAs(isShattered: true, isEmbeddedObject: true, saveEmbeddedAsIdentifier: false)]
   [DefaultValue(null)]
   [Description("All rulers that have ruled in this government state.")]
   [ParseAs("ruler_term", isEmbedded: true, isShatteredList: true, itemNodeType: AstNodeType.BlockNode)]
   public ObservableRangeCollection<RulerTerm> RulerTerms { get; set; } = [];

   [SaveAs]
   [DefaultValue(null)]
   [Description("All regnal numbers that have been used in this government state.")]
   [ParseAs("regnal_numbers", AstNodeType.BlockNode, itemNodeType: AstNodeType.ContentNode)]
   public ObservableRangeCollection<RegnalNumber> RegnalNumbers { get; set; } = [];

   [SaveAs(SavingValueType.Identifier)]
   [DefaultValue(null)]
   [Description("All estate privileges that are currently enacted.")]
   [ParseAs("privilege", AstNodeType.BlockNode)]
   public ObservableRangeCollection<string> Privileges { get; set; } = [];

   [SaveAs(SavingValueType.IAgs, isShattered: true, isEmbeddedObject: true, saveEmbeddedAsIdentifier: false)]
   [ParseAs("-", itemNodeType: AstNodeType.BlockNode, iEu5KeyType: typeof(Estate))]
   [DefaultValue(null)]
   [Description("List of estate attribute definitions associated with this pop type.")]
   public ObservableRangeCollection<EstateSatisfactionDefinition> EstateAttributes { get; set; } = [];

   #region IEu5Object Implementation

   public string GetNamespace => $"Court.{nameof(GovernmentState)}";
   public void OnSearchSelected() => SelectionManager.Eu5ObjectSelectedInSearch(this);

   public ISearchResult VisualRepresentation => new SearchResultItem(null, SavingKey, string.Empty);
   public Enum SearchCategory => IQueastorSearchSettings.DefaultCategories.GameObjects;
   public bool IsReadonly => false;
   public NUISetting NUISettings => Config.Settings.NUIObjectSettings.GovernmentStateSettings;
   public INUINavigation[] Navigations { get; } = [];
   public AgsSettings AgsSettings => Config.Settings.AgsSettings.GovernmentState;
   public Eu5FileObj Source { get; set; } = Eu5FileObj.Empty;
   public Eu5ObjectLocation FileLocation { get; set; } = Eu5ObjectLocation.Empty;
   public string SavingKey => "government";
   public InjRepType InjRepType { get; set; } = InjRepType.None;

   [SuppressAgs]
   [PropertyConfig(isReadonly: true)]
   public string UniqueId
   {
      get => SavingKey;
      set { }
   }
   public static Dictionary<string, GovernmentState> GetGlobalItems() => [];

   public static GovernmentState Empty { get; } = new(true);

   #endregion
}