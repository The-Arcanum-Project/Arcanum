using System.ComponentModel;
using System.Diagnostics;
using Arcanum.API.UtilServices.Search;
using Arcanum.Core.CoreSystems.Jomini.AudioTags;
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
using Arcanum.Core.GameObjects.InGame.Map.LocationCollections;
using Nexus.Core.Attributes;
using ModValInstance = Arcanum.Core.CoreSystems.Jomini.Modifiers.ModValInstance;

namespace Arcanum.Core.GameObjects.InGame.Map;

[NexusConfig]
[ObjectSaveAs]
public partial class Topography : IEu5Object<Topography>, IMapInferable
{
#pragma warning disable AGS004
   [Description("Unique key of this object. Must be unique among all objects of this type.")]
   [DefaultValue("null")]
   public string UniqueId { get; set; } = null!;

   [SuppressAgs]
   public Eu5FileObj Source { get; set; } = Eu5FileObj.Empty;
   public Eu5ObjectLocation FileLocation { get; set; } = Eu5ObjectLocation.Empty;
#pragma warning restore AGS004

   # region Nexus Properties

   [SaveAs]
   [DefaultValue(null)]
   [ParseAs("color")]
   [Description("The color used to represent this topography on the map.")]
   public JominiColor Color { get; set; } = JominiColor.Empty;

   [SaveAs]
   [DefaultValue(null)]
   [ParseAs("debug_color")]
   [Description("The color used for topology_screenshot mapmode")]
   public JominiColor DebugColor { get; set; } = JominiColor.Empty;

   [SaveAs]
   [DefaultValue(1f)]
   [ParseAs("movement_cost")]
   [Description("The movement cost multiplier for units moving through this topography.")]
   public float MovementCost { get; set; } = 1f;

   [SaveAs]
   [DefaultValue(1f)]
   [ParseAs("vegetation_density")]
   [Description("The density of vegetation in this topography, effects unknown.")]
   public float VegetationDensity { get; set; } = 1f;

   [SaveAs]
   [DefaultValue(0f)]
   [ParseAs("weather_front_strength_change_percent")]
   [Description("How much weather fronts are strengthened or weakened when passing through this topography, as a percentage." +
                "\n-0.08 #every FRONT_DEGRADATION_DISTANCE_FOR_TOPOGRAPHY pixels moved")]
   public float WeatherFrontStrengthChangePercent { get; set; }

   [SaveAs]
   [DefaultValue(0f)]
   [ParseAs("weather_cyclone_strength_change_percent")]
   [Description("How much cyclones are strengthened or weakened when passing through this topography, as a percentage.")]
   public float WeatherCycloneStrengthChangePercent { get; set; }

   [SaveAs]
   [DefaultValue(0f)]
   [ParseAs("weather_tornado_strength_change_percent")]
   [Description("How much tornadoes are strengthened or weakened when passing through this topography, as a percentage.")]
   public float WeatherTornadoStrengthChangePercent { get; set; }

   [SaveAs]
   [DefaultValue(0)]
   [ParseAs("defender")]
   [Description("The defender bonus provided by this topography.")]
   public int DefenderDice { get; set; }

   [SaveAs]
   [DefaultValue(false)]
   [ParseAs("blocked_in_winter")]
   [Description("Whether this topography is impassable during winter.")]
   public bool BlockedInWinter { get; set; }

   [SaveAs]
   [DefaultValue(false)]
   [ParseAs("can_have_ice")]
   [Description("Whether ice can form on this topography.")]
   public bool CanHaveIce { get; set; }

   [SaveAs]
   [DefaultValue(false)]
   [ParseAs("can_freeze_over")]
   [Description("Whether this topography can freeze over completely.")]
   public bool CanFreezeOver { get; set; }

   [SaveAs]
   [DefaultValue(false)]
   [ParseAs("has_sand")]
   [Description("Whether this topography has sand")]
   public bool HasSand { get; set; }

   [SaveAs]
   [DefaultValue(false)]
   [ParseAs("is_deep_ocean")]
   [Description("Whether this topography is deep ocean.")]
   public bool IsDeepOcean { get; set; }

   [SaveAs]
   [DefaultValue(false)]
   [ParseAs("is_lake")]
   [Description("Whether this topography is a lake.")]
   public bool IsLake { get; set; }

   [SaveAs]
   [DefaultValue(null)]
   [ParseAs("location_modifier", AstNodeType.BlockNode, itemNodeType: AstNodeType.ContentNode)]
   [Description("The location modifier applied to provinces with this climate.")]
   public ObservableRangeCollection<ModValInstance> LocationModifiers { get; set; } = [];

   [SaveAs]
   [DefaultValue(null)]
   [ParseAs("audio_tags", AstNodeType.BlockNode, itemNodeType: AstNodeType.ContentNode)]
   [Description("The audio tags associated with this climate.")]
   public ObservableRangeCollection<AudioTag> AudioTags { get; set; } = [];

   # endregion

   #region Interface Properties

   public InjRepType InjRepType { get; set; } = InjRepType.None;
   public bool IsReadonly => false;
   public NUISetting NUISettings => Config.Settings.NUIObjectSettings.TopographySettings;
   public INUINavigation[] Navigations { get; } = [];
   public static Topography Empty { get; } = new() { UniqueId = "Arcanum_Empty_Topography" };
   public static Dictionary<string, Topography> GetGlobalItems() => Globals.Topography;

   #endregion

   #region ISearchable

   public string GetNamespace => $"Map.{nameof(Topography)}";
   public string ResultName => UniqueId;
   public List<string> SearchTerms => [UniqueId];

   public void OnSearchSelected()
   {
      SelectionManager.Eu5ObjectSelectedInSearch(this);
   }

   public ISearchResult VisualRepresentation => new SearchResultItem(null, UniqueId, GetNamespace.Replace('.', '>'));
   public Enum SearchCategory => IQueastorSearchSettings.DefaultCategories.MapObjects |
                                 IQueastorSearchSettings.DefaultCategories.GameObjects;

   #endregion

   public AgsSettings AgsSettings { get; } = Config.Settings.AgsSettings.TopographyAgsSettings;
   public string SavingKey => UniqueId;

   #region IMapInferable

   public MapModeManager.MapModeType GetMapMode => MapModeManager.MapModeType.Topography;

   public List<IEu5Object> GetInferredList(IEnumerable<Location> sLocs)
   {
      HashSet<IEu5Object> items = [];
      foreach (var loc in sLocs)
      {
         if (loc.TemplateData == LocationTemplateData.Empty && loc.TemplateData.Topography != Empty)
            continue;

         items.Add(loc.TemplateData.Topography);
      }

      return items.ToList();
   }

   public List<Location> GetRelevantLocations(IEu5Object[] items)
   {
      Debug.Assert(items.All(x => x is Topography));
      var objs = items.Cast<Topography>().ToArray();

      List<Location> locations = [];

      foreach (var loc in Globals.Locations.Values)
         if (objs.Contains(loc.TemplateData.Topography) &&
             loc.TemplateData != LocationTemplateData.Empty &&
             loc.TemplateData.Topography != Empty)
            locations.Add(loc);

      return locations;
   }

   #endregion
}