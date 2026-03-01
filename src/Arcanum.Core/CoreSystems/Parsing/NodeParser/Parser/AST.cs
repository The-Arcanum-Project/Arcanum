namespace Arcanum.Core.CoreSystems.Parsing.NodeParser.Parser;

/// <summary>
///    base class for all AST nodes
/// </summary>
public abstract class AstNode(int start, int length)
{
   protected AstNode() : this(0, 0)
   {
   }

   public int Start => start;
   public int Length => length;
   public int End => Start + Length;

   // #MOD# Added properties to store comments for reconstruction
   /// <summary>
   ///    Comments appearing strictly before this node (on previous lines).
   /// </summary>
   public string? LeadingComment { get; set; }

   /// <summary>
   ///    Comments appearing on the same line as this node (e.g. key = val # comment).
   /// </summary>
   public string? InlineComment { get; set; }

   public abstract (int, int) GetLocation();

   public abstract (int line, int charPos) GetEndLocation();

   /// <summary>
   ///    Returns the end location (line and character position) of a given token.
   /// </summary>
   protected static (int line, int charPos) GetTokenEnd(Token token) => (token.Line, token.Start + token.Length);

   public virtual string GetKeyText(string? source) => string.Empty;
}

// #MOD# New Node Type: Represents a standalone structural comment
/// <summary>
///    Represents a standalone comment line (e.g. # TODO: Fix this)
///    strictly used for structural comments or file headers.
/// </summary>
public class CommentNode : StatementNode
{
   // Serializer Constructor
   public CommentNode(string text)
   {
      CommentText = text;
      // StatementNode requires a KeyNode. We create a dummy 0-length key.
      KeyNode = new SimpleKeyNode(string.Empty);
   }

   // Parser Constructor (for future use)
   public CommentNode(Token token) : base(token.Start, token.Length)
   {
      KeyNode = new SimpleKeyNode(token);
      CommentText = string.Empty; // Would be extracted later
   }

   public string CommentText { get; }

   public override (int, int) GetLocation() => KeyNode.GetLocation();
   public override (int line, int charPos) GetEndLocation() => KeyNode.GetEndLocation();
}

/// <summary>
///    Base class for nodes that can act as a key in a statement.
/// </summary>
public abstract class KeyNodeBase(int start, int length) : AstNode(start, length)
{
   protected KeyNodeBase() : this(0, 0)
   {
   }

   public int Column => GetLocation().Item2;
   public int Line => GetLocation().Item1;
   public string GetLexeme(Span<string> source) => source[Start..].ToString();

   public abstract string GetText(string? source);
}

/// <summary>
///    Represents a simple, single-token key (e.g., 'width').
/// </summary>
public class SimpleKeyNode(Token keyToken) : KeyNodeBase(keyToken.Start, keyToken.Length)
{
   private readonly string? _syntheticText;

   public SimpleKeyNode(string text) : this(default(Token)) => _syntheticText = text;

   public Token KeyToken { get; } = keyToken;

   public override (int, int) GetLocation() => (KeyToken.Line, KeyToken.Column);
   public override (int line, int charPos) GetEndLocation() => GetTokenEnd(KeyToken);

   public override string GetKeyText(string? source) => GetText(source);

   public override string GetText(string? source)
   {
      if (_syntheticText != null)
         return _syntheticText;
      if (source != null)
         return KeyToken.GetLexeme(source);

      return string.Empty;
   }
}

/// <summary>
///    Represents a scoped key (e.g., 'religion:shinto').
/// </summary>
public class ScopedKeyNode(List<Token> segments)
   : KeyNodeBase(segments[0].Start, segments[^1].End - segments[0].Start)
{
   private readonly string? _syntheticText;

   public ScopedKeyNode(string text) : this([default]) // Pass dummy list to primary
   {
      _syntheticText = text;
      Segments = null!; // Clear the dummy list if you want, or just ignore it
   }

   public List<Token> Segments { get; } = segments;

   public override (int, int) GetLocation() => Segments != null! ? (Segments[0].Line, Segments[0].Column) : (0, 0); // #MOD# Added null check

   public override (int line, int charPos) GetEndLocation() => Segments != null! ? GetTokenEnd(Segments[^1]) : (0, 0); // #MOD# Added null check

   public override string GetKeyText(string? source) => GetText(source); // #MOD# nullable source

   public override string GetText(string? source)
   {
      if (_syntheticText != null)
         return _syntheticText;
      if (source != null && Segments != null!)
         return string.Join(":", Segments.Select(t => t.GetLexeme(source)));

      return string.Empty;
   }
}

/// <summary>
///    Root of a file containing a list of top level statements
/// </summary>
public class RootNode(int start, int length) : AstNode(start, length)
{
   public RootNode() : this(0, 0)
   {
   }

   public List<StatementNode> Statements { get; } = [];
   public override (int, int) GetLocation() => Statements.Count > 0 ? Statements[0].GetLocation() : (0, 0);

