using System.Text;
using Arcanum.Core.CoreSystems.Common;
using Arcanum.Core.CoreSystems.ErrorSystem.BaseErrorTypes;
using Arcanum.Core.CoreSystems.ErrorSystem.Diagnostics;
using Arcanum.Core.CoreSystems.SavingSystem.Util;

namespace Arcanum.Core.CoreSystems.Parsing.NodeParser.Parser;

public class Parser(LexerResult lexerResult)
{
   private readonly string _source = lexerResult.Source;
   private readonly IReadOnlyList<Token> _tokens = lexerResult.Tokens;
   private readonly int _tokensCount = lexerResult.Tokens.Count;
   private int _current;
   private static Eu5FileObj _fileObj = null!;

   public static RootNode Parse(Eu5FileObj fileObj, out string source, out LocationContext ctx)
   {
      _fileObj = fileObj;
      ctx = LocationContext.GetNew(fileObj);
      var rn = Parse(fileObj, out source);
      _fileObj = null!;
      return rn;
   }

   public static RootNode Parse(Eu5FileObj fileObj, out string source)
   {
      source = IO.IO.ReadAllTextUtf8(fileObj.Path.FullPath)!;
      if (string.IsNullOrWhiteSpace(source))
      {
         DiagnosticException.CreateAndHandle(new(1, 1, _fileObj.Path.FullPath),
                                             IOError.Instance.FileReadingError,
                                             "AST-Building",
                                             DiagnosticSeverity.Warning,
                                             DiagnosticReportSeverity.PopupNotify,
                                             fileObj.Path.FullPath);

         source = string.Empty;
         return new();
      }

      var lexer = new Lexer(source);
      var lexerResult = lexer.ScanTokens();
      var parser = new Parser(lexerResult);
      return parser.Parse();
   }

   public RootNode Parse()
   {
      var root = new RootNode();
      while (!IsAtEnd())
         root.Statements.Add(ParseStatement());

      return root;
   }

   private StatementNode ParseStatement()
   {
      const string scriptedTrigger = "scripted_trigger";
      const string scriptedEffect = "scripted_effect";
      // Case: Array block `{ ... }`
      if (Check(TokenType.LeftBrace))
         return ParseAnonymousBlock();

      // Allow statements to begin with an Identifier, Date, Number or Quoted String
      if (Check(TokenType.Identifier) ||
          Check(TokenType.Date) ||
          Check(TokenType.Number) ||
          Check(TokenType.String))
      {
         // Check for scripted_trigger/effect pattern...
         if (Check(TokenType.Identifier) && CheckNext(TokenType.Identifier) && CheckAt(2, TokenType.Equals))
         {
            var keyword = Peek().GetValue(_source);
            if (keyword is scriptedTrigger or scriptedEffect)
               return ParseScriptedStatement();
         }

         // Identifiers, Dates, and Numbers as keys
         switch (PeekNext().Type)
         {
            case TokenType.LeftBrace:
               return ParseBlockStatement();
            case TokenType.Equals
              or TokenType.NotEquals
              or TokenType.Less
              or TokenType.Greater
              or TokenType.LessOrEqual
              or TokenType.GreaterOrEqual
              or TokenType.QuestionEquals:
               return ParseContentOrBlockStatement();
         }

         return new KeyOnlyNode(Advance());
      }

      if (Check(TokenType.AtIdentifier))
         return ParseContentOrBlockStatement();

      DiagnosticException.CreateAndHandle(new(Current().Line, Current().Column, _fileObj.Path.FullPath),
                                          ParsingError.Instance.SyntaxError,
                                          "AST-Building",
                                          DiagnosticSeverity.Error,
                                          DiagnosticReportSeverity.PopupError,
                                          Current().Line,
                                          _tokens[0].Column,
                                          Current().GetValue(_source),
                                          "a block or content definition");

      throw
         new($"Syntax Error on line {Peek().Line}: Unexpected token '{Peek().GetValue(_source)}' where a statement was expected.");
   }

   private BlockNode ParseAnonymousBlock()
   {
      var brace = Expect(TokenType.LeftBrace, "'{' to start anonymous block.");
      var block = new BlockNode(brace); // Use the '{' token as the identifier
      while (!Check(TokenType.RightBrace) && !IsAtEnd())
         block.Children.Add(ParseStatement());

      Expect(TokenType.RightBrace, "'}' to close anonymous block.");
      return block;
   }

