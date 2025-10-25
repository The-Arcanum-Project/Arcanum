// Ast.cs

namespace Arcanum.Core.CoreSystems.Parsing.NodeParser.Parser;

/// <summary>
/// base class for all AST nodes
/// </summary>
public abstract class AstNode
{
   public abstract (int, int) GetLocation();

   public abstract (int line, int charPos) GetEndLocation();

   protected static (int, int) GetTokenEnd(Token token)
   {
      return (token.Line, token.Column + token.Length);
   }
}

/// <summary>
/// Root of a file containing a list of top level statements
/// </summary>
public class RootNode : AstNode
{
   public List<StatementNode> Statements { get; } = [];
   public override (int, int) GetLocation() => Statements.Count > 0 ? Statements[0].GetLocation() : (0, 0);

   public override (int line, int charPos) GetEndLocation()
      => Statements.Count > 0 ? Statements.Last().GetEndLocation() : (0, 0);
}

/// <summary>
/// A base class for all statement nodes: blocks, content pairs, and scripted statements
/// </summary>
public abstract class StatementNode : AstNode
{
   public Token KeyNode { get; init; }
}

/// <summary>
/// Represents a named or array block: `graphics = { ... }` or `{ ... }`
/// </summary>
public class BlockNode : StatementNode
{
   public Token? ClosingToken;

   /// <summary>
   /// Represents a named or array block: `graphics = { ... }` or `{ ... }`
   /// </summary>
   public BlockNode(Token identifier)
   {
      KeyNode = identifier;
   }

   public List<StatementNode> Children { get; } = [];
   public override (int, int) GetLocation() => (KeyNode.Line, KeyNode.Column);

   public override (int line, int charPos) GetEndLocation()
   {
      if (ClosingToken != null)
         return GetTokenEnd(ClosingToken.Value);

      return Children.Count > 0 ? Children.Last().GetEndLocation() : GetTokenEnd(KeyNode);
   }
}

/// <summary>
/// Represents a key-value pair: `width = 1280`
/// </summary>
public class ContentNode : StatementNode
{
   /// <summary>
   /// Represents a key-value pair: `width = 1280`
   /// </summary>
   public ContentNode(Token key, Token separator, ValueNode value)
   {
      Separator = separator;
      Value = value;
      KeyNode = key;
   }

   public Token Separator { get; } // The separator token (e.g., '=', '<=', etc.)
   public ValueNode Value { get; } // The value on the right-hand side
   public override (int, int) GetLocation() => (KeyNode.Line, KeyNode.Column);
   public override (int line, int charPos) GetEndLocation() => Value.GetEndLocation();
}

/// <summary>
/// A base class for all possible value types
/// </summary>
public abstract class ValueNode : AstNode;

/// <summary>
/// A simple literal value: a number, a string, 'yes', 'no', or an identifier like 'high'
/// </summary>
/// <param name="value"></param>
public class LiteralValueNode(Token value) : ValueNode
{
   public Token Value { get; } = value;
   public override (int, int) GetLocation() => (Value.Line, Value.Column);
   public override (int line, int charPos) GetEndLocation() => GetTokenEnd(Value);
}

/// <summary>
/// An inline math expression: `@[ 2 * 3 + 1 ]`
/// </summary>
/// <param name="tokens"></param>
public class MathExpressionNode(IReadOnlyList<Token> tokens) : ValueNode
{
   // For now, we just capture all the tokens inside @[ ... ]
   public IReadOnlyList<Token> Tokens { get; } = tokens;
   public override (int, int) GetLocation() => Tokens.Count > 0 ? (Tokens[0].Line, Tokens[0].Column) : (0, 0);
   public override (int line, int charPos) GetEndLocation() => Tokens.Count > 0 ? GetTokenEnd(Tokens[^1]) : (0, 0);
}

