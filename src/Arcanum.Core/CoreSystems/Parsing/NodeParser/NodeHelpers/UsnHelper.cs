using Arcanum.Core.CoreSystems.Common;
using Arcanum.Core.CoreSystems.ErrorSystem.BaseErrorTypes;
using Arcanum.Core.CoreSystems.ErrorSystem.Diagnostics;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.Parser;

namespace Arcanum.Core.CoreSystems.Parsing.NodeParser.NodeHelpers;

public static class UsnHelper
{
   public static bool TryParseFloatValue(this UnaryStatementNode node,
                                         LocationContext ctx,
                                         string source,
                                         string actionName,
                                         ref bool validation,
                                         out float value)
   {
      if (!node.Value.IsLiteralValueNodeOptionalUnary(ctx, actionName, source, ref validation, out var lexeme, out _))
      {
         validation = false;
         value = 0f;
         return false;
      }

      if (float.TryParse(lexeme, out value))
         return true;

      ctx.SetPosition(node.KeyNode);
      DiagnosticException.LogWarning(ctx,
                                     ParsingError.Instance.FloatParsingError,
                                     actionName,
                                     lexeme);
      validation = false;
      value = 0f;
      return false;
   }
}