   private StatementNode ParseContentOrBlockStatement()
   {
      var key = Advance();
      var separator = Advance();

      // If the value is a block `{...}`, we treat the whole thing as a BlockNode.
      if (Check(TokenType.LeftBrace))
      {
         var block = ParseBlockStatement(key);
         return block;
      }

      // Otherwise, it's a standard ContentNode like `width = 1280`.
      var value = ParseValue();
      return new ContentNode(key, separator, value);
   }

   private BlockNode ParseBlockStatement(Token? knownIdentifier = null)
   {
      var name = knownIdentifier ?? Advance(); // Use the passed identifier or consume a new one

      // Handle optional `=` before the brace
      if (knownIdentifier == null)
         Match(TokenType.Equals);

      Expect(TokenType.LeftBrace, $"'{{' after block name '{name.GetValue(_source)}'.");
      var block = new BlockNode(name);

      while (!Check(TokenType.RightBrace) && !IsAtEnd())
         block.Children.Add(ParseStatement());

      Expect(TokenType.RightBrace, "'}' to close the block.");
      return block;
   }

   private ValueNode ParseValue()
   {
      if (Match(TokenType.Minus))
      {
         var op = Previous();
         // After the '-', we recursively call ParseValue to get the operand.
         // This is powerful because it could handle `-(2+3)` if you extend the grammar later.
         var right = ParseValue();
         return new UnaryNode(op, right);
      }

      // An Identifier followed by a LeftBrace is a function call.
      if (Check(TokenType.Identifier) && PeekNext().Type == TokenType.LeftBrace)
         return ParseFunctionCallNode();

      // A block used as a value, e.g., OR = { ... }
      if (Match(TokenType.LeftBrace))
      {
         var blockValue = new BlockValueNode();
         while (!Check(TokenType.RightBrace) && !IsAtEnd())
            blockValue.Children.Add(ParseStatement());

         Expect(TokenType.RightBrace, "'}' to close block value.");
         return blockValue;
      }

      // @[ ... ] math expression
      if (Match(TokenType.LeftBracket))
      {
         var mathTokens = new List<Token>();
         while (!Check(TokenType.RightBracket) && !IsAtEnd())
            mathTokens.Add(Advance());

         Expect(TokenType.RightBracket, "']' to close math expression.");
         return new MathExpressionNode(mathTokens);
      }

      // FALLBACK: If it's not a special case, it must be a simple literal.
      // correctly handles `quality = high` without consuming `rgb` prematurely.
      if (Match(TokenType.Number, TokenType.String, TokenType.Yes, TokenType.No, TokenType.Identifier, TokenType.Date))
         return new LiteralValueNode(Previous());

      DiagnosticException.CreateAndHandle(new(Current().Line, Current().Column, _fileObj.Path.FullPath),
                                          ParsingError.Instance.SyntaxError,
                                          "AST-Building",
                                          DiagnosticSeverity.Error,
                                          DiagnosticReportSeverity.PopupError,
                                          Current().Line,
                                          _tokens[0].Column,
                                          Current().GetValue(_source),
                                          "a value");

      throw
         new($"Syntax Error on line {Peek().Line}: Unexpected token '{Peek().GetValue(_source)}' where a value was expected.");
   }

   private FunctionCallNode ParseFunctionCallNode()
   {
      var name = Expect(TokenType.Identifier, "function name.");
      var funcCall = new FunctionCallNode(name);

      Expect(TokenType.LeftBrace, $"'{{' after function name '{name.GetValue(_source)}'.");

      // Loop until we find the closing brace.
      while (!Check(TokenType.RightBrace) && !IsAtEnd())
         // Parse each argument as a value. This allows for nested function calls
         funcCall.Arguments.Add(ParseValue());

      Expect(TokenType.RightBrace, "'}' to close function call.");
      return funcCall;
   }

   #region Helper Methods

   private bool Match(params TokenType[] types)
   {
      foreach (var type in types)
         if (Check(type))
         {
            Advance();
            return true;
         }

      return false;
   }

   private Token Expect(TokenType type, string message)
   {
      if (Check(type))
         return Advance();

      DiagnosticException.CreateAndHandle(new(Current().Line, Current().Column, _fileObj.Path.FullPath),
                                          ParsingError.Instance.SyntaxError,
                                          "AST-Building",
                                          DiagnosticSeverity.Error,
                                          DiagnosticReportSeverity.PopupError,
                                          Current().Line,
                                          _tokens[0].Column,
                                          Current().GetValue(_source),
                                          message);

      throw new($"Syntax Error on line {Peek().Line}: {message}");
   }

