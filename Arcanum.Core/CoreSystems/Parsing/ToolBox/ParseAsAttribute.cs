namespace Arcanum.Core.CoreSystems.Parsing.ToolBox;

public enum AstNodeType
{
   ContentNode,
   BlockNode,
   KeyOnlyNode,
}

[AttributeUsage(AttributeTargets.Property)]
public class ParseAsAttribute(AstNodeType nodeType, string? key, string? customParser = null) : Attribute
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
}