using System.ComponentModel;
using Arcanum.API.UtilServices.Search;
using Arcanum.Core.CoreSystems.NUI;
using Arcanum.Core.CoreSystems.Parsing.ParsingHelpers.ArcColor;
using Arcanum.Core.CoreSystems.Parsing.ToolBox;
using Arcanum.Core.GameObjects.BaseTypes;
using Arcanum.Core.GlobalStates;

namespace Arcanum.Core.GameObjects.Map;

public partial
   class Vegetation(string name) : OldNameKeyDefined(name), INUI, IEmpty<Vegetation>, ICollectionProvider<Vegetation>
{
   # region Nexus Properties

   [ParseAs("color", AstNodeType.ContentNode)]
   [Description("The color associated with this vegetation type used on the map.")]
   public JominiColor Color { get; set; } = JominiColor.Empty;

   [ParseAs("debug_color", AstNodeType.ContentNode)]
   [Description("The debug color associated with this vegetation type used in the vegetation_screenshot mapmode.")]
   public JominiColor DebugColor { get; set; } = JominiColor.Empty;

   [ParseAs("movement_cost", AstNodeType.ContentNode)]
   [Description("The movement cost modifier for units moving through this vegetation type.")]
   public float MovementCost { get; set; } = 1f;

   [ParseAs("has_sand", AstNodeType.ContentNode)]
   [Description("Whether this vegetation type includes sandy terrain, affecting certain gameplay mechanics.")]
   public bool HasSand { get; set; } = false;

   [ParseAs("defender", AstNodeType.ContentNode)]
   [Description("The defender bonus provided by this vegetation type in combat scenarios.")]
   public int DefenderDice { get; set; } = 0;

   # endregion

   #region Interface Properties

   public bool IsReadonly => false;
   public NUISetting NUISettings => Config.Settings.NUIObjectSettings.VegetationSettings;
   public INUINavigation[] Navigations { get; } = [];
   public static Vegetation Empty { get; } = new("Arcanum_Empty_Vegetation");
   public static IEnumerable<Vegetation> GetGlobalItems() => Globals.Vegetation.Values;

   #endregion

   #region ISearchable

   public override string GetNamespace => nameof(Vegetation);
   public override IQueastorSearchSettings.Category SearchCategory
      => IQueastorSearchSettings.Category.MapObjects | IQueastorSearchSettings.Category.GameObjects;

   #endregion
}