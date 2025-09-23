namespace Arcanum.Core.CoreSystems.Parsing.NodeParser.Parser;

public class NodeArena
{
   private readonly List<AstNode> _nodes;

   public NodeArena(int initialCapacity) // Start with a reasonable capacity
   {
      _nodes = new(initialCapacity);
   }

   public NodeArena(List<AstNode> nodes)
   {
      _nodes = nodes;
   }

   public RootNode AllocateRootNode()
   {
      var rootNode = new RootNode();
      _nodes.Add(rootNode);
      return rootNode;
   }

   // Add overloads for nodes that have constructor parameters
   public ContentNode AllocateContentNode(Token key, Token separator, ValueNode value)
   {
      var node = new ContentNode(key, separator, value);
      _nodes.Add(node);
      return node;
   }

   public BlockNode AllocateBlockNode(Token identifier)
   {
      var node = new BlockNode(identifier);
      _nodes.Add(node);
      return node;
   }

   public KeyOnlyNode AllocateKeyOnlyNode(Token key)
   {
      var node = new KeyOnlyNode(key);
      _nodes.Add(node);
      return node;
   }

   public LiteralValueNode AllocateLiteralValueNode(Token token)
   {
      var node = new LiteralValueNode(token);
      _nodes.Add(node);
      return node;
   }

   public MathExpressionNode AllocateMathExpressionNode(List<Token> tokens)
   {
      var node = new MathExpressionNode(tokens);
      _nodes.Add(node);
      return node;
   }

   public FunctionCallNode AllocateFunctionCallNode(Token functionName)
   {
      var node = new FunctionCallNode(functionName);
      _nodes.Add(node);
      return node;
   }

   public ScriptedStatementNode AllocateScriptedStatementNode(Token keyword, Token param)
   {
      var node = new ScriptedStatementNode(keyword, param);
      _nodes.Add(node);
      return node;
   }

   public ValueNode AllocateUnaryNode(Token op, ValueNode right)
   {
      var node = new UnaryNode(op, right);
      _nodes.Add(node);
      return node;
   }

   public BlockValueNode AllocateBlockValueNode()
   {
      var node = new BlockValueNode();
      _nodes.Add(node);
      return node;
   }
}