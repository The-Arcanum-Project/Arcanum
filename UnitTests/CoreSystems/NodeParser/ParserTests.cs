using Arcanum.Core.CoreSystems.Parsing.NodeParser.Parser;

namespace UnitTests.CoreSystems.NodeParser;

[TestFixture]
public class ParserTests
{
   // A master helper that combines lexing and parsing for convenience.
   // Most tests will call this.
   private RootNode ParseSource(string source)
   {
      // 1. Lex the source code
      // We need a temporary buffer for the lexer to write into.
      var tokenBuffer = new Token[source.Length + 1]; // Guaranteed to be large enough
      var lexer = new Lexer(source.AsSpan(), tokenBuffer.AsSpan());
      var tokens = lexer.ScanTokens();

      // 2. Parse the token stream
      var parser = new Parser(tokens, source);
      return parser.Parse();
   }

   // A helper to get the single statement from a source string, which is common in tests.
   // It asserts that there is only one statement, making tests cleaner.
   private T GetSingleStatement<T>(string source) where T : StatementNode
   {
      var root = ParseSource(source);
      Assert.That(root.Statements, Has.Count.EqualTo(1), "Expected the root to contain exactly one statement.");

      var statement = root.Statements[0];
      Assert.That(statement, Is.InstanceOf<T>(), $"Expected statement to be of type {typeof(T).Name}.");

      return (T)statement;
   }

   [TestCase("name = \"GLaDOS\"", "=", TokenType.String, "GLaDOS")]
   [TestCase("version > 1.0", ">", TokenType.Number, "1.0")]
   [TestCase("is_enabled = yes", "=", TokenType.Yes, "yes")]
   [TestCase("difficulty <= hard", "<=", TokenType.Identifier, "hard")]
   [TestCase("release_date = 2007.10.10", "=", TokenType.Date, "2007.10.10")]
   public void Parses_ContentNode_With_Various_Values(string source, string sep, TokenType valType, string valText)
   {
      var node = GetSingleStatement<ContentNode>(source);

      Assert.That(node.KeyNode.GetLexeme(source), Is.EqualTo(source.Split(' ')[0]));
      Assert.That(node.Separator.GetLexeme(source), Is.EqualTo(sep));

      Assert.That(node.Value, Is.InstanceOf<LiteralValueNode>());
      var literalValue = (LiteralValueNode)node.Value;
      var lexeme = literalValue.Value.GetLexeme(source);

      Assert.That(literalValue.Value.Type, Is.EqualTo(valType));
      Assert.That(lexeme, Is.EqualTo(valText));
   }

   [Test]
   public void Parses_ContentNode_With_AtIdentifier_Key()
   {
      var source = "@variable = 123";
      var node = GetSingleStatement<ContentNode>(source);

      Assert.That(node.KeyNode.Type, Is.EqualTo(TokenType.AtIdentifier));
      Assert.That(node.KeyNode.GetLexeme(source), Is.EqualTo("variable"));
   }

   [Test]
   public void Parses_Named_BlockNode()
   {
      var source = "graphics = { width = 1920 }";
      var node = GetSingleStatement<BlockNode>(source);

      Assert.That(node.KeyNode.GetLexeme(source), Is.EqualTo("graphics"));
      Assert.That(node.Children, Has.Count.EqualTo(1));
      Assert.That(node.Children[0], Is.InstanceOf<ContentNode>());
   }

   [Test]
   public void Parses_Anonymous_BlockNode_As_Statement()
   {
      var source = "{ stockholm gotland }";
      var node = GetSingleStatement<BlockNode>(source);

      // For anonymous blocks, the KeyNode is the LeftBrace token itself.
      Assert.That(node.KeyNode.Type, Is.EqualTo(TokenType.LeftBrace));
      Assert.That(node.Children, Has.Count.EqualTo(2));
      Assert.That(node.Children[0], Is.InstanceOf<KeyOnlyNode>());
      Assert.That(node.Children[1], Is.InstanceOf<KeyOnlyNode>());
   }

   [Test]
   public void Parses_KeyOnlyNodes_Inside_A_Block()
   {
      var source = "provinces = { one two three }";
      var node = GetSingleStatement<BlockNode>(source);

      Assert.That(node.Children, Has.Count.EqualTo(3));

      var child = node.Children[0] as KeyOnlyNode;
      Assert.That(child, Is.Not.Null);
      Assert.That(child.KeyNode.GetLexeme(source), Is.EqualTo("one"));
   }

