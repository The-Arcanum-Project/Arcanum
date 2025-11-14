using System.ComponentModel;
using Arcanum.API.UtilServices.Search;
using Arcanum.Core.CoreSystems.Jomini.AiTags;
using Arcanum.Core.CoreSystems.Jomini.CurrencyDatas;
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
using Arcanum.Core.GameObjects.CountryLevel;
using Arcanum.Core.GameObjects.Court.State;
using Common.UI;
using Nexus.Core.Attributes;
using ReligiousSchool = Arcanum.Core.GameObjects.Religious.ReligiousSchool;

namespace Arcanum.Core.GameObjects.LocationCollections;

[NexusConfig]
[ObjectSaveAs]
public partial class Country : IEu5Object<Country>
{
   #region Nexus

   [SuppressAgs]
   [Description("The unique tag for this country.")]
   public string UniqueId { get; set; } = null!;

   [SaveAs]
   [ParseAs("capital")]
   [DefaultValue(null)]
   [Description("The capital location of this country.")]
   public Location Capital { get; set; } = Location.Empty;

   [SaveAs]
   [ParseAs("revolt")]
   [DefaultValue(false)]
   [Description("If this country is a rebel faction.")]
   public bool Revolt { get; set; }

   [SaveAs]
   [ParseAs("is_valid_for_release")]
   [DefaultValue(false)]
   [Description("If this country is valid for release by the player.")]
   public bool IsValidForRelease { get; set; }

   [SaveAs]
   [ParseAs("type")]
   [DefaultValue(CountryType.Location)]
   [Description("The type of this country.\nValid types: Location, Army, Pop, Building")]
   public CountryType CountryType { get; set; } = CountryType.Location;

   [SaveAs]
   [ParseAs("color")]
   [DefaultValue(null)]
   [Description("The color key of this country")]
   public JominiColor Color { get; set; } = JominiColor.Empty;

   [SaveAs]
   [ParseAs("religious_school")]
   [DefaultValue(null)]
   [Description("The religious school of this country.")]
   public ReligiousSchool ReligiousSchool { get; set; } = ReligiousSchool.Empty;

   [SaveAs]
   [DefaultValue("")]
   [ParseAs("dynasty", isShatteredList: true)]
   [Description("The ruling dynasty of this country.")]
   public ObservableRangeCollection<string> Dynasty { get; set; } = [];

   [SaveAs]
   [ParseAs("court_language")]
   [DefaultValue("")]
   [Description("The court language of this country.")]
   public string CourtLanguage { get; set; } = string.Empty;

   [SaveAs]
   [ParseAs("liturgical_language")]
   [DefaultValue("")]
   [Description("The liturgical language of this country.")]
   public string LiturgicalLanguage { get; set; } = string.Empty;

   [SaveAs]
   [ParseAs("country_rank")]
   [DefaultValue(null)]
   [Description("The rank of this country.")]
   public CountryRank CountryRank { get; set; } =
      Globals.CountryRanks.Values.FirstOrDefault(x => x.Level == 1) ?? CountryRank.Empty;

   [SaveAs]
   [ParseAs("starting_technology_level")]
   [DefaultValue(0)]
   [Description("The technology level this country starts with.")]
   public int StartingTechLevel { get; set; }

   [SaveAs]
   [ParseAs("flag")]
   [DefaultValue("")]
   [Description("The flag of this country.")]
   public string Flag { get; set; } = string.Empty;

   [SaveAs]
   [ParseAs("country_name")]
   [DefaultValue("")]
   [Description("The key for the name of this country.")]
   public string CountryName { get; set; } = string.Empty;

   [SaveAs]
   [ParseAs("own_control_core", AstNodeType.BlockNode)]
   [DefaultValue(null)]
   [Description("The owned and controlled locations of this country.")]
   public ObservableRangeCollection<Location> OwnControlCores { get; set; } = [];

   [SaveAs]
   [ParseAs("own_control_integrated", AstNodeType.BlockNode)]
   [DefaultValue(null)]
   [Description("The owned and controlled locations that are integrated of this country.")]
   public ObservableRangeCollection<Location> OwnControlIntegrated { get; set; } = [];

   [SaveAs]
   [ParseAs("own_control_conquered", AstNodeType.BlockNode)]
   [DefaultValue(null)]
   [Description("All Locations conquered but controlled by someone else than this country.")]
   public ObservableRangeCollection<Location> OwnControlConquered { get; set; } = [];

   [SaveAs]
   [ParseAs("own_control_colony", AstNodeType.BlockNode)]
   [DefaultValue(null)]
   [Description("The owned colony locations of this country.")]
   public ObservableRangeCollection<Location> OwnControlColony { get; set; } = [];

   [SaveAs]
   [ParseAs("own_core", AstNodeType.BlockNode)]
   [DefaultValue(null)]
   [Description("The owned core locations of this country.")]
   public ObservableRangeCollection<Location> OwnCores { get; set; } = [];

   [SaveAs]
   [ParseAs("own_conquered", AstNodeType.BlockNode)]
   [DefaultValue(null)]
   [Description("All Locations conquered but owned by someone else than this country.")]
   public ObservableRangeCollection<Location> OwnConquered { get; set; } = [];

   [SaveAs]
   [ParseAs("own_integrated", AstNodeType.BlockNode)]
   [DefaultValue(null)]
   [Description("The owned and integrated locations of this country.")]
   public ObservableRangeCollection<Location> OwnIntegrated { get; set; } = [];

