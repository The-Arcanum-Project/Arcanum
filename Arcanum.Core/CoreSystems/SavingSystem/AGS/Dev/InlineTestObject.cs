#if DEBUG
using Arcanum.Core.CoreSystems.NUI;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.Parser;
using Arcanum.Core.CoreSystems.Parsing.ToolBox;
using Arcanum.Core.CoreSystems.SavingSystem.AGS.Attributes;

namespace Arcanum.Core.CoreSystems.SavingSystem.AGS.Dev;

[ObjectSaveAs]
public partial class InlineTestObject : IAgs, ICollectionProvider<IAgs>
{
   [SaveAs(separator: TokenType.LessOrEqual)]
   [ParseAs(AstNodeType.ContentNode, "id")]
   public int Id { get; set; } = Random.Shared.Next(0, 140);
   [SaveAs]
   [ParseAs(AstNodeType.ContentNode, "name")]
   public string Name { get; set; } = "Inline Test Object";
   [SaveAs]
   [ParseAs(AstNodeType.ContentNode, "description")]
   public string Description { get; set; } = "This is a test object for inline saving.";
   [SaveAs]
   [ParseAs(AstNodeType.ContentNode, "floating")]
   public float SomeFloat { get; set; } = 3.14f;
   [SaveAs]
   [ParseAs(AstNodeType.ContentNode, "some_bool")]
   public bool SomeBool { get; set; } = true;
   [SaveAs(separator: TokenType.Greater, valueType: SavingValueType.Identifier)]
   [ParseAs(AstNodeType.BlockNode, "some_strings")]
   public List<string> SomeStrings { get; set; } = ["One", "Two", "Three"];
   [SaveAs(separator: TokenType.Equals)]
   [ParseAs(AstNodeType.ContentNode, "formation")]
   public SavingFormat Format { get; set; } = SavingFormat.Spacious;

   public AgsSettings AgsSettings { get; } = new();
   public string SavingKey => "inline_this";
   public static IEnumerable<IAgs> GetGlobalItems() => [new InlineTestObject()];
}
#endif