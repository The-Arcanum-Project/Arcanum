using System.ComponentModel;
using Arcanum.Core.CoreSystems.NUI;
using Arcanum.Core.CoreSystems.NUI.Attributes;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.ToolBox;
using Arcanum.Core.CoreSystems.SavingSystem.AGS;
using Arcanum.Core.CoreSystems.SavingSystem.AGS.Attributes;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.GameObjects.BaseTypes;
using Arcanum.Core.GameObjects.BaseTypes.InjectReplace;

namespace Arcanum.Core.GameObjects.InGame.Map.LocationCollections.SubObjects;

public enum VariableDataType
{
   [EnumAgsData("boolean")]
   Bool,

   [EnumAgsData("value")]
   Value,
}

[ObjectSaveAs]
public partial class VariableDataBlock : IEmbeddedEu5Object<VariableDataBlock>
{
   [SaveAs(SavingValueType.Enum)]
   [ParseAs("type")]
   [DefaultValue(VariableDataType.Bool)]
   [Description("The data type of this variable data block.")]
   public VariableDataType DataType { get; set; } = VariableDataType.Bool;

   [SaveAs(SavingValueType.Int)]
   [ParseAs("identity")]
   [DefaultValue(0)]
   [Description("The identity number of this variable data block.")]
   public int Identity { get; set; }

   public NUISetting NUISettings => Config.Settings.NUIObjectSettings.VariableDataBlockSettings;
   public AgsSettings AgsSettings => Config.Settings.AgsSettings.VariableDataBlockAgsSettings;

   [PropertyConfig(isReadonly: true)]
   public string UniqueId { get; set; } = string.Empty;
   public Eu5FileObj Source { get; set; } = Eu5FileObj.Empty;
   public Eu5ObjectLocation FileLocation { get; set; } = Eu5ObjectLocation.Empty;
   public InjRepType InjRepType { get; set; } = InjRepType.None;
   public static VariableDataBlock Empty { get; } = new() { UniqueId = "VARIABLE_DATA_BLOCK_EMPTY" };
}