   [SaveAs]
   [ParseAs("own_colony", AstNodeType.BlockNode)]
   [DefaultValue(null)]
   [Description("The owned colony locations of this country.")]
   public ObservableRangeCollection<Location> OwnColony { get; set; } = [];

   [SaveAs]
   [ParseAs("control_core", AstNodeType.BlockNode)]
   [DefaultValue(null)]
   [Description("The controlled core locations of this country.")]
   public ObservableRangeCollection<Location> ControlCores { get; set; } = [];

   [SaveAs]
   [ParseAs("control", AstNodeType.BlockNode)]
   [DefaultValue(null)]
   [Description("The controlled locations of this country.")]
   public ObservableRangeCollection<Location> Control { get; set; } = [];

   [SaveAs]
   [ParseAs("our_cores_conquered_by_others", AstNodeType.BlockNode)]
   [DefaultValue(null)]
   [Description("All Locations that are our cores but conquered by other countries.")]
   public ObservableRangeCollection<Location> OurCoresConqueredByOthers { get; set; } = [];

   [SaveAs]
   [ParseAs("include", isShatteredList: true)]
   [DefaultValue(null)]
   [Description("A list of included ??? for this country.")]
   public ObservableRangeCollection<string> Includes { get; set; } = [];

   [SaveAs]
   [ParseAs("accepted_cultures", AstNodeType.BlockNode)]
   [DefaultValue(null)]
   [Description("A list of accepted cultures for this country.")]
   public ObservableRangeCollection<string> AcceptedCultures { get; set; } = [];

   [SaveAs]
   [ParseAs("tolerated_cultures", AstNodeType.BlockNode)]
   [DefaultValue(null)]
   [Description("A list of tolerated cultures for this country.")]
   public ObservableRangeCollection<string> ToleratedCultures { get; set; } = [];

   [SaveAs]
   [ParseAs("currency_data", AstNodeType.BlockNode, itemNodeType: AstNodeType.ContentNode)]
   [DefaultValue(null)]
   [Description("A list of currency data effects for this country before game start.")]
   public ObservableRangeCollection<CurrencyData> CurrencyData { get; set; } = [];

   [SaveAs]
   [ParseAs("add_pops_from_locations", AstNodeType.BlockNode)]
   [DefaultValue(null)]
   [Description("When starting the game, add pops from these locations to this country.")]
   public ObservableRangeCollection<Location> AddedPopsFromLocations { get; set; } = [];

   [SaveAs]
   [ParseAs("discovered_provinces", AstNodeType.BlockNode)]
   [DefaultValue(null)]
   [Description("These provinces discovered by this country at game start.")]
   public ObservableRangeCollection<Province> DiscoveredProvinces { get; set; } = [];

   [SaveAs]
   [ParseAs("discovered_areas", AstNodeType.BlockNode)]
   [DefaultValue(null)]
   [Description("These areas discovered by this country at game start.")]
   public ObservableRangeCollection<Area> DiscoveredAreas { get; set; } = [];

   [SaveAs]
   [ParseAs("discovered_regions", AstNodeType.BlockNode)]
   [DefaultValue(null)]
   [Description("These regions discovered by this country at game start.")]
   public ObservableRangeCollection<Region> DiscoveredRegions { get; set; } = [];

   [SaveAs]
   [ParseAs("ai_advance_preference_tags", AstNodeType.BlockNode, itemNodeType: AstNodeType.ContentNode)]
   [DefaultValue(null)]
   [Description("These tags are used by the AI to determine which advances to prefer.")]
   public ObservableRangeCollection<AiTag> AiAdvancePreferenceTags { get; set; } = [];

   [SaveAs]
   [ParseAs("timed_modifier",
              AstNodeType.BlockNode,
              isEmbedded: true,
              isShatteredList: true,
              itemNodeType: AstNodeType.BlockNode)]
   [DefaultValue(null)]
   [Description("A modifier starting and ending at a given date.")]
   public ObservableRangeCollection<TimedModifier> TimedModifier { get; set; } = [];

   [SaveAs]
   [ParseAs("government", AstNodeType.BlockNode, isEmbedded: true)]
   [DefaultValue(null)]
   [Description("The government state of this country.")]
   public GovernmentState GovernmentState { get; set; } = GovernmentState.Empty;

   #endregion

   public bool IsReadonly => true;
   public NUISetting NUISettings { get; } = Config.Settings.NUIObjectSettings.CountrySettings;
   public INUINavigation[] Navigations { get; } = [];
   public static Dictionary<string, Country> GetGlobalItems() => Globals.Countries;

   public static Country Empty { get; } = new() { UniqueId = "Arcanum_EMPTY_Country" };
   public string GetNamespace => "Map.Country";
   public Eu5ObjectLocation FileLocation { get; set; } = Eu5ObjectLocation.Empty;
   public string ResultName => UniqueId;
   public List<string> SearchTerms => [UniqueId];

   public void OnSearchSelected()
   {
      SelectionManager.Eu5ObjectSelectedInSearch(this);
   }

   public ISearchResult VisualRepresentation => new SearchResultItem(null, UniqueId, GetNamespace.Replace('.', '>'));
   public Enum SearchCategory => IQueastorSearchSettings.DefaultCategories.MapObjects |
                                 IQueastorSearchSettings.DefaultCategories.GameObjects;
   public InjRepType InjRepType { get; set; } = InjRepType.None;
   public AgsSettings AgsSettings { get; } = Config.Settings.AgsSettings.CountryAgsSettings;
   public string SavingKey => UniqueId;
   public Eu5FileObj Source { get; set; } = Eu5FileObj.Empty;
}