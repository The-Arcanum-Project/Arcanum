namespace Arcanum.Core.CoreSystems.Parsing.ToolBox;

public enum AstNodeType
{
   ContentNode,
   BlockNode,
   KeyOnlyNode,
}

[AttributeUsage(AttributeTargets.Property)]
public class ParseAsAttribute(string? key,
                              AstNodeType nodeType = AstNodeType.ContentNode,
                              string? customParser = null,
                              bool isShatteredList = false,
                              AstNodeType itemNodeType = AstNodeType.KeyOnlyNode) : Attribute
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

   /// <summary>
   /// If we are parsing a list of items, this specifies the node type of each item in the list.
   /// </summary>
   public AstNodeType ItemNodeType { get; set; } = itemNodeType;
}