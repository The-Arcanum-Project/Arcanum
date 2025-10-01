namespace Arcanum.Core.CoreSystems.Parsing.NodeParser.ToolBox;

public enum AstNodeType
{
   ContentNode,
   BlockNode,
   KeyOnlyNode,
   StatementNode,
}

/// <summary>
/// Defines how a parser is being build. <br/>
/// By default we look for a content node but Collections and embedded objects are defaulted to block nodes.
/// </summary>
/// <param name="key"></param>
/// <param name="nodeType"></param>
/// <param name="customParser"></param>
/// <param name="isShatteredList"></param>
/// <param name="itemNodeType"></param>
/// <param name="isEmbedded"></param>
[AttributeUsage(AttributeTargets.Property)]
public class ParseAsAttribute(string? key,
                              AstNodeType nodeType = AstNodeType.ContentNode,
                              string? customParser = null,
                              bool isShatteredList = false,
                              AstNodeType itemNodeType = AstNodeType.KeyOnlyNode,
                              bool isEmbedded = false,
                              Type? iEu5KeyType = null) : Attribute
{
   public AstNodeType NodeType { get; } = nodeType;

   /// <summary>
   /// The key to look for in the AST node. If null, the property name will be used as the key.
   /// </summary>
   public string? Key { get; } = key;
   /// <summary>
   /// Optional. Specifies the name of a custom, handwritten static method in the
   /// parser class to use for parsing this property, instead of an auto-generated one.
   /// The method must match the required parser delegate signature.
   /// </summary>
   public string? CustomParser { get; set; } = customParser;

   /// <summary>
   /// If true, several items of this type are expected, but they are not wrapped in a parent node.
   /// Instead, all nodes with the same key will be parsed into a list.
   /// </summary>
   public bool IsShatteredList { get; set; } = isShatteredList;

   public Type? IEu5KeyType { get; set; } = iEu5KeyType;

   /// <summary>
   /// If we are parsing a list of items, this specifies the node type of each item in the list.
   /// </summary>
   public AstNodeType ItemNodeType { get; set; } = itemNodeType;

   /// <summary>
   /// If true, the property is an embedded object which has it's own parser and is wrapped by the key.
   /// </summary>
   public bool IsEmbedded { get; set; } = isEmbedded;
}