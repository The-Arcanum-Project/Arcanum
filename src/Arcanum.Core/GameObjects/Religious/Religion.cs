using System.ComponentModel;
using System.Diagnostics;
using Arcanum.API.UtilServices.Search;
using Arcanum.Core.CoreSystems.Jomini.Date;
using Arcanum.Core.CoreSystems.Jomini.Modifiers;
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
using Arcanum.Core.GameObjects.Cultural;
using Arcanum.Core.GameObjects.Cultural.SubObjects;
using Arcanum.Core.GameObjects.LocationCollections;
using Arcanum.Core.GameObjects.Map;
using Arcanum.Core.GameObjects.Religious.SubObjects;
using Nexus.Core.Attributes;

namespace Arcanum.Core.GameObjects.Religious;

[NexusConfig]
[ObjectSaveAs]
[DebuggerDisplay("{UniqueId,nq}")]
public partial class Religion : IEu5Object<Religion>, IMapInferable
{
   #region Nexus Properties

   [ParseAs("color")]
   [DefaultValue(null)]
   [SaveAs]
   [Description("Color associated with this Religion.")]
   public JominiColor Color { get; set; } = JominiColor.Empty;

   [ParseAs("enable")]
   [DefaultValue(null)]
   [SaveAs]
   [Description("The date when this Religion becomes enabled.")]
   public JominiDate EnableDate { get; set; } = JominiDate.Empty;

   [ParseAs("group")]
   [DefaultValue(null)]
   [SaveAs(SavingValueType.Identifier)]
   [Description("The ReligionGroup this Religion belongs to.")]
   public ReligionGroup Group { get; set; } = ReligionGroup.Empty;

   [ParseAs("language")]
   [DefaultValue(null)]
   [SaveAs(SavingValueType.Identifier)]
   [Description("The language used in this Religion.")]
   public Language Language { get; set; } = Language.Empty;

   [ParseAs("important_country")]
   [DefaultValue(null)]
   [SaveAs(SavingValueType.Identifier)]
   [Description("The most important country for this Religion.")]
   public Country ImportantCountry { get; set; } = Country.Empty;

   [ParseAs("has_karma")]
   [DefaultValue(false)]
   [SaveAs]
   [Description("Indicates whether this Religion has a karma system.")]
   public bool HasKarma { get; set; }

   [ParseAs("has_purity")]
   [DefaultValue(false)]
   [SaveAs]
   [Description("Indicates whether this Religion has a purity system.")]
   public bool HasPurity { get; set; }

   [ParseAs("has_honor")]
   [DefaultValue(false)]
   [SaveAs]
   [Description("Indicates whether this Religion has an honor system.")]
   public bool HasHonor { get; set; }

   [ParseAs("has_canonization")]
   [DefaultValue(false)]
   [SaveAs]
   [Description("Indicates whether this Religion has a canonization system.")]
   public bool HasCanonization { get; set; }

   [ParseAs("needs_reform")]
   [DefaultValue(false)]
   [SaveAs]
   [Description("Indicates whether this Religion needs reform.")]
   public bool NeedsReform { get; set; }

   [ParseAs("has_religious_head")]
   [DefaultValue(false)]
   [SaveAs]
   [Description("Indicates whether this Religion has a religious head.")]
   public bool HasReligiousHead { get; set; }

   [ParseAs("has_cardinals")]
   [DefaultValue(false)]
   [SaveAs]
   [Description("Indicates whether this Religion has cardinals.")]
   public bool HasCardinals { get; set; }

   [ParseAs("has_doom")]
   [DefaultValue(false)]
   [SaveAs]
   [Description("Indicates whether this Religion has a doom system.")]
   public bool HasDoom { get; set; }

   [ParseAs("has_religious_influence")]
   [DefaultValue(false)]
   [SaveAs]
   [Description("Indicates whether this Religion has religious influence.")]
   public bool HasReligiousInfluence { get; set; }

   [ParseAs("ai_wants_convert")]
   [DefaultValue(false)]
   [SaveAs]
   [Description("Indicates whether the AI desires to convert to this Religion.")]
   public bool AiWantsConvert { get; set; }

   [ParseAs("has_autocephalous_patriarchates")]
   [DefaultValue(false)]
   [SaveAs]
   [Description("Indicates whether this Religion has autocephalous patriarchates.")]
   public bool HasAutocephalousPatriarchates { get; set; }

   [ParseAs("has_rite_power")]
   [DefaultValue(false)]
   [SaveAs]
   [Description("Indicates whether this Religion has rite power.")]
   public bool HasRitePower { get; set; }

   [ParseAs("use_icons")]
   [DefaultValue(false)]
   [SaveAs]
   [Description("Indicates whether this Religion uses icons.")]
   public bool UseIcons { get; set; }

   [ParseAs("has_avatars")]
   [DefaultValue(false)]
   [SaveAs]
   [Description("Indicates whether this Religion has avatars.")]
   public bool HasAvatars { get; set; }

   [ParseAs("has_patriarchs")]
   [DefaultValue(false)]
   [SaveAs]
   [Description("Indicates whether this Religion has patriarchs.")]
   public bool HasPatriarchs { get; set; }

   [ParseAs("has_yanantin")]
   [DefaultValue(false)]
   [SaveAs]
   [Description("Indicates whether this Religion has yanantin.")]
   public bool HasYanantin { get; set; }

   [ParseAs("culture_locked")]
   [DefaultValue(false)]
   [SaveAs]
   [Description("Indicates whether this Religion is locked to specific cultures.")]
   public bool CultureLocked { get; set; }

