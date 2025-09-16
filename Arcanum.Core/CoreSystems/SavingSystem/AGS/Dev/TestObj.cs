using System.ComponentModel;
using Arcanum.Core.CoreSystems.NUI;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.Parser;
using Arcanum.Core.CoreSystems.Parsing.ToolBox;
using Arcanum.Core.CoreSystems.SavingSystem.AGS.Attributes;

namespace Arcanum.Core.CoreSystems.SavingSystem.AGS.Dev;

[ObjectSaveAs]
public partial class TestOb2j : IAgs, ICollectionProvider<IAgs>
{
   [SaveAs(separator: TokenType.LessOrEqual)]
   [ParseAs(AstNodeType.ContentNode, "is_prop")]
   [Description("Hello there I am a property")]
   public List<bool> IsProp { get; set; } = new();

   [SaveAs(savingMethod: "ExampleCustomSavingMethod")]
   [ParseAs(AstNodeType.ContentNode, "key")]
   [Description("Hello there I am a property")]
   public string Key { get; set; } = "boh_hussite_king";

   [SaveAs]
   [ParseAs(AstNodeType.BlockNode, "inline")]
   public InlineTestObject Inline { get; set; } = new();

   public AgsSettings Settings { get; } = new();
   public string SavingKey => "test_obj";
   public static IEnumerable<IAgs> GetGlobalItems() => new TestOb2j[] { new(), new() { Key = "second" } };
}