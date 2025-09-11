namespace Arcanum.Core.CoreSystems.Parsing.ToolBox;

/// <summary>
/// Marks a partial class as a parser that should be completed by the source generator.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class ParserForAttribute : Attribute
{
   public Type TargetType { get; }

   /// <param name="targetType">The data model class (e.g., typeof(ToolBoxObj)) that this parser is for.</param>
   public ParserForAttribute(Type targetType)
   {
      TargetType = targetType;
   }
}