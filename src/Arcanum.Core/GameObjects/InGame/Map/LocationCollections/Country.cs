using System.ComponentModel;
using System.Diagnostics;
using Arcanum.API.UtilServices.Search;
using Arcanum.Core.CoreSystems.Jomini.AiTags;
using Arcanum.Core.CoreSystems.Jomini.CurrencyDatas;
using Arcanum.Core.CoreSystems.Jomini.Modifiers;
using Arcanum.Core.CoreSystems.NUI;
using Arcanum.Core.CoreSystems.NUI.Attributes;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.ToolBox;
using Arcanum.Core.CoreSystems.SavingSystem.AGS;
using Arcanum.Core.CoreSystems.SavingSystem.AGS.Attributes;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.CoreSystems.Selection;
using Arcanum.Core.GameObjects.BaseTypes;
using Arcanum.Core.GameObjects.BaseTypes.InjectReplace;
using Arcanum.Core.GameObjects.InGame.CountryLevel;
using Arcanum.Core.GameObjects.InGame.Court;
using Arcanum.Core.GameObjects.InGame.Court.State;
using Arcanum.Core.GameObjects.InGame.Cultural;
using Arcanum.Core.GameObjects.InGame.Map.LocationCollections.SubObjects;
using Nexus.Core.Attributes;
using ReligiousSchool = Arcanum.Core.GameObjects.InGame.Religious.ReligiousSchool;

namespace Arcanum.Core.GameObjects.InGame.Map.LocationCollections;

[NexusConfig]
[ObjectSaveAs]
public partial class Country : IEu5Object<Country>
{
   public Country(string uniqueId)
   {
      UniqueId = uniqueId;
   }

   public Country()
   {
      Capital = Location.Empty;
      GovernmentState = GovernmentState.Empty;
      Definition = CountryDefinition.Empty;
   }

   #region Nexus

   [SaveAs(savingMethod: "Setup_vars_saving")]
   [ParseAs("variables", customParser: "ArcParse_Variables")]
   [Description("A collection of variable declarations contained within this data container.")]
   [DefaultValue(null)]
   public ObservableRangeCollection<VariableDeclaration> Variables { get; set; } = [];
   [SuppressAgs]
   [Description("The unique tag for this country.")]
   public string UniqueId { get; set; } = null!;

   [SaveAs(SavingValueType.Identifier)]
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
   [ParseAs("religious_school")]
   [DefaultValue(null)]
   [Description("The religious school of this country.")]
   public ReligiousSchool ReligiousSchool { get; set; } = ReligiousSchool.Empty;

   [SaveAs(isShattered: true, valueType: SavingValueType.Identifier)]
   [DefaultValue(null)]
   [ParseAs("dynasty", isShatteredList: true)]
   [Description("The ruling dynasty of this country.")]
   public ObservableRangeCollection<Dynasty> Dynasty { get; set; } = [];

   [SaveAs]
   [ParseAs("court_language")]
   [DefaultValue(null)]
   [Description("The court language of this country.")]
   public Language CourtLanguage { get; set; } = Language.Empty;

   [SaveAs]
   [ParseAs("liturgical_language")]
   [DefaultValue(null)]
   [Description("The liturgical language of this country.")]
   public Language LiturgicalLanguage { get; set; } = Language.Empty;

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

   [SaveAs(SavingValueType.Identifier, saveEmbeddedAsIdentifier: false, mustNotBeWritten: "BuildingBasedCountryLimit")]
   [ParseAs("own_control_core", AstNodeType.BlockNode)]
   [DefaultValue(null)]
   [Description("The owned and controlled locations of this country.")]
   public ObservableRangeCollection<Location> OwnControlCores { get; set; } = [];

   [SaveAs(SavingValueType.Identifier, saveEmbeddedAsIdentifier: false, mustNotBeWritten: "BuildingBasedCountryLimit")]
   [ParseAs("own_control_integrated", AstNodeType.BlockNode)]
   [DefaultValue(null)]
   [Description("The owned and controlled locations that are integrated of this country.")]
   public ObservableRangeCollection<Location> OwnControlIntegrated { get; set; } = [];

   [SaveAs(SavingValueType.Identifier, saveEmbeddedAsIdentifier: false, mustNotBeWritten: "BuildingBasedCountryLimit")]
   [ParseAs("own_control_conquered", AstNodeType.BlockNode)]
   [DefaultValue(null)]
   [Description("All Locations conquered but controlled by someone else than this country.")]
   public ObservableRangeCollection<Location> OwnControlConquered { get; set; } = [];

