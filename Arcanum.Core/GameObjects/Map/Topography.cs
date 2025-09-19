using System.ComponentModel;
using Arcanum.API.UtilServices.Search;
using Arcanum.Core.CoreSystems.Jomini.AudioTags;
using Arcanum.Core.CoreSystems.Jomini.ModifierSystem;
using Arcanum.Core.CoreSystems.NUI;
using Arcanum.Core.CoreSystems.NUI.Attributes;
using Arcanum.Core.CoreSystems.Parsing.ParsingHelpers.ArcColor;
using Arcanum.Core.CoreSystems.Parsing.ToolBox;
using Arcanum.Core.CoreSystems.SavingSystem.AGS;
using Arcanum.Core.CoreSystems.SavingSystem.AGS.Attributes;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.GameObjects.BaseTypes;
using Arcanum.Core.GlobalStates;
using Common.UI;

namespace Arcanum.Core.GameObjects.Map;

[ObjectSaveAs]
public partial class Topography : IEu5Object<Topography>
{
#pragma warning disable AGS004
   [ReadonlyNexus]
   [Description("Unique key of this object. Must be unique among all objects of this type.")]
   [DefaultValue("null")]
   public string UniqueKey { get; set; } = null!;

   [SuppressAgs]
   public FileObj Source { get; set; } = null!;
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
   [ParseAs("location_modifier", AstNodeType.BlockNode)]
   [Description("The location modifier applied to provinces with this climate.")]
   public ObservableRangeCollection<ModValInstance> LocationModifiers { get; set; } = [];

   [SaveAs]
   [DefaultValue(null)]
   [ParseAs("audio_tags", AstNodeType.BlockNode)]
   [Description("The audio tags associated with this climate.")]
   public ObservableRangeCollection<AudioTag> AudioTags { get; set; } = [];

   # endregion

   #region Interface Properties

   public bool IsReadonly => false;
   public NUISetting NUISettings => Config.Settings.NUIObjectSettings.TopographySettings;
   public INUINavigation[] Navigations { get; } = [];
   public static Topography Empty { get; } = new() { UniqueKey = "Arcanum_Empty_Topography" };
   public static IEnumerable<Topography> GetGlobalItems() => Globals.Topography.Values;

   #endregion

   #region ISearchable

   public string GetNamespace => $"Map.{nameof(Topography)}";
   public string ResultName => UniqueKey;
   public List<string> SearchTerms => [UniqueKey];

   public void OnSearchSelected()
   {
      UIHandle.Instance.PopUpHandle.OpenPropertyGridWindow(this);
   }

   public ISearchResult VisualRepresentation => new SearchResultItem(null, UniqueKey, string.Empty);
   public IQueastorSearchSettings.Category SearchCategory
      => IQueastorSearchSettings.Category.MapObjects | IQueastorSearchSettings.Category.GameObjects;

   #endregion

   public AgsSettings AgsSettings { get; } = Config.Settings.AgsSettings.TopographyAgsSettings;
   public string SavingKey => UniqueKey;

   public override string ToString() => UniqueKey;
}