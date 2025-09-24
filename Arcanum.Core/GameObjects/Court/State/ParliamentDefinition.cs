using System.ComponentModel;
using Arcanum.Core.CoreSystems.NUI;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.ToolBox;
using Arcanum.Core.CoreSystems.SavingSystem.AGS;
using Arcanum.Core.CoreSystems.SavingSystem.AGS.Attributes;
using Arcanum.Core.GameObjects.BaseTypes;

namespace Arcanum.Core.GameObjects.Court.State;

[ObjectSaveAs]
#pragma warning disable ARC002
public partial class ParliamentDefinition : INUI, IAgs, IEmpty<ParliamentDefinition>
#pragma warning restore ARC002
{
   [SaveAs]
   [DefaultValue("")]
   [Description("The type of this parliament definition.")]
   [ParseAs("parliament_type")]
   public string Type { get; set; } = string.Empty;

   public bool IsReadonly => false;
   public NUISetting NUISettings => Config.Settings.NUIObjectSettings.ParliamentDefinitionSettings;
   public INUINavigation[] Navigations => throw new NotImplementedException();
   public AgsSettings AgsSettings => Config.Settings.AgsSettings.ParliamentDefinitionAgsSettings;
   public string SavingKey => string.Empty;
   public static ParliamentDefinition Empty { get; } = new() { Type = "Arcanum_empty_parliament_definition" };
}