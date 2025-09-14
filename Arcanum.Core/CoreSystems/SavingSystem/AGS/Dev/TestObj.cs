using System.ComponentModel;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.Parser;
using Arcanum.Core.CoreSystems.Parsing.ToolBox;
using Arcanum.Core.CoreSystems.SavingSystem.AGS.Attributes;
using Nexus.Core;

namespace Arcanum.Core.CoreSystems.SavingSystem.AGS.Dev;

[ObjectSaveAs(nameof(Key))]
public partial class Tes2tObj : IAgs
{
   [SaveAs(SavingValueType.Bool, TokenType.LessOrEqual)]
   [ParseAs(AstNodeType.ContentNode, "is_prop")]
   [Description("Hello there I am a property")]
   public bool IsProp { get; set; } = true;

   [SuppressAgs]
   [ParseAs(AstNodeType.ContentNode, "is_prop")]
   [Description("Hello there I am a property")]
   public string Key { get; set; } = "TestObj";
   public AgsSettings Settings { get; } = new();

   public static void Test()
   {
      var key = Field.Key;
   }
}