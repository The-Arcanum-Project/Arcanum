namespace Arcanum.Core.CoreSystems.Parsing.ToolBox;

/// <summary>
/// Marks a property as an embedded object that should be parsed recursively.
/// The property's type must also have a generated parser helper.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class ParseAsEmbeddedAttribute(string key) : Attribute
{
   public string Key { get; } = key;
}