using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using Arcanum.Core.CoreSystems.NUI;
using Arcanum.Core.CoreSystems.Parsing.ToolBox;
using Arcanum.Core.CoreSystems.SavingSystem.AGS;
using Arcanum.Core.CoreSystems.SavingSystem.AGS.Attributes;
using Arcanum.Core.GameObjects.BaseTypes;
using Arcanum.Core.GlobalStates;

namespace Arcanum.Core.GameObjects.Common;

[ObjectSaveAs]
public partial class ModifierGameData
   : INUI, IEmpty<ModifierGameData>, ICollectionProvider<ModifierGameData>, IAgs
{
   #region Nexus Properties

   [DefaultValue(0)]
   [ParseAs(AstNodeType.ContentNode, "min")]
   [Description("The minimum value the modifier can have.")]
   [SaveAs]
   public int Min { get; set; }
   [ParseAs(AstNodeType.ContentNode, "max")]
   [Description("The maximum value the modifier can have.")]
   [SaveAs]
   [DefaultValue(0)]
   public int Max { get; set; }
   [ParseAs(AstNodeType.ContentNode, "category")]
   [Description("The categories this modifier can apply to.")]
   [SaveAs(SavingValueType.FlagsEnum)]
   [DefaultValue((ModifierCategory)0)]
   public ModifierCategory Category { get; set; } = ModifierCategory.All;
   [ParseAs(AstNodeType.ContentNode, "ai")]
   [Description("Whether the ai should use this modifier in its calculations.")]
   [SaveAs]
   [DefaultValue(false)]
   public bool Ai { get; set; }
   [ParseAs(AstNodeType.ContentNode, "should_show_in_modifier_tab")]
   [Description("Whether this modifier should be shown in the country modifier tab of the UI.")]
   [SaveAs]
   [DefaultValue(true)]
   public bool ShouldShowInModifierTab { get; set; } = true;
   [ParseAs(AstNodeType.ContentNode, "scale_with_pop")]
   [Description("Whether this modifier scales with the pop size.")]
   [SaveAs]
   [DefaultValue(false)]
   public bool ScaleWithPop { get; set; }
   [ParseAs(AstNodeType.ContentNode, "cap_zero_to_one")]
   [Description("Whether this modifier is capped between 0 and 1.")]
   [SaveAs]
   [DefaultValue(false)]
   public bool CapZeroToOne { get; set; }
   [ParseAs(AstNodeType.ContentNode, "format")]
   [Description("The format to display the modifier in.")]
   [SaveAs]
   [DefaultValue(ModifierFormat.Default)]
   public ModifierFormat Format { get; set; } = ModifierFormat.Default;
   [ParseAs(AstNodeType.ContentNode, "bias_type")]
   [Description("The type of bias this modifier applies to.")]
   [SaveAs(SavingValueType.FlagsEnum)]
   [DefaultValue(BiasType.None)]
   public BiasType Bias { get; set; } = BiasType.None;

   # endregion

   #region Interface Properties

   public bool IsReadonly => false;
   public NUISetting NUISettings => Config.Settings.NUIObjectSettings.ModifierGameDataSettings;
   public INUINavigation[] Navigations { get; } = [];
   public static ModifierGameData Empty { get; } = new();

   public static IEnumerable<ModifierGameData> GetGlobalItems()
      => Globals.ModifierDefinitions.Values.Select(x => x.GameData);

   #endregion

   public override string ToString() => Category.ToString();

   protected bool Equals(ModifierGameData other) => Min == other.Min &&
                                                    Max == other.Max &&
                                                    Category == other.Category &&
                                                    Ai == other.Ai &&
                                                    ShouldShowInModifierTab == other.ShouldShowInModifierTab &&
                                                    ScaleWithPop == other.ScaleWithPop &&
                                                    CapZeroToOne == other.CapZeroToOne &&
                                                    Format == other.Format;

   [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
   public override int GetHashCode() => HashCode.Combine(Min,
                                                         Max,
                                                         (int)Category,
                                                         Ai,
                                                         ShouldShowInModifierTab,
                                                         ScaleWithPop,
                                                         CapZeroToOne,
                                                         (int)Format);

   public override bool Equals(object? obj)
   {
      if (obj is not ModifierGameData other)
         return false;

      return Min == other.Min &&
             Max == other.Max &&
             Category == other.Category &&
             Ai == other.Ai &&
             ShouldShowInModifierTab == other.ShouldShowInModifierTab &&
             ScaleWithPop == other.ScaleWithPop &&
             CapZeroToOne == other.CapZeroToOne &&
             Format == other.Format &&
             Bias == other.Bias;
   }

   public AgsSettings AgsSettings { get; } = Config.Settings.AgsSettings.ModifierDataSettings;
   public string SavingKey => "game_data";
}