   private Token Advance()
   {
      SkipUnexpectedTokens();

      if (!IsAtEnd())
         _current++;
      return Previous();
   }

   private bool IsAtEnd()
   {
      SkipUnexpectedTokens();

      if (_tokens[_current].Type == TokenType.EndOfFile)
         return true;

      return false;
   }

   private Token Peek()
   {
      SkipUnexpectedTokens();
      return _tokens[_current];
   }

   private Token Current() => _tokens[_current];
   private Token PeekNext() => _current + 1 >= _tokensCount ? _tokens[^1] : _tokens[_current + 1];
   private Token Previous() => _tokens[_current - 1];
   private bool Check(TokenType type) => !IsAtEnd() && Peek().Type == type;
   private Token PeekAt(int offset) => _current + offset >= _tokensCount ? _tokens[^1] : _tokens[_current + offset];
   private bool CheckNext(TokenType type) => !IsAtEnd() && PeekNext().Type == type;
   private bool CheckAt(int offset, TokenType type) => !IsAtEnd() && PeekAt(offset).Type == type;

   private ScriptedStatementNode ParseScriptedStatement()
   {
      var keyword = Advance(); // Consume 'scripted_trigger'
      var name = Advance(); // Consume the name

      var node = new ScriptedStatementNode(keyword, name);

      Expect(TokenType.Equals, $"Expected '=' after name in '{keyword.GetValue(_source)}' statement.");
      Expect(TokenType.LeftBrace, "Expected '{' to open scripted statement block.");

      // The content inside is just a list of normal statements. We can reuse ParseStatement!
      while (!Check(TokenType.RightBrace) && !IsAtEnd())
         node.Children.Add(ParseStatement());

      Expect(TokenType.RightBrace, "Expected '}' to close scripted statement block.");
      return node;
   }

   private void SkipUnexpectedTokens()
   {
      while (_tokens[_current].Type == TokenType.Unexpected)
      {
         if (_current < _tokensCount)
            _current++;
         else
            // The last token is always EOF, which is not Unexpected.
            break;
      }
   }

   #endregion

   #region Print AST

   public static void PrintAst(AstNode node, StringBuilder sb, string indent = "", string source = "")
   {
      switch (node)
      {
         case RootNode root:
            sb.AppendLine($"{indent}Root:");
            root.Statements.ForEach(s => PrintAst(s, sb, indent + "  ", source));
            break;

         case BlockNode block:
            var name = block.KeyNode.Type == TokenType.LeftBrace
                          ? "Array Block"
                          : block.KeyNode.GetValue(source);
            sb.AppendLine($"{indent}Block: '{name}'");
            block.Children.ForEach(c => PrintAst(c, sb, indent + "  ", source));
            break;

         case UnaryNode unary:
            sb.Append($"Unary: '{unary.Operator.GetValue(source)}' on ");
            PrintValue(unary.Value, source, sb);
            break;

         case KeyOnlyNode keyOnly:
            sb.AppendLine($"{indent}Key: '{keyOnly.KeyNode.GetValue(source)}'");
            break;

         case ScriptedStatementNode scripted:
            var keyword = scripted.KeyNode.GetValue(source);
            var name2 = scripted.Name.GetValue(source);
            sb.AppendLine($"{indent}ScriptedStatement: '{keyword}' on '{name2}'");
            scripted.Children.ForEach(c => PrintAst(c, sb, indent + "  ", source));
            break;

         case ContentNode content:
            var key = content.KeyNode.GetValue(source);
            var sep = content.Separator.GetValue(source);
            sb.Append($"{indent}Content: '{key}' {sep} ");
            PrintValue(content.Value, source, sb);
            break;
      }
   }

   private static void PrintValue(ValueNode value, string source, StringBuilder sb, string indent = "   ")
   {
      switch (value)
      {
         case LiteralValueNode literal:
            sb.AppendLine($"Literal: '{literal.Value.GetValue(source)}'");
            break;
         case MathExpressionNode math:
            var expr = string.Join(" ", math.Tokens.Select(t => t.GetValue(source)));
            sb.AppendLine($"Math: @[ {expr} ]");
            break;
         case FunctionCallNode func:
            sb.AppendLine($"Function: '{func.FunctionName.GetValue(source)}'");
            foreach (var arg in func.Arguments)
            {
               sb.Append($"{indent}  -> Arg: ");
               PrintValue(arg, source, sb);
            }

            break;
         case BlockValueNode blockVal:
            sb.AppendLine("BlockValue:");
            blockVal.Children.ForEach(c => PrintAst(c, sb, "  ", source));
            break;
      }
   }

