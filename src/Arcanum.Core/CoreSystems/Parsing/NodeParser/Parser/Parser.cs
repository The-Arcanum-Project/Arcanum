using System.Text;
using Arcanum.Core.CoreSystems.Common;
using Arcanum.Core.CoreSystems.ErrorSystem.BaseErrorTypes;
using Arcanum.Core.CoreSystems.ErrorSystem.Diagnostics;
using Arcanum.Core.CoreSystems.SavingSystem.Util;

namespace Arcanum.Core.CoreSystems.Parsing.NodeParser.Parser;

public sealed class Parser(LexerResult lexerResult)
{
   private readonly string _source = lexerResult.Source;
   private readonly IReadOnlyList<Token> _tokens = lexerResult.Tokens;
   private readonly int _tokensCount = lexerResult.Tokens.Count;
   private int _current;
   private static Eu5FileObj _fileObj = Eu5FileObj.Empty;

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
         DiagnosticException.CreateAndHandle(new (1, 1, fileObj.Path.FullPath),
                                             IOError.Instance.FileReadingError,
                                             "AST-Building",
                                             DiagnosticSeverity.Warning,
                                             DiagnosticReportSeverity.PopupNotify,
                                             fileObj.Path.FullPath);

         source = string.Empty;
         return new (0, 0);
      }

      var lexer = new Lexer(source);
      var lexerResult = lexer.ScanTokens();
      var parser = new Parser(lexerResult);
      return parser.Parse();
   }

   public RootNode Parse()
   {
      var root = new RootNode(0, _tokens[^1].End);
      while (!IsAtEnd())
         root.Statements.Add(ParseStatement());

      return root;
   }

   private KeyNodeBase ParseKey()
   {
      // Return simple key (handles quoted strings, etc).
      if (!Check(TokenType.Identifier))
         return new SimpleKeyNode(Advance());

      // If the next token IS NOT a ':', it is a simple identifier key.
      if (!CheckNext(TokenType.ScopeSeparator))
         return new SimpleKeyNode(Advance());

      // It is a scoped key. Consume the chain.
      var segments = new List<Token> { Advance() };

      while (Check(TokenType.ScopeSeparator))
      {
         Advance();
         var nextPart = Expect(TokenType.Identifier, "identifier after scope separator ':'.");
         segments.Add(nextPart);
      }

      return new ScopedKeyNode(segments);
   }

   private bool CheckTokenText(Token token, string text)
   {
      return _source.AsSpan(token.Start, token.Length).SequenceEqual(text);
   }

   private StatementNode ParseStatement()
   {
      if (Check(TokenType.LeftBrace))
         return ParseAnonymousBlock();

      if (Check(TokenType.Minus))
      {
         var value = ParseValue();
         if (value is UnaryNode unaryNode)
            return new UnaryStatementNode(unaryNode);
      }

      // Allow statements to begin with an Identifier, Date, Number or Quoted String
      if (Check(TokenType.Identifier) ||
          Check(TokenType.Date) ||
          Check(TokenType.Number) ||
          Check(TokenType.String))
      {
         // Check for scripted_trigger/effect pattern...
         if (Check(TokenType.Identifier) && CheckNext(TokenType.Identifier) && CheckAt(2, TokenType.Equals))
         {
            if (CheckTokenText(Peek(), "scripted_trigger") || CheckTokenText(Peek(), "scripted_effect"))
               return ParseScriptedStatement();
         }

         var key = ParseKey();

         return Peek().Type switch
         {
            TokenType.Equals when PeekNext().Type == TokenType.LeftBrace => ParseBlockStatement(key),
            TokenType.LeftBrace => ParseBlockStatement(key),
            TokenType.Equals
            or TokenType.NotEquals
            or TokenType.Less
            or TokenType.Greater
            or TokenType.LessOrEqual
            or TokenType.GreaterOrEqual
            or TokenType.QuestionEquals => ParseContentStatement(key),
            _ => new KeyOnlyNode(key),
         };
      }

      if (Check(TokenType.AtIdentifier))
         return ParseContentStatement(ParseKey());

      // ReSharper disable twice ConditionalAccessQualifierIsNonNullableAccordingToAPIContract
      var current = Current();
      DiagnosticException.CreateAndHandle(new (current.Line, current.Column, _fileObj?.Path?.FullPath ?? "N/A"),
                                          ParsingError.Instance.SyntaxError,
                                          "AST-Building",
                                          DiagnosticSeverity.Error,
                                          DiagnosticReportSeverity.PopupError,
                                          current.Line,
                                          _tokens[0].Column,
                                          current.GetValue(_source),
                                          "a block or content definition");

      throw
         new ($"Syntax Error on line {Peek().Line}: Unexpected token '{Peek().GetValue(_source)}' where a statement was expected.");
   }

   private BlockNode ParseAnonymousBlock()
   {
      var brace = Expect(TokenType.LeftBrace, "'{' to start anonymous block.");
      var block = new BlockNode(brace); // Use the '{' token as the identifier
      while (!Check(TokenType.RightBrace) && !IsAtEnd())
         block.Children.Add(ParseStatement());

      Expect(TokenType.RightBrace, "'}' to close anonymous block.");
      block.ClosingToken = Previous();
      return block;
   }

   private StatementNode ParseContentStatement(KeyNodeBase key)
   {
      var separator = Advance();
      var value = ParseValue();
      return new ContentNode(key, separator, value);
   }

   private BlockNode ParseBlockStatement(KeyNodeBase key)
   {
      Match(TokenType.Equals);

      Expect(TokenType.LeftBrace, $"'{{' after block name '{key.GetKeyText(_source)}'.");
      var block = new BlockNode(key);

      while (!Check(TokenType.RightBrace) && !IsAtEnd())
         block.Children.Add(ParseStatement());

      Expect(TokenType.RightBrace, "'}' to close the block.");
      block.ClosingToken = Previous();
      return block;
   }

   private ValueNode ParseValue()
   {
      if (Match(TokenType.Minus))
      {
         var op = Previous();
         var right = ParseValue();
         return new UnaryNode(op, right);
      }

      // Check for Identifier driven values
      if (Check(TokenType.Identifier))
      {
         // Function Call: identifier followed by {
         if (CheckNext(TokenType.LeftBrace))
            return ParseFunctionCallNode();

         // Scoped Identifier: identifier followed by :
         if (CheckNext(TokenType.ScopeSeparator))
         {
            var segments = new List<Token> { Advance() };

            while (Check(TokenType.ScopeSeparator))
            {
               Advance();
               var nextPart = Expect(TokenType.Identifier, "identifier after scope separator ':'.");
               segments.Add(nextPart);
            }

            return new ScopedIdentifierNode(segments);
         }
      }

      // A block used as a value, e.g., OR = { ... }
      if (Match(TokenType.LeftBrace))
      {
         var brace = Previous();
         var blockValue = new BlockValueNode(brace);
         while (!Check(TokenType.RightBrace) && !IsAtEnd())
            blockValue.Children.Add(ParseStatement());

         Expect(TokenType.RightBrace, "'}' to close block value.");
         blockValue.ClosingToken = Previous();
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
      if (Match(TokenType.Number, TokenType.String, TokenType.Yes, TokenType.No, TokenType.Identifier, TokenType.Date))
         return new LiteralValueNode(Previous());

      DiagnosticException.CreateAndHandle(new (Current().Line, Current().Column, _fileObj.Path.FullPath),
                                          ParsingError.Instance.SyntaxError,
                                          "AST-Building",
                                          DiagnosticSeverity.Error,
                                          DiagnosticReportSeverity.PopupError,
                                          Current().Line,
                                          _tokens[0].Column,
                                          Current().GetValue(_source),
                                          "a value");

      throw
         new ($"Syntax Error on line {Peek().Line}: Unexpected token '{Peek().GetValue(_source)}' where a value was expected.");
   }

   private FunctionCallNode ParseFunctionCallNode()
   {
      var name = Expect(TokenType.Identifier, "function name.");
      var funcCall = new FunctionCallNode(name);

      Expect(TokenType.LeftBrace, $"'{{' after function name '{name.GetValue(_source)}'.");

      while (!Check(TokenType.RightBrace) && !IsAtEnd())
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

      DiagnosticException.CreateAndHandle(new (Current().Line, Current().Column, _fileObj.Path.FullPath),
                                          ParsingError.Instance.SyntaxError,
                                          "AST-Building",
                                          DiagnosticSeverity.Error,
                                          DiagnosticReportSeverity.PopupError,
                                          Current().Line,
                                          _tokens[0].Column,
                                          Current().GetValue(_source),
                                          message);

      throw new ($"Syntax Error on line {Peek().Line}: {message}");
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

      var node = new ScriptedStatementNode(keyword, name, keyword.Start, name.End - keyword.Start);

      Expect(TokenType.Equals, $"Expected '=' after name in '{keyword.GetValue(_source)}' statement.");
      Expect(TokenType.LeftBrace, "Expected '{' to open scripted statement block.");

      while (!Check(TokenType.RightBrace) && !IsAtEnd())
         node.Children.Add(ParseStatement());

      Expect(TokenType.RightBrace, "Expected '}' to close scripted statement block.");
      node.ClosingToken = Previous();
      return node;
   }

   private void SkipUnexpectedTokens()
   {
      while (_tokens[_current].Type == TokenType.Unexpected)
         if (_current < _tokensCount)
            _current++;
         else
            // The last token is always EOF, which is not Unexpected.
            break;
   }

   #endregion

   #region Print AST

   public static string PrintAst(AstNode node, string source = "")
   {
      var sb = new StringBuilder();
      PrintAst(node, sb, "   ", source);
      return sb.ToString();
   }

   public static void PrintAst(AstNode node, StringBuilder sb, string indent = "", string source = "")
   {
      switch (node)
      {
         case RootNode root:
            sb.AppendLine($"{indent}Root:");
            root.Statements.ForEach(s => PrintAst(s, sb, indent + "  ", source));
            break;

         case BlockNode block:
            var name = block.KeyNode.GetKeyText(source);
            if (block.KeyNode is SimpleKeyNode skn && skn.KeyToken.Type == TokenType.LeftBrace)
               name = "Array Block";

            sb.AppendLine($"{indent}Block: '{name}'");
            block.Children.ForEach(c => PrintAst(c, sb, indent + "  ", source));
            break;

         case UnaryNode unary:
            sb.Append($"Unary: '{unary.Operator.GetValue(source)}' on ");
            PrintValue(unary.Value, source, sb);
            break;

         case KeyOnlyNode keyOnly:
            sb.AppendLine($"{indent}Key: '{keyOnly.KeyNode.GetKeyText(source)}'");
            break;

         case ScriptedStatementNode scripted:
            var keyword = scripted.KeyNode.GetKeyText(source);
            var name2 = scripted.Name.GetValue(source);
            sb.AppendLine($"{indent}ScriptedStatement: '{keyword}' on '{name2}'");
            scripted.Children.ForEach(c => PrintAst(c, sb, indent + "  ", source));
            break;

         case ContentNode content:
            var key = content.KeyNode.GetKeyText(source);
            var sep = content.Separator.GetValue(source);
            sb.Append($"{indent}Content: '{key}' {sep} ");
            PrintValue(content.Value, source, sb);
            break;

         case ScopedIdentifierNode scoped:
            sb.AppendLine($"ScopedIdentifier: '{scoped.GetKeyText(source)}'");
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
         case ScopedIdentifierNode scoped:
            sb.AppendLine($"ScopedIdentifier: '{scoped.GetKeyText(source)}'");
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
         case UnaryNode unary:
            sb.Append(unary.Operator.GetLexeme(source)).AppendLine(((LiteralValueNode)unary.Value).Value.GetValue(source));
            break;
         case BlockValueNode blockVal:
            sb.AppendLine("BlockValue:");
            blockVal.Children.ForEach(c => PrintAst(c, sb, "  ", source));
            break;
      }
   }

   #endregion

   #region Utility Methods

   public static bool VerifyNodeTypes(List<AstNode> node, Type[] allowedTypes, ref ParsingContext pc)
   {
      var allValid = true;
      foreach (var n in node)
      {
         var type = n.GetType();
         if (allowedTypes.Contains(type))
            continue;

         DiagnosticException.LogWarning(ref pc,
                                        ParsingError.Instance.InvalidBlockType,
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
                                                ref ParsingContext pc,
                                                out List<T> results)
      where T : AstNode
   {
      results = nodes.OfType<T>().ToList();
      if (expectedCount == -1)
         expectedCount = nodes.Count;
      var actualCount = results.Count;
      if (actualCount != expectedCount)
      {
         DiagnosticException.LogWarning(ref pc,
                                        ParsingError.Instance.InvalidNodeCountOfType,
                                        typeof(T).Name,
                                        expectedCount,
                                        actualCount);
         return false;
      }

      return true;
   }

   public static bool GetIdentifierKvp(StatementNode node,
                                       ref ParsingContext pc,
                                       out string key,
                                       out string value)
   {
      if (node is not ContentNode cn)
      {
         key = string.Empty;
         value = string.Empty;
         pc.SetContext(node);
         DiagnosticException.LogWarning(ref pc,
                                        ParsingError.Instance.InvalidContentKeyOrType,
                                        node.GetType(),
                                        "a content node");
         return false;
      }

      if (cn.Value is not LiteralValueNode lvn || lvn.Value.Type != TokenType.Identifier)
      {
         key = string.Empty;
         value = string.Empty;
         pc.SetContext(node);
         DiagnosticException.LogWarning(ref pc,
                                        ParsingError.Instance.InvalidContentKeyOrType,
                                        pc.SliceString(cn),
                                        "a string value and key");
         return false;
      }

      key = pc.SliceString(cn.KeyNode);
      value = pc.SliceString(lvn.Value);
      return true;
   }

   #endregion
}