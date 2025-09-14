using Arcanum.Core.CoreSystems.Common;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.Parser;

namespace Arcanum.Core.CoreSystems.SavingSystem.AGS;

public class PropertySavingMetaData
{
   public required string Keyword { get; init; }
   public TokenType Separator { get; init; } = TokenType.Equals;
   public required Enum NxProp { get; init; }
   public SavingValueType ValueType { get; init; }
   public required Func<IAgs, Enum, string>? CommentProvider { get; set; }
   public required Action<IAgs, PropertySavingMetaData, IndentedStringBuilder>? SavingMethod { get; set; }
}