   public override (int line, int charPos) GetEndLocation() => Statements.Count > 0 ? Statements.Last().GetEndLocation() : (0, 0);
}

/// <summary>
///    A base class for all statement nodes: blocks, content pairs, and scripted statements
/// </summary>
public abstract class StatementNode(int start, int length) : AstNode(start, length)
{
   protected StatementNode() : this(0, 0)
   {
   }

   // Changed from Token KeyNode to KeyNodeBase Key
   public KeyNodeBase KeyNode { get; protected init; } = null!;
}

/// <summary>
///    Represents a named or array block: `graphics = { ... }` or `{ ... }`
/// </summary>
public class BlockNode : StatementNode
{
   public Token? ClosingToken;

   /// <summary>
   ///    Represents a named or array block: `graphics = { ... }` or `{ ... }`
   /// </summary>
   // Updated constructor to accept KeyNodeBase
   public BlockNode(KeyNodeBase keyNode) : base(keyNode.Start, keyNode.Length) => KeyNode = keyNode;

   public BlockNode(string key) => KeyNode = new SimpleKeyNode(key);

   // Overload for anonymous blocks
   public BlockNode(Token openingBrace) : base(openingBrace.Start, openingBrace.Length) => KeyNode = new SimpleKeyNode(openingBrace);

   // Closing Comment (e.g. "} # End of graphics")
   public string? ClosingComment { get; set; }

   public List<StatementNode> Children { get; } = [];

   // Builder Factory for Anonymous blocks
   public static BlockNode CreateAnonymous() => new(default(Token)) { KeyNode = new SimpleKeyNode("") };
   public override (int, int) GetLocation() => KeyNode.GetLocation();

   public override (int line, int charPos) GetEndLocation()
   {
      if (ClosingToken != null)
         return (ClosingToken.Value.Line, ClosingToken.Value.End);

      return Children.Count > 0 ? Children.Last().GetEndLocation() : KeyNode.GetEndLocation();
   }
}

/// <summary>
///    Represents a key-value pair: `width = 1280`
/// </summary>
public class ContentNode : StatementNode
{
   /// <summary>
   ///    Represents a key-value pair: `width = 1280`
   /// </summary>
   // Updated constructor to accept KeyNodeBase
   public ContentNode(KeyNodeBase keyNode, Token separator, ValueNode value) :
      base(keyNode.Start, value.End - keyNode.Start)
   {
      Separator = separator;
      Value = value;
      KeyNode = keyNode;
   }

   public ContentNode(string key, ValueNode value)
   {
      KeyNode = new SimpleKeyNode(key);
      Value = value;
      // Separator Token is left default (empty), serializer assumes '='
   }

   public Token Separator { get; } // The separator token (e.g., '=', '<=', etc.)
   public ValueNode Value { get; } // The value on the right-hand side
   public override (int, int) GetLocation() => KeyNode.GetLocation();
   public override (int line, int charPos) GetEndLocation() => Value.GetEndLocation();
}

/// <summary>
///    A base class for all possible value types
/// </summary>
public abstract class ValueNode(int start, int length) : AstNode(start, length)
{
   protected ValueNode() : this(0, 0)
   {
   }
}

/// <summary>
///    A simple literal value: a number, a string, 'yes', 'no', or an identifier like 'high'
/// </summary>
/// <param name = "value" ></param>
public class LiteralValueNode(Token value) : ValueNode(value.Start, value.Length)
{
   private readonly string? _syntheticValue;

   public LiteralValueNode(string text) : this(default(Token)) => _syntheticValue = text;

   public Token Value { get; } = value;

   public string GetValueText(string? source)
   {
      if (_syntheticValue != null)
         return _syntheticValue;
      if (source != null)
         return Value.GetLexeme(source);

      return string.Empty;
   }

   public override (int, int) GetLocation() => (Value.Line, Value.Column);
   public override (int line, int charPos) GetEndLocation() => GetTokenEnd(Value);
}

/// <summary>
///    Represents a scoped identifier when used as a value, e.g. societal_value:centralization_vs_decentralization
/// </summary>
public class ScopedIdentifierNode(List<Token> segments)
   : ValueNode(segments[0].Start, segments[^1].End - segments[0].Start)
{
   private readonly string? _syntheticText;

   public ScopedIdentifierNode(string text) : this([default]) => _syntheticText = text;

   public List<Token> Segments { get; } = segments;

   public override (int, int) GetLocation() => (Segments[0].Line, Segments[0].Column);

   public override (int line, int charPos) GetEndLocation() => GetTokenEnd(Segments[^1]);

   public override string GetKeyText(string? source) // #MOD# nullable source
   {
      if (_syntheticText != null)
         return _syntheticText;
      if (source != null)
         return string.Join(":", Segments.Select(t => t.GetLexeme(source)));

      return "";
   }
}

