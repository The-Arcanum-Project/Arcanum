using System.ComponentModel;
using Arcanum.API.UtilServices.Search;
using Arcanum.Core.CoreSystems.NUI;
using Arcanum.Core.CoreSystems.Parsing.ParsingHelpers.ArcColor;
using Arcanum.Core.CoreSystems.Parsing.ToolBox;
using Arcanum.Core.GameObjects.BaseTypes;
using Arcanum.Core.GlobalStates;

namespace Arcanum.Core.GameObjects.Map;

public partial
   class Topography(string name) : OldNameKeyDefined(name), INUI, IEmpty<Topography>, ICollectionProvider<Topography>
{
   # region Nexus Properties

   [ParseAs("color", AstNodeType.ContentNode)]
   [Description("The color used to represent this topography on the map.")]
   public JominiColor Color { get; set; } = JominiColor.Empty;
   [ParseAs("debug_color", AstNodeType.ContentNode)]
   [Description("The color used for topology_screenshot mapmode")]
   public JominiColor DebugColor { get; set; } = JominiColor.Empty;
   [ParseAs("movement_cost", AstNodeType.ContentNode)]
   [Description("The movement cost multiplier for units moving through this topography.")]
   public float MovementCost { get; set; } = 1f;
   [ParseAs("vegetation_density", AstNodeType.ContentNode)]
   [Description("The density of vegetation in this topography, effects unknown.")]
   public float VegetationDensity { get; set; } = 1f;
   [ParseAs("weather_front_strength_change_percent", AstNodeType.ContentNode)]
   [Description("How much weather fronts are strengthened or weakened when passing through this topography, as a percentage." +
                "\n-0.08 #every FRONT_DEGRADATION_DISTANCE_FOR_TOPOGRAPHY pixels moved")]
   public float WeatherFrontStrengthChangePercent { get; set; }
   [ParseAs("weather_cyclone_strength_change_percent", AstNodeType.ContentNode)]
   [Description("How much cyclones are strengthened or weakened when passing through this topography, as a percentage.")]
   public float WeatherCycloneStrengthChangePercent { get; set; }
   [ParseAs("weather_tornado_strength_change_percent", AstNodeType.ContentNode)]
   [Description("How much tornadoes are strengthened or weakened when passing through this topography, as a percentage.")]
   public float WeatherTornadoStrengthChangePercent { get; set; }
   [ParseAs("defender", AstNodeType.ContentNode)]
   [Description("The defender bonus provided by this topography.")]
   public int DefenderDice { get; set; }
   [ParseAs("blocked_in_winter", AstNodeType.ContentNode)]
   [Description("Whether this topography is impassable during winter.")]
   public bool BlockedInWinter { get; set; }
   [ParseAs("can_have_ice", AstNodeType.ContentNode)]
   [Description("Whether ice can form on this topography.")]
   public bool CanHaveIce { get; set; }
   [ParseAs("can_freeze_over", AstNodeType.ContentNode)]
   [Description("Whether this topography can freeze over completely.")]
   public bool CanFreezeOver { get; set; }
   [ParseAs("has_sand", AstNodeType.ContentNode)]
   [Description("Whether this topography has sand")]
   public bool HasSand { get; set; }
   [ParseAs("is_deep_ocean", AstNodeType.ContentNode)]
   [Description("Whether this topography is deep ocean.")]
   public bool IsDeepOcean { get; set; }
   [ParseAs("is_lake", AstNodeType.ContentNode)]
   [Description("Whether this topography is a lake.")]
   public bool IsLake { get; set; }

   # endregion

   #region Interface Properties

   public bool IsReadonly => false;
   public NUISetting NUISettings => Config.Settings.NUIObjectSettings.TopographySettings;
   public INUINavigation[] Navigations { get; } = [];
   public static Topography Empty { get; } = new("Arcanum_Empty_Topography");
   public static IEnumerable<Topography> GetGlobalItems() => Globals.Topography.Values;

   #endregion

   #region ISearchable

   public override string GetNamespace => nameof(Topography);
   public override IQueastorSearchSettings.Category SearchCategory
      => IQueastorSearchSettings.Category.MapObjects | IQueastorSearchSettings.Category.GameObjects;

   #endregion
}