   [ParseAs("max_sects")]
   [DefaultValue(0)]
   [SaveAs]
   [Description("The maximum number of sects allowed for this Religion.")]
   public int MaxSects { get; set; }

   [ParseAs("num_religious_focuses_needed_for_reform")]
   [DefaultValue(0)]
   [SaveAs]
   [Description("The number of religious focuses needed for reforming this Religion.")]
   public int NumReligiousFocusesNeededForReform { get; set; }

   [ParseAs("religious_aspects")]
   [DefaultValue(0)]
   [SaveAs]
   [Description("The number of religious aspects associated with this Religion.")]
   public int ReligiousAspects { get; set; }

   [ParseAs("tithe")]
   [DefaultValue(0f)]
   [SaveAs]
   [Description("The tithe percentage for this Religion. (2% is 20% of the tenth)")]
   public float Tithe { get; set; }

   [SaveAs]
   [DefaultValue(null)]
   [ParseAs("definition_modifier", itemNodeType: AstNodeType.ContentNode)]
   [Description("Modifiers applied to this Religion.")]
   public ObservableRangeCollection<ModValInstance> Modifiers { get; set; } = [];

   [SaveAs]
   [DefaultValue(null)]
   [ParseAs("opinions", itemNodeType: AstNodeType.ContentNode)]
   [Description("Opinions towards other religions.")]
   public ObservableRangeCollection<ReligionOpinionValue> Opinions { get; set; } = [];

   [SaveAs(SavingValueType.Identifier)]
   [DefaultValue(null)]
   [ParseAs("tags", itemNodeType: AstNodeType.KeyOnlyNode)]
   [Description("Tags associated with this Religion.")]
   public ObservableRangeCollection<string> Tags { get; set; } = [];

   [SaveAs(SavingValueType.Identifier)]
   [DefaultValue(null)]
   [ParseAs("custom_tags", itemNodeType: AstNodeType.KeyOnlyNode)]
   [Description("Custom tags associated with this Religion.")]
   public ObservableRangeCollection<string> CustomTags { get; set; } = [];

   [SaveAs(SavingValueType.Identifier)]
   [DefaultValue(false)]
   [Description("A list of available names for this religion")]
   [ParseAs("unique_names", AstNodeType.BlockNode)]
   public ObservableRangeCollection<string> UniqueNames { get; set; } = [];

   [SaveAs]
   [DefaultValue(null)]
   [ParseAs("factions", itemNodeType: AstNodeType.KeyOnlyNode)]
   [Description("Factions associated with this Religion.")]
   public ObservableRangeCollection<ReligiousFaction> Factions { get; set; } = [];

   [SaveAs]
   [DefaultValue(null)]
   [ParseAs("religious_school", itemNodeType: AstNodeType.ContentNode, isShatteredList: true)]
   [Description("Religious schools associated with this Religion.")]
   public ObservableRangeCollection<ReligiousSchool> ReligiousSchools { get; set; } = [];

   [SaveAs]
   [DefaultValue(null)]
   [ParseAs("religious_focuses", itemNodeType: AstNodeType.KeyOnlyNode)]
   [Description("Religious focuses associated with this Religion.")]
   public ObservableRangeCollection<ReligiousFocus> ReligiousFocuses { get; set; } = [];

   #endregion

#pragma warning disable AGS004
   [Description("Unique key of this Religion. Must be unique among all objects of this type.")]
   [DefaultValue("null")]
   public string UniqueId { get; set; } = null!;

   [SuppressAgs]
   public Eu5FileObj Source { get; set; } = Eu5FileObj.Empty;
#pragma warning restore AGS004

   #region IEu5Object

   public string GetNamespace => $"{nameof(Religion)}";
   public void OnSearchSelected() => SelectionManager.Eu5ObjectSelectedInSearch(this);
   public ISearchResult VisualRepresentation => new SearchResultItem(null, UniqueId, GetNamespace.Replace('.', '>'));
   public Enum SearchCategory => IQueastorSearchSettings.DefaultCategories.GameObjects;
   public bool IsReadonly => true;
   public InjRepType InjRepType { get; set; } = InjRepType.None;
   public NUISetting NUISettings => Config.Settings.NUIObjectSettings.ReligionSettings;
   public INUINavigation[] Navigations => [];
   public AgsSettings AgsSettings => Config.Settings.AgsSettings.ReligionAgsSettings;
   public static Dictionary<string, Religion> GetGlobalItems() => Globals.Religions;
   public Eu5ObjectLocation FileLocation { get; set; } = Eu5ObjectLocation.Empty;

   public static Religion Empty { get; } = new() { UniqueId = "Arcanum_Empty_Religion" };

   #endregion

   #region IMapInferable

   public MapModeManager.MapModeType GetMapMode => MapModeManager.MapModeType.Religion;

   public List<IEu5Object> GetInferredList(IEnumerable<Location> sLocs)
   {
      HashSet<IEu5Object> items = [];
      foreach (var loc in sLocs)
      {
         if (loc.TemplateData == LocationTemplateData.Empty && loc.TemplateData.Religion != Empty)
            continue;

         items.Add(loc.TemplateData.Religion);
      }

      return items.ToList();
   }

   public List<Location> GetRelevantLocations(IEu5Object[] items)
   {
      Debug.Assert(items.All(x => x is Religion));
      var religions = items.Cast<Religion>().ToArray();

      List<Location> locations = [];

      foreach (var loc in Globals.Locations.Values)
         if (religions.Contains(loc.TemplateData.Religion) &&
             loc.TemplateData != LocationTemplateData.Empty &&
             loc.TemplateData.Religion != Empty)
            locations.Add(loc);

      return locations;
   }

   #endregion
}