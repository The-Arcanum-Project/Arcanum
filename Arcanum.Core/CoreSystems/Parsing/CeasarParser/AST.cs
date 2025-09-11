// Ast.cs

using Arcanum.Core.CoreSystems.Common;
using Arcanum.Core.GameObjects.Culture;

namespace Arcanum.Core.CoreSystems.Parsing.CeasarParser;

/// <summary>
/// base class for all AST nodes
/// </summary>
public abstract class AstNode
{
   public abstract (int, int) GetLocation();
}

/// <summary>
/// Root of a file containing a list of top level statements
/// </summary>
public class RootNode : AstNode
{
   public List<StatementNode> Statements { get; } = [];
   public override (int, int) GetLocation() => Statements.Count > 0 ? Statements[0].GetLocation() : (0, 0);
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
   /// <summary>
   /// Represents a named or array block: `graphics = { ... }` or `{ ... }`
   /// </summary>
   public BlockNode(Token identifier)
   {
      KeyNode = identifier;
   }

   public List<StatementNode> Children { get; } = [];
   public override (int, int) GetLocation() => (KeyNode.Line, KeyNode.Column);
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
}

/// <summary>
/// A value that is itself an anonymous block, e.g., in `background = { key = value }`
/// </summary>
public class BlockValueNode : ValueNode
{
   public List<StatementNode> Children { get; } = [];
   public override (int, int) GetLocation() => Children.Count > 0 ? Children[0].GetLocation() : (0, 0);
}

/// <summary>
/// Represents a scripted statement like `scripted_trigger name = { ... }`
/// </summary>
public class ScriptedStatementNode : StatementNode
{
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
}

/// <summary>
/// Represents a unary expression, like a negative number.
/// e.g., the "-10" in `offset = -10`
/// </summary>
public class UnaryNode(Token op, ValueNode right) : ValueNode
{
   public Token Operator { get; } = op; // The operator token (e.g., '-')
   public ValueNode Right { get; } = right; // The value being operated on
   public override (int, int) GetLocation() => (Operator.Line, Operator.Column);
}