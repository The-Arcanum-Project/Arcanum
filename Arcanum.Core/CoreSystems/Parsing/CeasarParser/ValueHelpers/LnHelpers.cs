using Arcanum.Core.CoreSystems.Common;
using Arcanum.Core.CoreSystems.ErrorSystem.BaseErrorTypes;
using Arcanum.Core.CoreSystems.ErrorSystem.Diagnostics;

namespace Arcanum.Core.CoreSystems.Parsing.CeasarParser.ValueHelpers;

public static class LnHelpers
{
   public static bool IsLiteralValueNode(this ValueNode node, LocationContext ctx, string actionName, out LiteralValueNode? value)
   {
      if (node is not LiteralValueNode lvn)
      {
         DiagnosticException.LogWarning(ctx.GetInstance(),
                                        ParsingError.Instance.InvalidNodeType,
                                        actionName,
                                        node.GetType().Name,
                                        nameof(LiteralValueNode));
         value = null!;
         return false;
      }
      value = lvn;
      return true;
   }
}