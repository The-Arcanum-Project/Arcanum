using System.ComponentModel;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.Parser;
using Arcanum.Core.CoreSystems.Parsing.ToolBox;
using Arcanum.Core.CoreSystems.SavingSystem.AGS.Attributes;

namespace Arcanum.Core.CoreSystems.SavingSystem.AGS.Dev;

[ObjectSaveAs(nameof(Key))]
public partial class TestObj : IAgs
{
   [SaveAs(SavingValueType.Bool, TokenType.LessOrEqual)]
   [ParseAs(AstNodeType.ContentNode, "is_prop")]
   [Description("Hello there I am a property")]
   public bool IsProp { get; set; } = true;

   [SuppressAgs]
   [Description("Hello there I am a property")]
   public string Key { get; set; } = "TestObj";
   public AgsSettings Settings { get; } = new();
}