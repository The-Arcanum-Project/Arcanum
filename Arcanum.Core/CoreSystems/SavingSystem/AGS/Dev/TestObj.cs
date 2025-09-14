using System.ComponentModel;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.Parser;
using Arcanum.Core.CoreSystems.Parsing.ToolBox;
using Arcanum.Core.CoreSystems.SavingSystem.AGS.Attributes;
using Nexus.Core;

namespace Arcanum.Core.CoreSystems.SavingSystem.AGS.Dev;

public partial class TestObj : IAgs
{
   [SaveAs(SavingValueType.Bool, TokenType.LessOrEqual)]
   [ParseAs(AstNodeType.ContentNode, "is_prop")]
   [Description("Hello there I am a property")]
   public bool IsProp { get; set; } = true;
   public AgsSettings Settings { get; } = new();
}