   [SaveAs(SavingValueType.Identifier, saveEmbeddedAsIdentifier: false, mustNotBeWritten: "BuildingBasedCountryLimit")]
   [ParseAs("own_control_colony", AstNodeType.BlockNode)]
   [DefaultValue(null)]
   [Description("The owned colony locations of this country.")]
   public ObservableRangeCollection<Location> OwnControlColony { get; set; } = [];

   [SaveAs(SavingValueType.Identifier, saveEmbeddedAsIdentifier: false, mustNotBeWritten: "BuildingBasedCountryLimit")]
   [ParseAs("own_core", AstNodeType.BlockNode)]
   [DefaultValue(null)]
   [Description("The owned core locations of this country.")]
   public ObservableRangeCollection<Location> OwnCores { get; set; } = [];

   [SaveAs(SavingValueType.Identifier, saveEmbeddedAsIdentifier: false, mustNotBeWritten: "BuildingBasedCountryLimit")]
   [ParseAs("own_conquered", AstNodeType.BlockNode)]
   [DefaultValue(null)]
   [Description("All Locations conquered but owned by someone else than this country.")]
   public ObservableRangeCollection<Location> OwnConquered { get; set; } = [];

   [SaveAs(SavingValueType.Identifier, saveEmbeddedAsIdentifier: false, mustNotBeWritten: "BuildingBasedCountryLimit")]
   [ParseAs("own_integrated", AstNodeType.BlockNode)]
   [DefaultValue(null)]
   [Description("The owned and integrated locations of this country.")]
   public ObservableRangeCollection<Location> OwnIntegrated { get; set; } = [];

   [SaveAs(SavingValueType.Identifier, saveEmbeddedAsIdentifier: false, mustNotBeWritten: "BuildingBasedCountryLimit")]
   [ParseAs("own_colony", AstNodeType.BlockNode)]
   [DefaultValue(null)]
   [Description("The owned colony locations of this country.")]
   public ObservableRangeCollection<Location> OwnColony { get; set; } = [];

   [SaveAs(SavingValueType.Identifier, saveEmbeddedAsIdentifier: false, mustNotBeWritten: "BuildingBasedCountryLimit")]
   [ParseAs("control_core", AstNodeType.BlockNode)]
   [DefaultValue(null)]
   [Description("The controlled core locations of this country.")]
   public ObservableRangeCollection<Location> ControlCores { get; set; } = [];

   [SaveAs(SavingValueType.Identifier, saveEmbeddedAsIdentifier: false, mustNotBeWritten: "BuildingBasedCountryLimit")]
   [ParseAs("control", AstNodeType.BlockNode)]
   [DefaultValue(null)]
   [Description("The controlled locations of this country.")]
   public ObservableRangeCollection<Location> Control { get; set; } = [];

   [SaveAs(SavingValueType.Identifier, saveEmbeddedAsIdentifier: false, mustNotBeWritten: "BuildingBasedCountryLimit")]
   [ParseAs("our_cores_conquered_by_others", AstNodeType.BlockNode)]
   [DefaultValue(null)]
   [Description("All Locations that are our cores but conquered by other countries.")]
   public ObservableRangeCollection<Location> OurCoresConqueredByOthers { get; set; } = [];

   [SaveAs(isShattered: true)]
   [ParseAs("include", isShatteredList: true, itemNodeType: AstNodeType.ContentNode)]
   [DefaultValue(null)]
   [Description("A list of included ??? for this country.")]
   public ObservableRangeCollection<CountryTemplate> Includes { get; set; } = [];

   [SaveAs(SavingValueType.Identifier, saveEmbeddedAsIdentifier: false)]
   [ParseAs("accepted_cultures", AstNodeType.BlockNode)]
   [DefaultValue(null)]
   [Description("A list of accepted cultures for this country.")]
   public ObservableRangeCollection<Culture> AcceptedCultures { get; set; } = [];

   [SaveAs(SavingValueType.Identifier, saveEmbeddedAsIdentifier: false)]
   [ParseAs("tolerated_cultures", AstNodeType.BlockNode)]
   [DefaultValue(null)]
   [Description("A list of tolerated cultures for this country.")]
   public ObservableRangeCollection<Culture> ToleratedCultures { get; set; } = [];

   [SaveAs]
   [ParseAs("currency_data", AstNodeType.BlockNode, itemNodeType: AstNodeType.ContentNode)]
   [DefaultValue(null)]
   [Description("A list of currency data effects for this country before game start.")]
   public ObservableRangeCollection<CurrencyData> CurrencyData { get; set; } = [];

   [SaveAs(SavingValueType.Identifier, saveEmbeddedAsIdentifier: false, mustNotBeWritten: "IsPopsCountry")]
   [ParseAs("add_pops_from_locations", AstNodeType.BlockNode)]
   [DefaultValue(null)]
   [Description("When starting the game, add pops from these locations to this country.")]
   public ObservableRangeCollection<Location> AddedPopsFromLocations { get; set; } = [];

