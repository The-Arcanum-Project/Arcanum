using System.ComponentModel;
using Arcanum.API.UtilServices.Search;
using Arcanum.Core.CoreSystems.NUI;
using Arcanum.Core.CoreSystems.Parsing.ToolBox;
using Arcanum.Core.CoreSystems.SavingSystem.AGS;
using Arcanum.Core.CoreSystems.SavingSystem.AGS.Attributes;
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
   Country,
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
   FormatPopCaps,
   FormatManPower,
   FormatGold,
}

[ObjectSaveAs]
public partial class ModifierDefinition(string name) : NameKeyDefined(name),
                                                       IAgs,
                                                       INUI,
                                                       IEmpty<ModifierDefinition>,
                                                       ICollectionProvider<ModifierDefinition>
{
   # region Nexus Properties

   [SaveAs]
   [ParseAs(AstNodeType.ContentNode, "color")]
   [Description("The color to display the modifier in.")]
   public IsModifierGood IsGood { get; set; } = IsModifierGood.Good;

   [SaveAs]
   [ParseAs(AstNodeType.ContentNode, "decimals")]
   [Description("The number of decimal places to display the modifier with.")]
   public int NumDecimals { get; set; } = 2;

   [SaveAs]
   [ParseAs(AstNodeType.ContentNode, "boolean")]
   [Description("Whether this modifier is a boolean (yes/no) value.")]
   public bool IsBoolean { get; set; }
   [SaveAs]
   [ParseAs(AstNodeType.ContentNode, "percent")]
   [Description("Whether this modifier is a percentage value.")]
   public bool IsPercentage { get; set; }
   [SaveAs]
   [ParseAs(AstNodeType.ContentNode, "already_percent")]
   [Description("Whether this modifier is already in percent form (i.e., 10 for 10%).")]
   public bool IsAlreadyPercent { get; set; }

   [SaveAs]
   [ParseAsEmbedded("game_data")]
   [Description("Game data associated with this modifier.")]
   public ModifierGameData GameData { get; set; } = new();

   # endregion

   #region Interface Properties

   public bool IsReadonly => false;
   public NUISetting NUISettings => Config.Settings.NUIObjectSettings.ModifierDefinitionSettings;
   public AgsSettings AgsSettings { get; } = new();
   public string SavingKey => Name;
   public INUINavigation[] Navigations { get; } = [];
   public static ModifierDefinition Empty { get; } = new("Arcanum_Empty_ModifierDefinition");
   public static IEnumerable<ModifierDefinition> GetGlobalItems() => Globals.ModifierDefinitions.Values;

   #endregion

   #region ISearchable

   public override string GetNamespace => nameof(ModifierDefinition);
   public override IQueastorSearchSettings.Category SearchCategory => IQueastorSearchSettings.Category.AbstractObjects;

   #endregion
}