using System.ComponentModel;
using Arcanum.API.UtilServices.Search;
using Arcanum.Core.CoreSystems.NUI;
using Arcanum.Core.CoreSystems.Parsing.ParsingHelpers.ArcColor;
using Arcanum.Core.CoreSystems.Parsing.ToolBox;
using Arcanum.Core.GlobalStates;

namespace Arcanum.Core.GameObjects.Map;

public partial
   class Topography(string name) : NameKeyDefined(name), INUI, IEmpty<Topography>, ICollectionProvider<Topography>
{
   # region Nexus Properties

   [ParseAs(AstNodeType.ContentNode, "color")]
   [Description("The color used to represent this topography on the map.")]
   public JominiColor Color { get; set; } = JominiColor.Empty;
   [ParseAs(AstNodeType.ContentNode, "debug_color")]
   [Description("The color used for topology_screenshot mapmode")]
   public JominiColor DebugColor { get; set; } = JominiColor.Empty;
   [ParseAs(AstNodeType.ContentNode, "movement_cost")]
   [Description("The movement cost multiplier for units moving through this topography.")]
   public float MovementCost { get; set; } = 1f;
   [ParseAs(AstNodeType.ContentNode, "vegetation_density")]
   [Description("The density of vegetation in this topography, effects unknown.")]
   public float VegetationDensity { get; set; } = 1f;
   [ParseAs(AstNodeType.ContentNode, "weather_front_strength_change_percent")]
   [Description("How much weather fronts are strengthened or weakened when passing through this topography, as a percentage." +
                "\n-0.08 #every FRONT_DEGRADATION_DISTANCE_FOR_TOPOGRAPHY pixels moved")]
   public float WeatherFrontStrengthChangePercent { get; set; }
   [ParseAs(AstNodeType.ContentNode, "weather_cyclone_strength_change_percent")]
   [Description("How much cyclones are strengthened or weakened when passing through this topography, as a percentage.")]
   public float WeatherCycloneStrengthChangePercent { get; set; }
   [ParseAs(AstNodeType.ContentNode, "weather_tornado_strength_change_percent")]
   [Description("How much tornadoes are strengthened or weakened when passing through this topography, as a percentage.")]
   public float WeatherTornadoStrengthChangePercent { get; set; }
   [ParseAs(AstNodeType.ContentNode, "defender")]
   [Description("The defender bonus provided by this topography.")]
   public int DefenderDice { get; set; }
   [ParseAs(AstNodeType.ContentNode, "blocked_in_winter")]
   [Description("Whether this topography is impassable during winter.")]
   public bool BlockedInWinter { get; set; }
   [ParseAs(AstNodeType.ContentNode, "can_have_ice")]
   [Description("Whether ice can form on this topography.")]
   public bool CanHaveIce { get; set; }
   [ParseAs(AstNodeType.ContentNode, "can_freeze_over")]
   [Description("Whether this topography can freeze over completely.")]
   public bool CanFreezeOver { get; set; }
   [ParseAs(AstNodeType.ContentNode, "has_sand")]
   [Description("Whether this topography has sand")]
   public bool HasSand { get; set; }
   [ParseAs(AstNodeType.ContentNode, "is_deep_ocean")]
   [Description("Whether this topography is deep ocean.")]
   public bool IsDeepOcean { get; set; }
   [ParseAs(AstNodeType.ContentNode, "is_lake")]
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