   [SaveAs(SavingValueType.Identifier, saveEmbeddedAsIdentifier: false)]
   [ParseAs("discovered_provinces", AstNodeType.BlockNode)]
   [DefaultValue(null)]
   [Description("These provinces discovered by this country at game start.")]
   public ObservableRangeCollection<Province> DiscoveredProvinces { get; set; } = [];

   [SaveAs(SavingValueType.Identifier, saveEmbeddedAsIdentifier: false)]
   [ParseAs("discovered_areas", AstNodeType.BlockNode)]
   [DefaultValue(null)]
   [Description("These areas discovered by this country at game start.")]
   public ObservableRangeCollection<Area> DiscoveredAreas { get; set; } = [];

   [SaveAs(SavingValueType.Identifier, saveEmbeddedAsIdentifier: false)]
   [ParseAs("discovered_regions", AstNodeType.BlockNode)]
   [DefaultValue(null)]
   [Description("These regions discovered by this country at game start.")]
   public ObservableRangeCollection<Region> DiscoveredRegions { get; set; } = [];

   [SaveAs]
   [ParseAs("ai_advance_preference_tags", AstNodeType.BlockNode, itemNodeType: AstNodeType.ContentNode)]
   [DefaultValue(null)]
   [Description("These tags are used by the AI to determine which advances to prefer.")]
   public ObservableRangeCollection<AiTag> AiAdvancePreferenceTags { get; set; } = [];

   [SaveAs(SavingValueType.IAgs, isEmbeddedObject: true, isShattered: true)]
   [ParseAs("timed_modifier",
            isEmbedded: true,
            isShatteredList: true,
            itemNodeType: AstNodeType.BlockNode)]
   [DefaultValue(null)]
   [Description("A modifier starting and ending at a given date.")]
   public ObservableRangeCollection<TimedModifier> TimedModifier { get; set; } = [];

   [SaveAs(SavingValueType.IAgs, isEmbeddedObject: true, saveEmbeddedAsIdentifier: false)]
   [ParseAs("government", AstNodeType.BlockNode, isEmbedded: true)]
   [DefaultValue(null)]
   [Description("The government state of this country.")]
   public GovernmentState GovernmentState { get; set; } = null!;

   [PropertyConfig(isInlined: true)]
   [SuppressAgs]
   [DefaultValue(null)]
   [Description("The country definition associated with this country.")]
   public CountryDefinition Definition { get; set; } = null!;

   #endregion

   public bool IsReadonly => true;
   public NUISetting NUISettings { get; } = Config.Settings.NUIObjectSettings.CountrySettings;
   public INUINavigation[] Navigations { get; } = [];
   public static Dictionary<string, Country> GetGlobalItems() => Globals.Countries;

   private static readonly Lazy<Country> EmptyInstance = new(() =>
   {
      var emptyChar = new Country("Arcanum_Empty_Country");
      emptyChar.Capital = Location.Empty;
      emptyChar.GovernmentState = GovernmentState.Empty;
      emptyChar.Definition = CountryDefinition.Empty;
      return emptyChar;
   });

   public static Country Empty => EmptyInstance.Value;
   public string GetNamespace => "Map.Country";
   public Eu5ObjectLocation FileLocation { get; set; } = Eu5ObjectLocation.Empty;
   public string ResultName => UniqueId;
   public List<string> SearchTerms => [UniqueId];

   public void OnSearchSelected() => SelectionManager.Eu5ObjectSelectedInSearch(this);

   public ISearchResult VisualRepresentation => new SearchResultItem(null, UniqueId, GetNamespace.Replace('.', '>'));
   public Enum SearchCategory => IQueastorSearchSettings.DefaultCategories.MapObjects |
                                 IQueastorSearchSettings.DefaultCategories.GameObjects;
   public InjRepType InjRepType { get; set; } = InjRepType.None;
   public AgsSettings AgsSettings { get; } = Config.Settings.AgsSettings.Country;
   public string SavingKey => UniqueId;
   public Eu5FileObj Source { get; set; } = Eu5FileObj.Empty;

   public static bool BuildingBasedCountryLimit(object obj)
   {
      if (obj is not Country country)
      {
         Debug.Fail("BuildingBasedCountryLimit called with non-Country object.");
         return false;
      }

      return country.CountryType is CountryType.Building or CountryType.Pop;
   }

   private static bool IsPopsCountry(object obj)
   {
      if (obj is not Country country)
      {
         Debug.Fail("IsPopsCountry called with non-Country object.");
         return false;
      }

      return country.CountryType != CountryType.Pop;
   }
}