   [TestCase("scripted_trigger")]
   [TestCase("scripted_effect")]
   public void Parses_ScriptedStatementNode(string keyword)
   {
      var source = $"{keyword} my_trigger = {{ has_flag = my_flag }}";
      var node = GetSingleStatement<ScriptedStatementNode>(source);

      Assert.That(node.KeyNode.GetLexeme(source), Is.EqualTo(keyword));
      Assert.That(node.Name.GetLexeme(source), Is.EqualTo("my_trigger"));
      Assert.That(node.Children, Has.Count.EqualTo(1));
   }

   [Test]
   public void Parses_FunctionCallNode_As_Value()
   {
      var source = "color = rgb { 255 128 0 }";
      var node = GetSingleStatement<ContentNode>(source);

      Assert.That(node.Value, Is.InstanceOf<FunctionCallNode>());
      var funcCall = (FunctionCallNode)node.Value;

      Assert.That(funcCall.FunctionName.GetLexeme(source), Is.EqualTo("rgb"));
      Assert.That(funcCall.Arguments, Has.Count.EqualTo(3));
      Assert.That(funcCall.Arguments[0], Is.InstanceOf<LiteralValueNode>());
   }

   [Test]
   public void Parses_MathExpressionNode_As_Value()
   {
      var source = "value = @[ some_var * 2 ]";
      var node = GetSingleStatement<ContentNode>(source);

      Assert.That(node.Value, Is.InstanceOf<MathExpressionNode>());
      var mathNode = (MathExpressionNode)node.Value;

      Assert.That(mathNode.Tokens, Has.Count.EqualTo(3));
      Assert.That(mathNode.Tokens[0].GetLexeme(source), Is.EqualTo("some_var"));
      Assert.That(mathNode.Tokens[1].Type, Is.EqualTo(TokenType.Multiply));
   }

   [Test]
   public void Parses_UnaryNode_As_Value()
   {
      var source = "offset = -10";
      var node = GetSingleStatement<ContentNode>(source);

      Assert.That(node.Value, Is.InstanceOf<UnaryNode>());
      var unaryNode = (UnaryNode)node.Value;

      Assert.That(unaryNode.Operator.Type, Is.EqualTo(TokenType.Minus));
      Assert.That(unaryNode.Value, Is.InstanceOf<LiteralValueNode>());
   }

   [Test]
   public void Parses_Deeply_Nested_Structures()
   {
      var source = @"
        root = {
            child1 = {
                grandchild = ""text""
            }
            child2 = {
                items = { a b c }
            }
        }";

      var rootBlock = GetSingleStatement<BlockNode>(source);
      Assert.That(rootBlock.KeyNode.GetLexeme(source), Is.EqualTo("root"));
      Assert.That(rootBlock.Children, Has.Count.EqualTo(2));

      // Inspect child1
      var child1 = rootBlock.Children[0] as BlockNode;
      Assert.That(child1, Is.Not.Null);
      Assert.That(child1.KeyNode.GetLexeme(source), Is.EqualTo("child1"));
      Assert.That(child1.Children, Has.Count.EqualTo(1));
      var grandchild = child1.Children[0] as ContentNode;
      Assert.That(grandchild, Is.Not.Null);
      Assert.That(grandchild.KeyNode.GetLexeme(source), Is.EqualTo("grandchild"));

      // Inspect child2
      var child2 = rootBlock.Children[1] as BlockNode;
      Assert.That(child2, Is.Not.Null);
      Assert.That(child2.Children[0], Is.InstanceOf<BlockNode>());
   }

   [Test]
   public void Throws_Exception_On_Missing_Closing_Brace_In_Block()
   {
      var source = "gfx = { setting = 1\n";
      // NUnit's way of asserting that a specific piece of code throws an exception.
      Assert.Throws<NullReferenceException>(() => ParseSource(source));
   }

   [Test]
   public void Throws_Exception_On_Missing_Value_After_Equals()
   {
      var source = "name = ";
      Assert.Throws<NullReferenceException>(() => ParseSource(source));
   }

   [Test]
   public void Throws_Exception_On_Statement_Starting_With_Separator()
   {
      var source = "= 123";
      Assert.Throws<NullReferenceException>(() => ParseSource(source));
   }
}