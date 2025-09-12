using System.ComponentModel;
using Arcanum.Core.CoreSystems.NUI;
using Arcanum.Core.CoreSystems.Parsing.ToolBox;
using Arcanum.Core.GlobalStates;

namespace Arcanum.Core.GameObjects.Common;

public enum IsModifierGood
{
   Good,
   Bad,
   Neutral,
}

[Flags]
public enum BiasType
{
   None,
   Opinion,
   Trust,
   Voting,
}

[Flags]
public enum ModifierCategory
{
   All,
   None,
   COuntry,
   Location,
   Province,
   Character,
   Unit,
   Mercenary,
   Religion,
   InternationalOrganization,
   Rebel,
}

public enum ModifierFormat
{
   Default,
   FormatPopsCaps,
   FormatManPower,
   FormatGold,
}

public partial class ModifierDefinition(string name) : NameKeyDefined(name),
                                                       INUI,
                                                       IEmpty<ModifierDefinition>,
                                                       ICollectionProvider<ModifierDefinition>
{
   # region Nexus Properties

   [ParseAs(AstNodeType.ContentNode, "color")]
   [Description("The color to display the modifier in.")]
   public IsModifierGood IsGood { get; set; } = IsModifierGood.Good;
   [ParseAs(AstNodeType.ContentNode, "decimals")]
   [Description("The number of decimal places to display the modifier with.")]
   public int NumDecimals { get; set; } = 2;
   [ParseAs(AstNodeType.ContentNode, "min")]
   [Description("The minimum value the modifier can have.")]
   public int Min { get; set; }
   [ParseAs(AstNodeType.ContentNode, "max")]
   [Description("The maximum value the modifier can have.")]
   public int Max { get; set; }
   [ParseAs(AstNodeType.ContentNode, "category")]
   [Description("The categories this modifier can apply to.")]
   public ModifierCategory Category { get; set; } = ModifierCategory.All;
   [ParseAs(AstNodeType.ContentNode, "ai")]
   [Description("Whether the ai should use this modifier in its calculations.")]
   public bool Ai { get; set; }
   [ParseAs(AstNodeType.ContentNode, "boolean")]
   [Description("Whether this modifier is a boolean (yes/no) value.")]
   public bool IsBoolean { get; set; }
   [ParseAs(AstNodeType.ContentNode, "percent")]
   [Description("Whether this modifier is a percentage value.")]
   public bool IsPercentage { get; set; }
   [ParseAs(AstNodeType.ContentNode, "should_show_in_modifier_tab")]
   [Description("Whether this modifier should be shown in the country modifier tab of the UI.")]
   public bool ShouldShowInModifierTab { get; set; } = true;
   [ParseAs(AstNodeType.ContentNode, "already_percent")]
   [Description("Whether this modifier is already in percent form (i.e., 10 for 10%).")]
   public bool IsAlreadyPercent { get; set; }
   [ParseAs(AstNodeType.ContentNode, "scale_with_pop")]
   [Description("Whether this modifier scales with the pop size.")]
   public bool ScaleWithPop { get; set; }
   [ParseAs(AstNodeType.ContentNode, "cap_zero_to_one")]
   [Description("Whether this modifier is capped between 0 and 1.")]
   public bool CapZeroToOne { get; set; }
   [ParseAs(AstNodeType.ContentNode, "format")]
   [Description("The format to display the modifier in.")]
   public ModifierFormat Format { get; set; } = ModifierFormat.Default;
   [ParseAs(AstNodeType.ContentNode, "bias_type")]
   [Description("The type of bias this modifier applies to.")]
   public BiasType Bias { get; set; } = BiasType.None;

   # endregion

   #region Interface Properties

   public bool IsReadonly => false;
   public NUISetting Settings => Config.Settings.NUIObjectSettings.ModifierDefinitionSettings;
   public INUINavigation[] Navigations { get; } = [];
   public static ModifierDefinition Empty { get; } = new("Arcanum_Empty_ModifierDefinition");
   public static IEnumerable<ModifierDefinition> GetGlobalItems() => Globals.ModifierDefinitions.Values;

   #endregion
}