/// <summary>
///    An inline math expression: `@[ 2 * 3 + 1 ]`
/// </summary>
public class MathExpressionNode : ValueNode
{
   /// <summary>
   ///    An inline math expression: `@[ 2 * 3 + 1 ]`
   /// </summary>
   /// <param name = "tokens" ></param>
   public MathExpressionNode(IReadOnlyList<Token> tokens) : base(tokens.Count > 0 ? tokens[0].Start : 0,
                                                                 tokens.Count > 0
                                                                    ? tokens[^1].Start +
                                                                      tokens[^1].Length -
                                                                      tokens.Count >
                                                                      0
                                                                         ? tokens[0].Start
                                                                         : 0
                                                                    : 0) => Tokens = tokens;

   // For now, we just capture all the tokens inside @[ ... ]
   public IReadOnlyList<Token> Tokens { get; }
   public override (int, int) GetLocation() => Tokens.Count > 0 ? (Tokens[0].Line, Tokens[0].Column) : (0, 0);
   public override (int line, int charPos) GetEndLocation() => Tokens.Count > 0 ? GetTokenEnd(Tokens[^1]) : (0, 0);
}

/// <summary>
///    A function call: `rgb { 255 0 0 }`
/// </summary>
/// <param name = "functionName" ></param>
public class FunctionCallNode(Token functionName) : ValueNode(functionName.Start, functionName.Length)
{
   public Token FunctionName { get; } = functionName; // e.g., 'rgb' or 'hsv' 'hsv360'
   public List<ValueNode> Arguments { get; } = [];
   public override (int, int) GetLocation() => (FunctionName.Line, FunctionName.Column);

   public override (int line, int charPos) GetEndLocation() => Arguments.Count > 0 ? Arguments.Last().GetEndLocation() : GetTokenEnd(FunctionName);
}

/// <summary>
///    A value that is itself an anonymous block, e.g., in `background = { key = value }`
/// </summary>
public class BlockValueNode(int start, int length) : ValueNode(start, length)
{
   public BlockValueNode() : this(0, 0)
   {
   }

   /// <summary>
   ///    A value that is itself an anonymous block, e.g., in `background = { key = value }`
   /// </summary>
   public BlockValueNode(Token openingToken) : this(openingToken.Start, openingToken.Length) => OpeningToken = openingToken;

   public Token? OpeningToken { get; }
   public Token? ClosingToken { get; set; }

   public string? ClosingComment { get; set; }

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
///    Represents a scripted statement like `scripted_trigger name = { ... }`
/// </summary>
public class ScriptedStatementNode : StatementNode
{
   public Token? ClosingToken;

   /// <summary>
   ///    Represents a scripted statement like `scripted_trigger name = { ... }`
   /// </summary>
   public ScriptedStatementNode(Token keyword, Token name, int start, int length) : base(start, length)
   {
      Name = name;
      KeyNode = new SimpleKeyNode(keyword);
   }

   public Token Name { get; } // The identifier for the defined scripted token
   public List<StatementNode> Children { get; } = []; // The content inside the braces
   public override (int, int) GetLocation() => KeyNode.GetLocation();

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
///    Represents a statement that is just a key with no value, common in lists.
///    e.g., the "stockholm" in `own_control_core = { stockholm norrtalje ... }`
/// </summary>
public class KeyOnlyNode : StatementNode
{
   public KeyOnlyNode(KeyNodeBase keyNode) : base(keyNode.Start, keyNode.Length) => KeyNode = keyNode;

   public KeyOnlyNode(string key) => KeyNode = new SimpleKeyNode(key);

   public override (int, int) GetLocation() => KeyNode.GetLocation();
   public override (int line, int charPos) GetEndLocation() => KeyNode.GetEndLocation();
}

/// <summary>
///    Represents a unary expression, like a negative number.
///    e.g., the "-10" in `offset = -10`
/// </summary>
public class UnaryNode(Token op, ValueNode value) : ValueNode(op.Start, value.End - op.Start)
{
   public Token Operator { get; } = op; // The operator token (e.g., '-')
   public ValueNode Value { get; } = value; // The value being operated on
   public override (int, int) GetLocation() => (Operator.Line, Operator.Column);
   public override (int line, int charPos) GetEndLocation() => Value.GetEndLocation();
}

/// <summary>
///    Represents a statement that is just a unary expression, like a negative number in a list.
///    e.g., the "-10" in `movement_assistance = { -10 20 }`
/// </summary>
public class UnaryStatementNode : StatementNode
{
   public UnaryStatementNode(UnaryNode value) : base(value.Start, value.Length)
   {
      Value = value;
      KeyNode = new SimpleKeyNode(value.Operator);
   }

   public UnaryNode Value { get; }

   public override (int, int) GetLocation() => Value.GetLocation();
   public override (int line, int charPos) GetEndLocation() => Value.GetEndLocation();
}