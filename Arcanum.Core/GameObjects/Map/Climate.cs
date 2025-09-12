using System.ComponentModel;
using Arcanum.API.UtilServices.Search;
using Arcanum.Core.CoreSystems.NUI;
using Arcanum.Core.CoreSystems.Parsing.ParsingHelpers.ArcColor;
using Arcanum.Core.CoreSystems.Parsing.ToolBox;
using Arcanum.Core.GlobalStates;

namespace Arcanum.Core.GameObjects.Map;

public partial
   class Climate(string name) : NameKeyDefined(name), INUI, IEmpty<Climate>, ICollectionProvider<Climate>
{
   #region Enums

   public enum WinterType
   {
      None,
      Normal,
      Mild,
      Severe,
   }

   #endregion

   # region Nexus Properties

   [ParseAs(AstNodeType.ContentNode, "winter")]
   [Description("What type of winter this climate has. Options are None, Light, Normal, and Harsh.")]
   public WinterType Winter { get; set; } = WinterType.None;

   [ParseAs(AstNodeType.ContentNode, "color")]
   [Description("The Color the climate has on the map.")]
   public JominiColor Color { get; set; } = JominiColor.Empty;

   [ParseAs(AstNodeType.ContentNode, "debug_color")]
   [Description("The debug color the climate has on the climate_screenshot mapmode.")]
   public JominiColor DebugMapColor { get; set; } = JominiColor.Empty;

   [ParseAs(AstNodeType.ContentNode, "has_precipitation")]
   [Description("Whether this climate has precipitation.")]
   public bool HasPrecipitation { get; set; } = true;

   [ParseAs(AstNodeType.ContentNode, "always_winter")]
   [Description("Whether this climate is always in winter, regardless of the season.")]
   public bool AlwaysWinter { get; set; } = false;

   # endregion

   #region Interface Properties

   public bool IsReadonly => false;
   public NUISetting Settings => Config.Settings.NUIObjectSettings.ClimateSettings;
   public INUINavigation[] Navigations { get; } = [];
   public static Climate Empty { get; } = new("Arcanum_Empty_Climate");
   public static IEnumerable<Climate> GetGlobalItems() => Globals.Climates.Values;

   #endregion

   #region ISearchable

   public override string GetNamespace => nameof(Climate);
   public override IQueastorSearchSettings.Category SearchCategory
      => IQueastorSearchSettings.Category.MapObjects | IQueastorSearchSettings.Category.GameObjects;

   #endregion
}