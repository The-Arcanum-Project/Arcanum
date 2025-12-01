using Arcanum.Core.CoreSystems.ErrorSystem.BaseErrorTypes;
using Arcanum.Core.CoreSystems.ErrorSystem.Diagnostics;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.Parser;

namespace Arcanum.Core.CoreSystems.Parsing.NodeParser.NodeHelpers;

public static class UsnHelper
{
   public static bool TryParseFloatValue(this UnaryStatementNode node,
                                         ref ParsingContext pc,
                                         out float value)
   {
      using var scope = pc.PushScope();
      if (!node.Value.IsLiteralValueNodeOptionalUnary(ref pc, out var lexeme, out _))
      {
         value = 0f;
         return pc.Fail();
      }

      if (float.TryParse(lexeme, out value))
         return true;

      pc.SetContext(node);
      DiagnosticException.LogWarning(ref pc,
                                     ParsingError.Instance.FloatParsingError,
                                     lexeme);
      value = 0f;
      return pc.Fail();
   }
}