   #endregion

   #region Utility Methods

   public static bool VerifyNodeTypes(List<AstNode> node, Type[] allowedTypes, LocationContext ctx, string actionName)
   {
      var allValid = true;
      foreach (var n in node)
      {
         var type = n.GetType();
         if (allowedTypes.Contains(type))
            continue;

         DiagnosticException.LogWarning(ctx,
                                        ParsingError.Instance.InvalidBlockType,
                                        actionName,
                                        n.GetLocation().Item1,
                                        n.GetLocation().Item2,
                                        type.Name,
                                        string.Join(", ", allowedTypes.Select(t => t.Name)));
         allValid = false;
      }

      return allValid;
   }

   public static bool EnforceNodeType<T>(AstNode node, LocationContext ctx, string actionName, out T? result)
      where T : AstNode
   {
      if (node is not T tNode)
      {
         result = null;
         var location = node.GetLocation();
         ctx.LineNumber = location.Item1;
         ctx.ColumnNumber = location.Item2;
         DiagnosticException.LogWarning(ctx.GetInstance(),
                                        ParsingError.Instance.InvalidNodeType,
                                        actionName,
                                        node.GetType(),
                                        typeof(T),
                                        "N/A");
         return false;
      }

      result = tNode;
      return true;
   }

   public static bool EnforceNodeCountOfType<T>(List<AstNode> nodes,
                                                int expectedCount,
                                                LocationContext ctx,
                                                string actionName,
                                                out List<T> results)
      where T : AstNode
   {
      results = nodes.OfType<T>().ToList();
      var actualCount = results.Count;
      if (actualCount != expectedCount)
      {
         DiagnosticException.LogWarning(ctx.GetInstance(),
                                        ParsingError.Instance.InvalidNodeCountOfType,
                                        actionName,
                                        typeof(T).Name,
                                        expectedCount,
                                        actualCount);
         return false;
      }

      return true;
   }

   public static bool EnforceNodeCountOfType<T>(List<StatementNode> nodes,
                                                int expectedCount,
                                                LocationContext ctx,
                                                string actionName,
                                                out List<T> results)
      where T : AstNode
   {
      results = nodes.OfType<T>().ToList();
      if (expectedCount == -1)
         expectedCount = nodes.Count;
      var actualCount = results.Count;
      if (actualCount != expectedCount)
      {
         DiagnosticException.LogWarning(ctx.GetInstance(),
                                        ParsingError.Instance.InvalidNodeCountOfType,
                                        actionName,
                                        typeof(T).Name,
                                        expectedCount,
                                        actualCount);
         return false;
      }

      return true;
   }

   public static bool GetIdentifierKvp(StatementNode node,
                                       LocationContext ctx,
                                       string actionName,
                                       string source,
                                       out string key,
                                       out string value)
   {
      if (node is not ContentNode cn)
      {
         key = string.Empty;
         value = string.Empty;
         var location = node.GetLocation();
         ctx.LineNumber = location.Item1;
         ctx.ColumnNumber = location.Item2;
         DiagnosticException.LogWarning(ctx.GetInstance(),
                                        ParsingError.Instance.InvalidContentKeyOrType,
                                        actionName,
                                        node.GetType(),
                                        "a content node");
         return false;
      }

      if (cn.Value is not LiteralValueNode lvn || lvn.Value.Type != TokenType.Identifier)
      {
         key = string.Empty;
         value = string.Empty;

         ctx.LineNumber = cn.KeyNode.Line;
         ctx.ColumnNumber = cn.KeyNode.Column;
         DiagnosticException.LogWarning(ctx.GetInstance(),
                                        ParsingError.Instance.InvalidContentKeyOrType,
                                        actionName,
                                        cn.KeyNode.GetLexeme(source),
                                        "a string value and key");
         return false;
      }

      key = cn.KeyNode.GetLexeme(source);
      value = lvn.Value.GetLexeme(source);
      return true;
   }

   #endregion
}