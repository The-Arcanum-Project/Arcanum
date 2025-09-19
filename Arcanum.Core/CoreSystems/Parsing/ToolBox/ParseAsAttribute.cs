namespace Arcanum.Core.CoreSystems.Parsing.ToolBox;

public enum AstNodeType
{
   ContentNode,
   BlockNode,
}

[AttributeUsage(AttributeTargets.Property)]
public class ParseAsAttribute(string? key,
                              AstNodeType nodeType = AstNodeType.ContentNode,
                              string? customParser = null,
                              bool isContentNodeList = false) : Attribute
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
   /// If true, several <see cref="AstNodeType.ContentNode"/> of this type are expected and will be parsed into a list
   /// </summary>
   public bool IsContentNodeList { get; set; } = isContentNodeList;
}