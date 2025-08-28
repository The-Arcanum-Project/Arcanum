// Ast.cs

namespace Arcanum.Core.CoreSystems.Parsing.CeasarParser;

/// <summary>
/// base class for all AST nodes
/// </summary>
public abstract class AstNode;

/// <summary>
/// Root of a file containing a list of top level statements
/// </summary>
public class RootNode : AstNode
{
   public List<StatementNode> Statements { get; } = [];
}

/// <summary>
/// A base class for all statement nodes: blocks, content pairs, and scripted statements
/// </summary>
public abstract class StatementNode : AstNode;

/// <summary>
/// Represents a named or array block: `graphics = { ... }` or `{ ... }`
/// </summary>
public class BlockNode(Token identifier) : StatementNode
{
   // The name token (e.g., 'graphics'). Is a LeftBrace token for anonymous blocks.
   public Token Identifier { get; } = identifier;
   public List<StatementNode> Children { get; } = [];
}

/// <summary>
/// Represents a key-value pair: `width = 1280`
/// </summary>
public class ContentNode(Token key, Token separator, ValueNode value) : StatementNode
{
   public Token Key { get; } = key; // The key token (e.g., 'width' or '@scale')
   public Token Separator { get; } = separator; // The separator token (e.g., '=', '<=', etc.)
   public ValueNode Value { get; } = value; // The value on the right-hand side
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
}

/// <summary>
/// An inline math expression: `@[ 2 * 3 + 1 ]`
/// </summary>
/// <param name="tokens"></param>
public class MathExpressionNode(IReadOnlyList<Token> tokens) : ValueNode
{
   // For now, we just capture all the tokens inside @[ ... ]
   public IReadOnlyList<Token> Tokens { get; } = tokens;
}

/// <summary>
/// A function call: `rgb { 255 0 0 }`
/// </summary>
/// <param name="functionName"></param>
public class FunctionCallNode(Token functionName) : ValueNode
{
   public Token FunctionName { get; } = functionName; // e.g., 'rgb' or 'hsv' 'hsv360'
   public List<ValueNode> Arguments { get; } = [];
}

/// <summary>
/// A value that is itself an anonymous block, e.g., in `background = { key = value }`
/// </summary>
public class BlockValueNode : ValueNode
{
   public List<StatementNode> Children { get; } = [];
}

/// <summary>
/// Represents a scripted statement like `scripted_trigger name = { ... }`
/// </summary>
public class ScriptedStatementNode(Token keyword, Token name) : StatementNode
{
   public Token Keyword { get; } = keyword; // The 'scripted_trigger' or 'scripted_effect' token
   public Token Name { get; } = name; // The identifier for the defined scripted token
   public List<StatementNode> Children { get; } = []; // The content inside the braces
}