/// <summary>
/// A function call: `rgb { 255 0 0 }`
/// </summary>
/// <param name="functionName"></param>
public class FunctionCallNode(Token functionName) : ValueNode
{
   public Token FunctionName { get; } = functionName; // e.g., 'rgb' or 'hsv' 'hsv360'
   public List<ValueNode> Arguments { get; } = [];
   public override (int, int) GetLocation() => (FunctionName.Line, FunctionName.Column);

   public override (int line, int charPos) GetEndLocation()
   {
      return Arguments.Count > 0 ? Arguments.Last().GetEndLocation() : GetTokenEnd(FunctionName);
   }
}

/// <summary>
/// A value that is itself an anonymous block, e.g., in `background = { key = value }`
/// </summary>
public class BlockValueNode : ValueNode
{
   /// <summary>
   /// A value that is itself an anonymous block, e.g., in `background = { key = value }`
   /// </summary>
   public BlockValueNode(Token openingToken)
   {
      OpeningToken = openingToken;
   }

   public Token? OpeningToken { get; }
   public Token? ClosingToken { get; set; }

   public List<StatementNode> Children { get; } = [];
   public override (int, int) GetLocation() => Children.Count > 0 ? Children[0].GetLocation() : (0, 0);

   public override (int line, int charPos) GetEndLocation()
   {
      if (ClosingToken != null)
         return GetTokenEnd(ClosingToken.Value);
      if (Children.Count > 0)
         return Children.Last().GetEndLocation();
      if (OpeningToken != null)
         return GetTokenEnd(OpeningToken.Value);

      return (0, 0);
   }
}

/// <summary>
/// Represents a scripted statement like `scripted_trigger name = { ... }`
/// </summary>
public class ScriptedStatementNode : StatementNode
{
   public Token? ClosingToken;

   /// <summary>
   /// Represents a scripted statement like `scripted_trigger name = { ... }`
   /// </summary>
   public ScriptedStatementNode(Token keyword, Token name)
   {
      Name = name;
      KeyNode = keyword;
   }

   public Token Name { get; } // The identifier for the defined scripted token
   public List<StatementNode> Children { get; } = []; // The content inside the braces
   public override (int, int) GetLocation() => (KeyNode.Line, KeyNode.Column);

   public override (int line, int charPos) GetEndLocation()
   {
      if (ClosingToken != null)
         return GetTokenEnd(ClosingToken.Value);
      if (Children.Count > 0)
         return Children.Last().GetEndLocation();

      return GetTokenEnd(Name);
   }
}

/// <summary>
/// Represents a statement that is just a key with no value, common in lists.
/// e.g., the "stockholm" in `own_control_core = { stockholm norrtalje ... }`
/// </summary>
public class KeyOnlyNode : StatementNode
{
   public KeyOnlyNode(Token key)
   {
      KeyNode = key;
   }

   public override (int, int) GetLocation() => (KeyNode.Line, KeyNode.Column);
   public override (int line, int charPos) GetEndLocation() => GetTokenEnd(KeyNode);
}

/// <summary>
/// Represents a unary expression, like a negative number.
/// e.g., the "-10" in `offset = -10`
/// </summary>
public class UnaryNode(Token op, ValueNode value) : ValueNode
{
   public Token Operator { get; } = op; // The operator token (e.g., '-')
   public ValueNode Value { get; } = value; // The value being operated on
   public override (int, int) GetLocation() => (Operator.Line, Operator.Column);
   public override (int line, int charPos) GetEndLocation() => Value.GetEndLocation();
}

/// <summary>
/// Represents a statement that is just a unary expression, like a negative number in a list.
/// e.g., the "-10" in `movement_assistance = { -10 20 }`
/// </summary>
public class UnaryStatementNode : StatementNode
{
   public UnaryNode Value { get; }

   public UnaryStatementNode(UnaryNode value)
   {
      Value = value;
      KeyNode = value.Operator;
   }

   public override (int, int) GetLocation() => Value.GetLocation();
   public override (int line, int charPos) GetEndLocation() => Value.GetEndLocation();
}