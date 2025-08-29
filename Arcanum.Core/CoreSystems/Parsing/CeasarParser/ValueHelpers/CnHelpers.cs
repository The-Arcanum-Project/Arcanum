using Arcanum.Core.CoreSystems.Common;
using Arcanum.Core.CoreSystems.ErrorSystem.BaseErrorTypes;
using Arcanum.Core.CoreSystems.ErrorSystem.Diagnostics;

namespace Arcanum.Core.CoreSystems.Parsing.CeasarParser.ValueHelpers;

public static class CnHelpers
{
   public static bool TryGetStringContentNode(this ContentNode node,
                                              LocationContext ctx,
                                              string actionName,
                                              string source,
                                              out string value)
   {
      if (!SeparatorHelper.IsSeparatorOfType(node.Separator, TokenType.Equals, ctx, actionName))
      {
         value = string.Empty;
         return false;
      }

      if (node.Value is not LiteralValueNode lvn)
      {
         DiagnosticException.LogWarning(ctx.GetInstance(),
                                        ParsingError.Instance.InvalidNodeType,
                                        actionName,
                                        node.Value.GetType().Name,
                                        nameof(LiteralValueNode));
         value = string.Empty;
         return false;
      }

      value = lvn.Value.GetLexeme(source);
      return true;
   }
}