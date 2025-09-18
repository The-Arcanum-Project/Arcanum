using System.Diagnostics.CodeAnalysis;
using Arcanum.Core.CoreSystems.Common;
using Arcanum.Core.CoreSystems.ErrorSystem.BaseErrorTypes;
using Arcanum.Core.CoreSystems.ErrorSystem.Diagnostics;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.Parser;

namespace Arcanum.Core.CoreSystems.Parsing.NodeParser.NodeHelpers;

public static class VnHelper
{
   /// <summary>
   /// Returns a string containing the literal value, including a leading minus sign if the value is negative.
   /// If the node is not a literal value or an optional unary minus, logs a warning and returns false.
   /// </summary>
   /// <param name="node"></param>
   /// <param name="ctx"></param>
   /// <param name="actionName"></param>
   /// <param name="source"></param>
   /// <param name="validation"></param>
   /// <param name="value"></param>
   /// <param name="lvn"></param>
   /// <returns></returns>
   public static bool IsLiteralValueNodeOptionalUnary(this ValueNode node,
                                                      LocationContext ctx,
                                                      string actionName,
                                                      string source,
                                                      ref bool validation,
                                                      [MaybeNullWhen(false)] out string value,
                                                      [MaybeNullWhen(false)] out LiteralValueNode lvn)
   {
      if (node is UnaryNode { Value: LiteralValueNode lvn1, Operator.Type: TokenType.Minus } unary)
      {
         var unLoc = unary.GetLocation();
         value = lvn1.Value.GetLexeme(source, unLoc.Item2);
         lvn = lvn1;
         return true;
      }

      if (node.IsLiteralValueNode(ctx, actionName, ref validation, out lvn))
      {
         value = lvn.Value.GetLexeme(source);
         return true;
      }

      value = null;
      lvn = null;
      validation = false;
      return false;
   }

   public static bool IsLiteralValueNode(this ValueNode node,
                                         LocationContext ctx,
                                         string actionName,
                                         [MaybeNullWhen(false)] out LiteralValueNode value)
   {
      if (node is not LiteralValueNode lvn)
      {
         ctx.SetPosition(node);
         DiagnosticException.LogWarning(ctx.GetInstance(),
                                        ParsingError.Instance.InvalidNodeType,
                                        actionName,
                                        node.GetType().Name,
                                        nameof(LiteralValueNode),
                                        "N/A");
         value = null!;
         return false;
      }

      value = lvn;
      return true;
   }

   public static bool IsLiteralValueNode(this ValueNode node,
                                         LocationContext ctx,
                                         string actionName,
                                         ref bool validationResult,
                                         [MaybeNullWhen(false)] out LiteralValueNode value)
   {
      var returnVal = node.IsLiteralValueNode(ctx, actionName + ".ValueNode.IsLiteralValueNode", out value);
      validationResult &= returnVal;
      return returnVal;
   }
}