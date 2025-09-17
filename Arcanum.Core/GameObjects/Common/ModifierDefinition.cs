using System.ComponentModel;
using Arcanum.API.UtilServices.Search;
using Arcanum.Core.CoreSystems.NUI;
using Arcanum.Core.CoreSystems.NUI.Attributes;
using Arcanum.Core.CoreSystems.Parsing.ToolBox;
using Arcanum.Core.CoreSystems.SavingSystem.AGS;
using Arcanum.Core.CoreSystems.SavingSystem.AGS.Attributes;
using Arcanum.Core.GameObjects.BaseTypes;
using Arcanum.Core.GlobalStates;
using Nexus.Core;

namespace Arcanum.Core.GameObjects.Common;

public enum IsModifierGood
{
   [EnumAgsData("good")]
   Good,

   [EnumAgsData("bad")]
   Bad,

   [EnumAgsData("neutral")]
   Neutral,
}

[Flags]
public enum BiasType
{
   [EnumAgsData("none")]
   None,

   [EnumAgsData("opinion")]
   Opinion,

   [EnumAgsData("trust")]
   Trust,

   [EnumAgsData("voting")]
   Voting,
}

[Flags]
public enum ModifierCategory
{
   [EnumAgsData("all", true)]
   All,

   [EnumAgsData("none", true)]
   None,

   [EnumAgsData("country")]
   Country,

   [EnumAgsData("location")]
   Location,

   [EnumAgsData("province")]
   Province,

   [EnumAgsData("character")]
   Character,

   [EnumAgsData("unit")]
   Unit,

   [EnumAgsData("mercenary")]
   Mercenary,

   [EnumAgsData("religion")]
   Religion,

   [EnumAgsData("internationalorganization")]
   InternationalOrganization,

   [EnumAgsData("rebel")]
   Rebel,
}

public enum ModifierFormat
{
   [EnumAgsData("default", true)]
   Default,

   [EnumAgsData("FormatPopCaps")]
   FormatPopCaps,

   [EnumAgsData("FormatManPower")]
   FormatManPower,

   [EnumAgsData("FormatGold")]
   FormatGold,
}

[ObjectSaveAs]
public partial class ModifierDefinition : IEu5Object<ModifierDefinition>
{
#pragma warning disable AGS004
   [ReadonlyNexus]
   [Description("Unique key of this object. Must be unique among all objects of this type.")]
   [DefaultValue("null")]
   public string UniqueKey { get; set; } = null!;
#pragma warning restore AGS004

   # region Nexus Properties

   [SaveAs]
   [DefaultValue(IsModifierGood.Good)]
   [ParseAs("color")]
   [Description("The color to display the modifier in.")]
   public IsModifierGood IsGood { get; set; } = IsModifierGood.Good;

   [SaveAs]
   [DefaultValue(2)]
   [ParseAs("decimals")]
   [Description("The number of decimal places to display the modifier with.")]
   public int NumDecimals { get; set; } = 2;

   [SaveAs]
   [DefaultValue(false)]
   [ParseAs("boolean")]
   [Description("Whether this modifier is a boolean (yes/no) value.")]
   public bool IsBoolean { get; set; }
   [SaveAs]
   [DefaultValue(false)]
   [ParseAs("percent")]
   [Description("Whether this modifier is a percentage value.")]
   public bool IsPercentage { get; set; }
   [SaveAs]
   [DefaultValue(false)]
   [ParseAs("already_percent")]
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
   public string SavingKey => UniqueKey;
   public INUINavigation[] Navigations { get; } = [];
   public static ModifierDefinition Empty { get; } = new() { UniqueKey = "Arcanum_Empty_ModifierDefinition" };
   public static IEnumerable<ModifierDefinition> GetGlobalItems() => Globals.ModifierDefinitions.Values;

   #endregion

   #region ISearchable

   public string GetNamespace => nameof(ModifierDefinition);
   public string ResultName => UniqueKey;
   public List<string> SearchTerms => [UniqueKey];

   public void OnSearchSelected()
   {
   }

   public ISearchResult VisualRepresentation => new SearchResultItem(null, UniqueKey, string.Empty);
   public IQueastorSearchSettings.Category SearchCategory => IQueastorSearchSettings.Category.AbstractObjects;

   #endregion
}