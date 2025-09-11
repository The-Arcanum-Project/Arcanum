namespace Arcanum.Core.CoreSystems.Parsing.ToolBox;

public enum AstNodeType
{
   ContentNode,
   BlockNode,
   KeyOnlyNode,
}

[AttributeUsage(AttributeTargets.Property)]
public class ParseAsAttribute(AstNodeType nodeType, string? key) : Attribute
{
   public AstNodeType NodeType { get; } = nodeType;

   public string? Key { get; } = key;
}