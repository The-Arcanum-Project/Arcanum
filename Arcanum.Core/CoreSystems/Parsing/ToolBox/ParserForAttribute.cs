namespace Arcanum.Core.CoreSystems.Parsing.ToolBox;

/// <summary>
/// Marks a partial class as a parser that should be completed by the source generator.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class ParserForAttribute : Attribute
{
   public bool AllowUnknownNodes { get; set; }
   public Type TargetType { get; }
   /// <summary>
   /// if a block node is encountered with one of these keys and AllowUnknownNodes is false, it will be ignored instead of throwing an exception.
   /// </summary>
   public string[] IgnoredBlockKeys { get; set; }

   /// <summary>
   /// if a content node is encountered with one of these keys and AllowUnknownNodes is false, it will be ignored instead of throwing an exception.
   /// </summary>
   public string[] IgnoredContentKeys { get; set; }

   public bool ContainsOnlyChildObjects { get; set; } = false;
   public string? ChildObjectList { get; set; } = null;

   /// <param name="targetType">The data model class (e.g., typeof(ToolBoxObj)) that this parser is for.</param>
   /// <param name="allowUnknownNodes"></param>
   /// <param name="ignoredBlockKeys"></param>
   /// <param name="ignoredContentKeys"></param>
   /// <param name="containsOnlyChildObjects"></param>
   /// <param name="childObjectList"></param>
   public ParserForAttribute(Type targetType,
                             bool allowUnknownNodes = false,
                             string[]? ignoredBlockKeys = null,
                             string[]? ignoredContentKeys = null,
                             bool containsOnlyChildObjects = false,
                             string? childObjectList = null)
   {
      TargetType = targetType;
      AllowUnknownNodes = allowUnknownNodes;
      IgnoredBlockKeys = ignoredBlockKeys ?? [];
      IgnoredContentKeys = ignoredContentKeys ?? [];
      ContainsOnlyChildObjects = containsOnlyChildObjects;
      ChildObjectList = childObjectList;
   }
}